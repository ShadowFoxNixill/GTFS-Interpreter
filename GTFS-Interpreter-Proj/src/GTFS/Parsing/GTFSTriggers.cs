using Microsoft.Data.Sqlite;

namespace Nixill.GTFS.Parsing {
  internal static class GTFSTriggers {
    internal static void CreateTriggers(string tableName, SqliteConnection conn) {
      if (tableName == "stops") CreateStopsTriggers(conn);
    }

    internal static void CreateStopsTriggers(SqliteConnection conn) {
      SqliteCommand cmd = conn.CreateCommand();

      cmd.CommandText = @"
        CREATE TRIGGER stops_name BEFORE INSERT ON stops
        WHEN NEW.stop_name IS NULL AND NEW.location_type IN (0, 1, 2)
        BEGIN
          INSERT INTO gtfs_warnings (warn_message, warn_table, warn_field, warn_record) VALUES
            ('stop_name is required for location_type ' || NEW.location_type, 'stops', 'stop_name', NEW.stop_id);
          RAISE(FAIL, 'TRIGGER - stop_name is required for location_type' || NEW.location_type);
        END;

        CREATE TRIGGER stops_parent_missing BEFORE INSERT ON stops
        WHEN NEW.location_type IN (2, 3, 4) AND NEW.parent_station IS NULL
        BEGIN
          INSERT INTO gtfs_warnings (warn_message, warn_table, warn_field, warn_record) VALUES
            ('Stops of location_type ' || NEW.location_type || ' must have a parent_station.', 'stops', 'parent_station', NEW.stop_id);
          RAISE(FAIL, 'TRIGGER - Stops of location_type ' || NEW.location_type || ' must have a parent_station.');
        END;

        CREATE TRIGGER stops_parent_illegal_23 BEFORE INSERT ON stops
        WHEN NEW.location_type IN (2, 3) AND NEW.parent_station IS NOT NULL AND NEW.parent_station NOT IN
          (SELECT stop_id FROM stops WHERE location_type = 1)
        BEGIN
          INSERT INTO gtfs_warnings (warn_message, warn_table, warn_field, warn_record) VALUES
            ('Stops of location_type ' || NEW.location_type || ' must have a parent_station with location_type of 1.', 'stops', 'parent_station', NEW.stop_id);
          RAISE(FAIL, 'TRIGGER - Stops of location_type ' || NEW.location_type || ' must have a parent_station with location_type of 1.');
        END;

        CREATE TRIGGER stops_parent_illegal_4 BEFORE INSERT ON stops
        WHEN NEW.location_type = 4 AND NEW.parent_station IS NOT NULL AND NEW.parent_station NOT IN
          (SELECT stop_id FROM stops WHERE location_type = 0)
        BEGIN
          INSERT INTO gtfs_warnings (warn_message, warn_table, warn_field, warn_record) VALUES
            ('Stops of location_type ' || NEW.location_type || ' must have a parent_station with location_type of 1.', 'stops', 'parent_station', NEW.stop_id);
          RAISE(FAIL, 'TRIGGER - Stops of location_type ' || NEW.location_type || ' must have a parent_station with location_type of 1.');
        END;

        CREATE TRIGGER stops_parent_illegal_056 BEFORE INSERT ON stops
        WHEN NEW.location_type IN (0, 5, 6) AND NEW.parent_station IS NOT NULL AND NEW.parent_station NOT IN
          (SELECT stop_id FROM stops WHERE location_type = 1)
        BEGIN
          INSERT INTO gtfs_warnings (warn_message, warn_table, warn_field, warn_record) VALUES
            ('Stops of location_type ' || NEW.location_type || ' must have either no parent_station or a parent_station with location_type of 1.', 'stops', 'parent_station', NEW.stop_id);
          UPDATE NEW SET parent_station = NULL;
        END;

        CREATE TRIGGER stops_parent_illegal_1 BEFORE INSERT ON stops
        WHEN NEW.location_type = 1 AND NEW.parent_station IS NOT NULL
        BEGIN
          INSERT INTO gtfs_warnings (warn_message, warn_table, warn_field, warn_record) VALUES
            ('Stops of location_type 1 must not have a parent_station.', 'stops', 'parent_station', NEW.stop_id);
          UPDATE NEW SET parent_station = NULL;
        END;

        CREATE TRIGGER stops_location_012 BEFORE INSERT ON stops
        WHEN (NEW.stop_lat IS NULL OR NEW.stop_lon IS NULL) AND NEW.location_type IN (0, 1, 2)
        BEGIN
          INSERT INTO gtfs_warnings (warn_message, warn_table, warn_record) VALUES
            ('stop_lat and stop_lon are required for stops of location_type ' || NEW.location_type, 'stops', NEW.stop_id);
          RAISE(FAIL, 'TRIGGER - stop_lat and stop_lon are required for stops of location_type ' || NEW.location_type);
        END;

        CREATE TRIGGER stops_location_56 BEFORE INSERT ON stops
        WHEN (NEW.stop_lat IS NULL OR NEW.stop_lon IS NULL) AND NEW.location_type IN (5, 6) AND NEW.parent_station IS NULL
        BEGIN
          INSERT INTO gtfs_warnings (warn_message, warn_table, warn_record) VALUES
            ('stop_lat and stop_lon are required for parentless stops of location_type ' || NEW.location_type, 'stops', NEW.stop_id);
          RAISE(FAIL, 'TRIGGER - stop_lat and stop_lon are required for parentless stops of location_type ' || NEW.location_type);
        END;
      ";

      cmd.ExecuteNonQuery();
      cmd.Dispose();
    }
  }
}