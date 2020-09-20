using Microsoft.Data.Sqlite;

namespace Nixill.GTFS.Parsing {
  internal static class GTFSEnumerableMaker {
    internal static void Make(SqliteConnection conn) {
      using SqliteCommand cmd = conn.CreateCommand();
      cmd.CommandText = @"
CREATE TABLE enum_boolean (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_boolean VALUES
  (0, 'False'),
  (1, 'True');

CREATE TABLE enum_tristate (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_tristate VALUES
  (0, 'Unknown'),
  (1, 'Yes'),
  (2, 'No');

CREATE TABLE enum_location_type (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_location_type VALUES
  (0, 'Stop or Platform'),
  (1, 'Station'),
  (2, 'Station entrance or exit'),
  (3, 'Generic node'),
  (4, 'Boarding area'),
  (5, 'Unmanned pass point of sale'),
  (6, 'Manned pass point of sale');

CREATE TABLE enum_route_types (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_route_types VALUES
  (0, 'Tram, streetcar, light rail'),
  (1, 'Subway, metro'),
  (2, 'Rail'),
  (3, 'Bus'),
  (4, 'Ferry'),
  (5, 'Cable tram'),
  (6, 'Aerial lift'),
  (7, 'Funicular'),
  (11, 'Trolleybus'),
  (12, 'Monorail');

CREATE TABLE enum_pickup_drop_off (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_pickup_drop_off VALUES
  (0, 'Available normally.'),
  (1, 'Unavailable.'),
  (2, 'Phone agency.'),
  (3, 'Coordinate with driver.');

CREATE TABLE enum_calendar_date (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_calendar_date VALUES
  (1, 'Service added'),
  (2, 'Service removed');

CREATE TABLE enum_transfer_type (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_transfer_type VALUES
  (0, 'Recommended transfer point'),
  (1, 'Timed transfer point'),
  (2, 'Transfer requires time'),
  (3, 'Transfer not possible');

CREATE TABLE enum_pathway_mode (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_pathway_mode VALUES
  (1, 'Walkway'),
  (2, 'Stairs'),
  (3, 'Moving sidewalk'),
  (4, 'Escalator'),
  (5, 'Elevator'),
  (6, 'Fare gate'),
  (7, 'Exit gate');

CREATE TABLE enum_extt_column_row_order (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_extt_column_row_order VALUES
  (0, 'Reading order / Down'),
  (1, 'Reverse reading order / Up'),
  (2, 'Right / Down'),
  (3, 'Left / Up');

CREATE TABLE enum_extt_column_type (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_extt_column_type VALUES
  (0, 'Stop time'),
  (1, 'Route name'),
  (2, 'Trip name'),
  (3, 'Trip notes'),
  (4, 'Previous route'),
  (5, 'Previous trip'),
  (6, 'Next route'),
  (7, 'Next trip'),
  (8, 'Note column');

CREATE TABLE enum_extt_prefix_suffix_condition (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_extt_prefix_suffix_condition VALUES
  (0, 'Always'),
  (1, 'Only with value'),
  (2, 'Only with value; if stop, only non timepoint'),
  (3, 'Only with value; if stop, only timepoint');

CREATE TABLE enum_extt_around_stop_type (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_extt_around_stop_type VALUES
  (0, 'Before or after'),
  (1, 'Before'),
  (2, 'After or not served'),
  (3, 'After'),
  (4, 'Before or not served'),
  (5, 'Not served');

CREATE TABLE enum_exbp_activation_time (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_exbp_activation_time VALUES
  (0, 'First applied fare'),
  (1, 'Fixed period of time'),
  (2, 'Activated on purchase'),
  (3, 'First fare after purchase'),
  (4, 'When quantity is exhausted');

CREATE TABLE enum_exbp_timer_activation (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_exbp_timer_activation VALUES
  (0, 'Does not activate timer'),
  (1, 'Activates timer'),
  (2, 'Activates timer but only for this quantity group');

CREATE TABLE enum_exbp_location_type (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_exbp_location_type VALUES
  (0, 'Unknown'),
  (1, 'Transit agency building'),
  (2, 'Transit agency space in another building'),
  (3, 'Unrelated building'),
  (4, 'Agency''s mobile app'),
  (5, 'Agency''s TVM'),
  (6, 'Vehicles operated by the agency'),
  (7, 'Mail order passes');

CREATE TABLE enum_expf_exception_type (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_expf_exception_type VALUES
  (0, 'Fare is paid onboard.'),
  (1, 'Fare is paid in advance of boarding.'),
  (2, 'Fare is paid at a later stop on the route.'),
  (3, 'Fare does not apply (use another)'),
  (4, 'Fare does not need to be paid (is free)');

CREATE TABLE gtfs_warnings (
  warn_message TEXT NOT NULL,
  warn_table TEXT,
  warn_field TEXT,
  warn_record TEXT
);
";
      cmd.ExecuteNonQuery();
    }
  }
}