using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Nixill.Collections.Grid.CSV;
using Nixill.Utils;

namespace Nixill.GTFS.Parsing {
  internal struct GTFSColumn {
    internal string Name;
    internal GTFSDataType Type;
    internal bool Required;
    internal string ColumnDef;
    internal bool PrimaryKey;

    internal GTFSColumn(string name, GTFSDataType type, string columnDef, bool required = false, bool primaryKey = false) {
      Name = name;
      Type = type;
      Required = required;
      ColumnDef = columnDef;
      PrimaryKey = primaryKey;
    }
  }

  internal static class GTFSMaker {
    internal static IEnumerable<char> ZipEntryCharIterator(ZipArchiveEntry ent) {
      return FileUtils.StreamCharEnumerator(new StreamReader(ent.Open()));
    }

    internal static bool CreateTable(GTFSFile file, ZipArchive zip, List<GTFSWarning> warnings,
      string tableName, bool required, List<GTFSColumn> columns, bool agencyIdColumn = false,
      string virtualEntityTable = null, GTFSColumn? virtualEntityColumn = null,
      string primaryKey = null, List<string> parentTables = null) {
      // Long method signature. Here's the start of the code.
      // If there are any parent tables, we should make sure those exist first.
      HashSet<string> files = file.Files;

      if (parentTables != null) {
        foreach (string parent in parentTables) {
          if (!files.Contains(parent)) {
            return false;
          }
        }
      }

      // Let's open a transaction and make a command.
      SqliteConnection conn = file.Conn;
      SqliteTransaction trans = conn.BeginTransaction();
      SqliteCommand cmd = conn.CreateCommand();
      SqliteCommand veCmd = conn.CreateCommand();

      // First, if there's a virtual entity table, we need to make that first.
      string virtualEntityColumnName = null;
      if (virtualEntityTable != null) {
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + virtualEntityTable + " ( " + virtualEntityColumn.Value.Name + " TEXT PRIMARY KEY NOT NULL);";
        cmd.ExecuteNonQuery();
        cmd.Dispose();
        // This is at the bottom because we're replacing it.
        cmd = conn.CreateCommand();
        // And we'll save the name just to make things easier.
        virtualEntityColumnName = virtualEntityColumn.Value.Name;
        // Also let's not forget to populate the table!
        veCmd.CommandText = "INSERT OR IGNORE INTO " + virtualEntityTable + " VALUES (@p);";
      }

      // Now we need to figure out the main table creation command. While
      // we're at it, let's also build the list of required columns, and
      // the dictionary of allowed columns, that the method will use later
      // on.
      StringBuilder builder = new StringBuilder("CREATE TABLE " + tableName + " (");
      bool firstCol = true;
      HashSet<string> requiredCols = new HashSet<string>();
      Dictionary<string, GTFSColumn> allowedCols = new Dictionary<string, GTFSColumn>();

      foreach (GTFSColumn col in columns) {
        // Add a comma iff we're adding more han one column.
        if (!firstCol) {
          builder.Append(",");
        }
        else {
          firstCol = false;
        }

        // Now add the new line and indent
        builder.Append("\n  ");

        // And lastly the column definition
        builder.Append(col.Name + " " + col.ColumnDef);

        // Add the column to the allowedCols dict
        allowedCols.Add(col.Name, col);

        // And if it's required, add it to the requiredCols set
        if (col.Required) {
          requiredCols.Add(col.Name);
        }
      }

      // If we have a separate primary key definition, we'll need to add that too
      if (primaryKey != null) {
        builder.Append(",\n  PRIMARY KEY (" + primaryKey + ")");
      }

      builder.Append("\n);");
      cmd.CommandText = builder.ToString();

      // And actually make it
      cmd.ExecuteNonQuery();
      cmd.Dispose();

      GTFSTriggers.CreateTriggers(tableName, conn);

      // Are we populating this table?
      bool populated = false;

      // Now let's parse the actual table.
      ZipArchiveEntry tableFile = zip.GetEntry(tableName + ".txt");

      // Table warnings
      List<GTFSWarning> tableWarns = new List<GTFSWarning>();

      // If it's a required table, throw an error if it's not found.
      if (tableFile == null) {
        if (required) {
          throw new GTFSParseException(tableName + ".txt is a required file within GTFS.");
        }
        else {
          warnings.Add(new GTFSWarning("Optional table not included in GTFS.") {
            Table = tableName
          });
        }
      }
      else {
        // If we have the file, let's parse the table!
        // we do something different on the first row
        int rows = 0;
        List<GTFSColumn?> usedCols = new List<GTFSColumn?>();
        List<string> usedNames = new List<string>();

        // Iterate the rows
        foreach (IList<string> row in CSVParser.EnumerableToRows(ZipEntryCharIterator(tableFile))) {
          if (rows == 0) {
            IList<string> header = row;

            // Check the list of columns against the required ones.
            foreach (string col in header) {
              if (requiredCols.Contains(col)) requiredCols.Remove(col);

              if (allowedCols.ContainsKey(col)) {
                usedCols.Add(allowedCols[col]);
                allowedCols.Remove(col);
                usedNames.Add(col);
              }

              else {
                usedCols.Add(null);
              }
            }

            // If we don't have all the required columns, we need to stop.
            if (requiredCols.Count != 0) {
              // Garbage collection
              cmd.Dispose();
              veCmd.Dispose();
              trans.Commit();
              trans.Dispose();

              // Throw an error if the table is required.
              if (required) {
                throw new GTFSParseException("Required table " + tableName + ".txt is missing required column(s) " + string.Join(", ", requiredCols));
              }

              // Otherwise just log the warning and quit parsing this table.
              else {
                foreach (string col in requiredCols) {
                  warnings.Add(new GTFSWarning("Required column missing.") {
                    Table = tableName,
                    Field = col
                  });
                }
                warnings.Add(new GTFSWarning("Couldn't import due to missing required columns.") {
                  Table = tableName
                });
                return false;
              }
            }

            // Let's also prepare the command for populating the tables.
            cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO " + tableName + " ("
              + ((allowedCols.ContainsKey("agency_id") && agencyIdColumn) ? "agency_id, " : "")
              + string.Join(", ", usedNames) + ")\n"
              + "VALUES ("
              + ((allowedCols.ContainsKey("agency_id") && agencyIdColumn) ? "'agency', @p" : "@p")
              + String.Join(", @p", Enumerable.Range(0, usedNames.Count))
              + ");";
          }
          else {
            int c = 0;
            cmd.Parameters.Clear();
            List<GTFSWarning> rowWarns = new List<GTFSWarning>();
            bool skipRow = false;
            string pimaryKey = "Row " + rows;
            bool pkPopulated = false;

            // Iterate through the columns
            for (int i = 0; i < row.Count && i < usedCols.Count; i++) {
              // Skip unrecognized columns
              if (usedCols[i] == null) continue;

              // Let's get the parameter value
              GTFSColumn col = usedCols[i].Value;
              List<GTFSWarning> warns = new List<GTFSWarning>();
              object param = GTFSObjectParser.GetObject(col.Type, row[i], ref warns);

              // Add the parameter to the command
              cmd.Parameters.AddWithValue("@p" + c, param ?? DBNull.Value);

              // If there are any warnings, add them to the row list.
              foreach (GTFSWarning warn in warns) {
                warn.Field = col.Name;
                warn.Table = tableName;
                rowWarns.Add(warn);
              }

              // If there are any required columns that have a null, let's drop the row.
              if (param == null && col.Required) {
                skipRow = true;
                rowWarns.Add(new GTFSWarning("Required column with no valid value given.") {
                  Table = tableName,
                  Field = col.Name
                });
              }

              // Use a primary key to identify the row.
              if (param != null && col.PrimaryKey) {
                if (pkPopulated) {
                  primaryKey += " / " + row[i];
                }
                else {
                  primaryKey = row[i];
                }
              }

              // If this column is the virtual entity column, let's get its value ready too.
              if (col.Name == virtualEntityColumnName && param != null) {
                veCmd.Parameters.Clear();
                veCmd.Parameters.AddWithValue("@p", param);
                veCmd.Prepare();
                veCmd.ExecuteNonQuery();
              }

              // DON'T FORGET TO ACTUALLY INCREMENT THE USED COLUMN COUNTER YOU DUMBASS
              c++;
            }

            // Incorporate all the row's warnings into the table's warnings.
            foreach (GTFSWarning warn in rowWarns) {
              warn.Record = primaryKey;
              warnings.Add(warn);
            }

            // If we're not skipping the row, insert it now.
            if (!skipRow) {
              cmd.Prepare();
              try {
                cmd.ExecuteNonQuery();
                populated = true;
              }
              catch (SqliteException ex) {
                if (!ex.Message.StartsWith("TRIGGER - ")) {
                  warnings.Add(new GTFSWarning("SqliteException: " + ex) {
                    Table = tableName,
                    Record = primaryKey
                  });
                }
              }
            }
          }
          rows++;
        }
      }

      cmd.Dispose();
      veCmd.Dispose();

      // End the transaction too
      trans.Commit();
      trans.Dispose();

      // Lastly: Output whether or not the table has stuff in it.
      return populated;
    }

    internal static bool CreateFeedInfoTable(GTFSFile file, ZipArchive zip, List<GTFSWarning> warnings) {
      return CreateTable(
        file: file, zip: zip, warnings: warnings,
        tableName: "feed_info", required: false,
        columns: new List<GTFSColumn> {
          new GTFSColumn("feed_publisher_name", GTFSDataType.Text, "TEXT NOT NULL", true),
          new GTFSColumn("feed_publisher_url", GTFSDataType.Text, "TEXT"),
          new GTFSColumn("feed_lang", GTFSDataType.Language, "TEXT NOT NULL", true),
          new GTFSColumn("default_lang", GTFSDataType.Language, "TEXT"),
          new GTFSColumn("feed_start_date", GTFSDataType.Date, "TEXT"),
          new GTFSColumn("feed_end_date", GTFSDataType.Date, "TEXT"),
          new GTFSColumn("feed_version", GTFSDataType.Text, "TEXT"),
          new GTFSColumn("feed_contact_email", GTFSDataType.Email, "TEXT"),
          new GTFSColumn("feed_contact_url", GTFSDataType.Url, "TEXT")
        });
    }

    internal static bool CreateAgencyTable(GTFSFile file, ZipArchive zip, List<GTFSWarning> warnings) {
      if (!CreateTable(
        file: file, zip: zip, warnings: warnings,
        tableName: "agency", required: true,
        columns: new List<GTFSColumn> {
          new GTFSColumn("agency_id", GTFSDataType.ID, "TEXT PRIMARY KEY NOT NULL", true, true),
          new GTFSColumn("agency_name", GTFSDataType.Text, "TEXT NOT NULL", true),
          new GTFSColumn("agency_url", GTFSDataType.Url, "TEXT"),
          new GTFSColumn("agency_timezone", GTFSDataType.Timezone, "TEXT"),
          new GTFSColumn("agency_lang", GTFSDataType.Language, "TEXT"),
          new GTFSColumn("agency_fare_url", GTFSDataType.Url, "TEXT"),
          new GTFSColumn("agency_phone", GTFSDataType.Phone, "TEXT"),
          new GTFSColumn("agency_email", GTFSDataType.Email, "TEXT")
        }, agencyIdColumn: true
      )) {
        throw new GTFSParseException("No agencies were specified.");
      }

      GTFSValidation.ValidateAgency(file.Conn, warnings);

      return true;
    }

    internal static bool CreateRoutesTable(GTFSFile file, ZipArchive zip, List<GTFSWarning> warnings) {
      if (!(CreateTable(file: file, zip: zip, warnings: warnings,
      tableName: "routes", required: true, agencyIdColumn: true,
      columns: new List<GTFSColumn> {
        new GTFSColumn("route_id", GTFSDataType.ID, "TEXT PRIMARY KEY NOT NULL", true, true),
        new GTFSColumn("agency_id", GTFSDataType.ID, "TEXT NOT NULL REFERENCES agency"),
        new GTFSColumn("route_short_name", GTFSDataType.Text, "TEXT"),
        new GTFSColumn("route_long_name", GTFSDataType.Text, "TEXT"),
        new GTFSColumn("route_desc", GTFSDataType.Text, "TEXT"),
        new GTFSColumn("route_type", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_route_types", true),
        new GTFSColumn("route_url", GTFSDataType.Url, "TEXT"),
        new GTFSColumn("route_color", GTFSDataType.Color, "TEXT"),
        new GTFSColumn("route_text_color", GTFSDataType.Color, "TEXT"),
        new GTFSColumn("route_sort_order", GTFSDataType.NonNegativeInteger, "INTEGER"),
        new GTFSColumn("continuous_pickup", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_pickup_drop_off DEFAULT 1"),
        new GTFSColumn("continuous_drop_off", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_pickup_drop_off DEFAULT 1")
      }))) {
        throw new GTFSParseException("No routes were specified.");
      }

      SqliteCommand cmd = file.Conn.CreateCommand();
      cmd.CommandText = "SELECT route_id FROM routes WHERE route_short_name IS NULL AND route_long_name IS NULL;";
      SqliteDataReader reader = cmd.ExecuteReader();
      while (reader.Read()) {
        warnings.Add(new GTFSWarning("No name specified (removed from table).") {
          Table = "routes",
          Record = GTFSObjectParser.GetID(reader["route_id"])
        });
      }
      cmd.Dispose();

      cmd = file.Conn.CreateCommand();
      cmd.CommandText = "DELETE FROM routes WHERE route_short_name IS NULL AND route_long_name IS NULL;";
      cmd.ExecuteNonQuery();
      cmd.Dispose();

      // Now let's make sure we still have at least one route.
      cmd = file.Conn.CreateCommand();
      cmd.CommandText = "SELECT route_id FROM routes;";
      if (!cmd.ExecuteReader().HasRows) {
        throw new GTFSParseException("No routes were specified that had either a route_short_name or route_long_name.");
      }
      cmd.Dispose();

      // Lastly, return true.
      return true;
    }

    internal static bool CreateLevelsTable(GTFSFile file, ZipArchive zip, List<GTFSWarning> warnings) {
      return CreateTable(file: file, zip: zip, warnings: warnings,
        tableName: "levels", required: false, columns: new List<GTFSColumn>() {
          new GTFSColumn("level_id", GTFSDataType.ID, "TEXT NOT NULL PRIMARY KEY", true, true),
          new GTFSColumn("level_index", GTFSDataType.Float, "REAL NOT NULL", true),
          new GTFSColumn("level_name", GTFSDataType.Text, "TEXT")
        });
    }

    internal static bool CreateStopsTable(GTFSFile file, ZipArchive zip, List<GTFSWarning> warnings) {
      return CreateTable(file: file, zip: zip, warnings: warnings,
        tableName: "stops", required: true, virtualEntityTable: "fare_zones",
        virtualEntityColumn: new GTFSColumn("zone_id", GTFSDataType.ID, "TEXT PRIMARY KEY NOT NULL"),
        columns: new List<GTFSColumn>() {
          new GTFSColumn("stop_id", GTFSDataType.ID, "TEXT PRIMARY KEY NOT NULL", true, true),
          new GTFSColumn("stop_code", GTFSDataType.Text, "TEXT"),
          new GTFSColumn("stop_name", GTFSDataType.Text, "TEXT"),
          new GTFSColumn("stop_desc", GTFSDataType.Text, "TEXT"),
          new GTFSColumn("stop_lat", GTFSDataType.Latitude, "REAL"),
          new GTFSColumn("stop_lon", GTFSDataType.Longitude, "REAL"),
          new GTFSColumn("zone_id", GTFSDataType.ID, "TEXT REFERENCES fare_zones"),
          new GTFSColumn("stop_url", GTFSDataType.Url, "TEXT"),
          new GTFSColumn("location_type", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_location_type DEFAULT 0"),
          new GTFSColumn("parent_station", GTFSDataType.ID, "TEXT REFERENCES stops"),
          new GTFSColumn("stop_timezone", GTFSDataType.Timezone, "TEXT"),
          new GTFSColumn("wheelchair_boarding", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_tristate DEFAULT 0"),
          new GTFSColumn("level_id", GTFSDataType.ID, "TEXT REFERENCES levels"),
          new GTFSColumn("platform_code", GTFSDataType.Text, "TEXT")
        });
    }

    internal static bool CreateShapesTable(GTFSFile file, ZipArchive zip, List<GTFSWarning> warnings) {
      return CreateTable(file: file, zip: zip, warnings: warnings,
        tableName: "shapes", required: false, virtualEntityTable: "shape_ids",
        virtualEntityColumn: new GTFSColumn("shape_id", GTFSDataType.ID, ""),
        columns: new List<GTFSColumn>() {
          new GTFSColumn("shape_id", GTFSDataType.ID, "TEXT NOT NULL REFERENCES shape_ids", true, true),
          new GTFSColumn("shape_pt_sequence", GTFSDataType.ID, "INTEGER NOT NULL", true, true),
          new GTFSColumn("shape_pt_lat", GTFSDataType.Latitude, "REAL NOT NULL", true),
          new GTFSColumn("shape_pt_lon", GTFSDataType.Longitude, "REAL NOT NULL", true),
          new GTFSColumn("shape_dist_traveled", GTFSDataType.NonNegativeFloat, "REAL")
        }, primaryKey: "shape_id, shape_pt_lat");
    }

    internal static bool CreateCalendarTable(GTFSFile file, ZipArchive zip, List<GTFSWarning> warnings) {
      return CreateTable(file: file, zip: zip, warnings: warnings,
        tableName: "calendar", required: false, virtualEntityTable: "calendar_services",
        virtualEntityColumn: new GTFSColumn("service_id", GTFSDataType.ID, ""),
        columns: new List<GTFSColumn>() {
          new GTFSColumn("service_id", GTFSDataType.ID, "TEXT PRIMARY KEY NOT NULL REFERENCES calendar_services", true, true),
          new GTFSColumn("start_date", GTFSDataType.Date, "TEXT NOT NULL", true),
          new GTFSColumn("end_date", GTFSDataType.Date, "TEXT NOT NULL", true),
          new GTFSColumn("monday", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_boolean", true),
          new GTFSColumn("tuesday", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_boolean", true),
          new GTFSColumn("wednesday", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_boolean", true),
          new GTFSColumn("thursday", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_boolean", true),
          new GTFSColumn("friday", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_boolean", true),
          new GTFSColumn("saturday", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_boolean", true),
          new GTFSColumn("sunday", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_boolean", true),
        }
      );
    }

    internal static bool CreateCalendarDatesTable(GTFSFile file, ZipArchive zip, List<GTFSWarning> warnings) {
      return CreateTable(file: file, zip: zip, warnings: warnings,
        tableName: "calendar_dates", required: false, virtualEntityTable: "calendar_services",
        virtualEntityColumn: new GTFSColumn("service_id", GTFSDataType.ID, ""),
        columns: new List<GTFSColumn>() {
          new GTFSColumn("service_id", GTFSDataType.ID, "TEXT NOT NULL REFERENCES calendar_services", true, true),
          new GTFSColumn("date", GTFSDataType.Date, "TEXT NOT NULL", true, true),
          new GTFSColumn("exception_type", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_calendar_date", true)
        }, primaryKey: "service_id, date"
      );
    }

    internal static bool CreateFareAttributesTable(GTFSFile file, ZipArchive zip, List<GTFSWarning> warnings) {
      return CreateTable(file: file, zip: zip, warnings: warnings,
        tableName: "fare_attributes", required: false, agencyIdColumn: true,
        columns: new List<GTFSColumn>() {
          new GTFSColumn("fare_id", GTFSDataType.ID, "TEXT PRIMARY KEY NOT NULL", true, true),
          new GTFSColumn("price", GTFSDataType.NonNegativeFloat, "REAL NOT NULL", true),
          new GTFSColumn("currency_type", GTFSDataType.Currency, "TEXT NOT NULL", true),
          new GTFSColumn("payment_method", GTFSDataType.Enum, "INTEGER NOT NULL REFERENCES enum_boolean", true),
          new GTFSColumn("transfers", GTFSDataType.Integer, "INTEGER"),
          new GTFSColumn("agency_id", GTFSDataType.ID, "TEXT NOT NULL REFERENCES agency"),
          new GTFSColumn("transfer_duration", GTFSDataType.NonNegativeInteger, "INTEGER")
        }
      );
    }

    internal static bool CreateFareRulesTable(GTFSFile file, ZipArchive zip, List<GTFSWarning> warnings) {
      return CreateTable(file: file, zip: zip, warnings: warnings,
        tableName: "fare_rules", required: false, agencyIdColumn: false,
        columns: new List<GTFSColumn>() {
          new GTFSColumn("fare_id", GTFSDataType.ID, "TEXT NOT NULL REFERENCES fare_attributes", true),
          new GTFSColumn("route_id", GTFSDataType.ID, "TEXT REFERENCES routes"),
          new GTFSColumn("origin_id", GTFSDataType.ID, "TEXT REFERENCES fare_zones"),
          new GTFSColumn("destination_id", GTFSDataType.ID, "TEXT REFERENCES fare_zones"),
          new GTFSColumn("contains_id", GTFSDataType.ID, "TEXT REFERENCES fare_zones"),
          new GTFSColumn("contains_group_id", GTFSDataType.ID, "TEXT")
        }, parentTables: new List<string>() { "fare_attributes" }
      );
    }

    internal static void CreateWarningsTable(GTFSFile file, List<GTFSWarning> warnings) {
      SqliteCommand cmd = file.Conn.CreateCommand();
      cmd.CommandText = @"INSERT INTO gtfs_warnings (warn_message, warn_table, warn_field, warn_record) VALUES (@msg, @tbl, @fld, @rec);";

      foreach (GTFSWarning warn in warnings) {
        cmd.Parameters.AddWithValue("@msg", warn.Message);
        cmd.Parameters.AddWithValue("@tbl", (object)warn.Table ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@fld", (object)warn.Field ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@rec", (object)warn.Record ?? DBNull.Value);

        cmd.Prepare();

        cmd.ExecuteNonQuery();

        cmd.Parameters.Clear();
      }

      cmd.Dispose();
    }
  }
}