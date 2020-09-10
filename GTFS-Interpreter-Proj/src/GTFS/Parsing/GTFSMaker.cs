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

    internal GTFSColumn(string name, GTFSDataType type, string columnDef, bool required = false) {
      Name = name;
      Type = type;
      Required = required;
      ColumnDef = columnDef;
    }
  }

  internal enum GTFSDataType {
    Color, Currency, Date, Email, Enum, ID, Language, Latitude, Longitude, Float, NonNegativeFloat,
    PositiveFloat, Integer, NonNegativeInteger, PositiveInteger, Phone, Time, Text, Timezone, Url
  }

  internal static class GTFSMaker {
    internal static IEnumerable<char> ZipEntryCharIterator(ZipArchiveEntry ent) {
      return FileUtils.StreamCharEnumerator(new StreamReader(ent.Open()));
    }

    internal static void CreateTable(SqliteConnection conn, ZipArchive zip, GTFSWarnings warnings,
      string tableName, bool required, List<GTFSColumn> columns, bool agencyIdColumn = false,
      string virtualEntityTable = null, GTFSColumn? virtualEntityColumn = null,
      string primaryKey = null, List<string> parentTables = null) {
      // Long method signature. Here's the start of the code.
      // Let's open a transaction and make a command.
      conn.BeginTransaction();
      SqliteCommand cmd = conn.CreateCommand();

      // First, if there's a virtual entity table, we need to make that first.
      string virtualEntityColumnName = null;
      if (virtualEntityTable != null) {
        cmd.CommandText = "CREATE TABLE " + virtualEntityTable + " ( " + virtualEntityColumn.Value.ColumnDef + "} );";
        cmd.ExecuteNonQuery();
        cmd.Dispose();
        // This is at the bottom because we're replacing it.
        cmd = conn.CreateCommand();
        // And we'll save the name just to make things easier.
        virtualEntityColumnName = virtualEntityColumn.Value.Name;
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

      // Now let's parse the actual table.
      ZipArchiveEntry tableFile = zip.GetEntry(tableName + ".txt");

      // If it's a required table, throw an error if it's not found.
      if (tableFile == null) {
        if (required) {
          throw new GTFSParseException(tableName + ".txt is a required file within GTFS.");
        }
      }
      else {
        // If we have the file, let's parse the table!
        // we do something different on the first row
        bool firstRow = true;
        List<GTFSColumn?> usedCols = new List<GTFSColumn?>();
        List<string> usedNames = new List<string>();

        // Iterate the rows
        foreach (IList<string> row in CSVParser.EnumerableToRows(ZipEntryCharIterator(tableFile))) {
          if (firstRow) {
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
              // Throw an error if the table is required.
              if (required) {
                throw new GTFSParseException("Required table " + tableName + ".txt is missing required column(s) " + string.Join(", ", requiredCols));
              }
              // Otherwise just log the warning and quit parsing this table.
              else {
                warnings.UnusableFiles.Add(new GTFSUnusableFileWarning(tableName + ".txt", "Missing required column(s) " + string.Join(", ", requiredCols)));
                warnings.MissingTables.Add(tableName);
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
            for (int i = 0; i < row.Count && i < usedCols.Count; i++) {
              // Skip unrecognized columns
              if (usedCols[i] == null) continue;

              // Let's get the parameter value
              GTFSColumn col = usedCols[i].Value;
              string warning = null;
              object param = GetObject(col.Type, col.Required, row[i], ref warning);

              // 
            }
          }
        }
      }
    }

    internal static void CreateAgencyTable(SqliteConnection conn, ZipArchive zip, GTFSWarnings warnings) {
      CreateTable(
        conn: conn, zip: zip, warnings: warnings,
        tableName: "agency.txt", required: true,
        columns: new List<GTFSColumn> {
          new GTFSColumn("agency_id", GTFSDataType.ID, "TEXT PRIMARY KEY NOT NULL"),
          new GTFSColumn("agency_name", GTFSDataType.Text, "TEXT NOT NULL", true),
          new GTFSColumn("agency_url", GTFSDataType.Url, "TEXT"),
          new GTFSColumn("agency_timezone", GTFSDataType.Timezone, "TEXT"),
          new GTFSColumn("agency_lang", GTFSDataType.Language, "TEXT"),
          new GTFSColumn("agency_fare_url", GTFSDataType.Url, "TEXT"),
          new GTFSColumn("agency_phone", GTFSDataType.Phone, "TEXT"),
          new GTFSColumn("agency_email", GTFSDataType.Email, "TEXT")
        }, agencyIdColumn: true
      );
    }
  }
}