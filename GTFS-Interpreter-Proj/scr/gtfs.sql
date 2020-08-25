-- Drop tables if they exist
DROP TABLE IF EXISTS prepaid_fares;
DROP TABLE IF EXISTS enum_expf_exception_type;
DROP TABLE IF EXISTS bulk_pass_demographics;
DROP TABLE IF EXISTS bulk_pass_vehicle_purchase;
DROP TABLE IF EXISTS bulk_pass_purchase_groups;
DROP TABLE IF EXISTS bulk_pass_purchase_locations;
DROP TABLE IF EXISTS bulk_pass_purchase_group_ids;
DROP TABLE IF EXISTS bulk_pass_constraints;
DROP TABLE IF EXISTS bulk_pass_fares;
DROP TABLE IF EXISTS bulk_passes;
DROP TABLE IF EXISTS enum_exbp_location_type;
DROP TABLE IF EXISTS enum_exbp_timer_activation;
DROP TABLE IF EXISTS enum_exbp_days_of_week;
DROP TABLE IF EXISTS enum_exbp_activation_time;
DROP TABLE IF EXISTS fare_demographics_prices;
DROP TABLE IF EXISTS fare_demographics_properties;
DROP TABLE IF EXISTS fare_demographics;
DROP TABLE IF EXISTS enum_exfd_demographic_inclusion;
DROP TABLE IF EXISTS timetable_notes;
DROP TABLE IF EXISTS timetable_rows;
DROP TABLE IF EXISTS timetable_column_stops;
DROP TABLE IF EXISTS timetable_columns;
DROP TABLE IF EXISTS timetables;
DROP TABLE IF EXISTS enum_extt_around_stop_type;
DROP TABLE IF EXISTS enum_extt_prefix_suffix_condition;
DROP TABLE IF EXISTS enum_extt_column_type;
DROP TABLE IF EXISTS enum_extt_column_row_order;
DROP TABLE IF EXISTS attributions;
DROP TABLE IF EXISTS translations;
DROP TABLE IF EXISTS feed_info;
DROP TABLE IF EXISTS pathways;
DROP TABLE IF EXISTS transfers;
DROP TABLE IF EXISTS frequencies;
DROP TABLE IF EXISTS fare_rules;
DROP TABLE IF EXISTS fare_attributes;
DROP TABLE IF EXISTS calendar_dates;
DROP TABLE IF EXISTS stop_times;
DROP TABLE IF EXISTS trips;
DROP TABLE IF EXISTS calendar;
DROP TABLE IF EXISTS shapes;
DROP TABLE IF EXISTS shape_ids;
DROP TABLE IF EXISTS routes;
DROP TABLE IF EXISTS stops;
DROP TABLE IF EXISTS fare_zones;
DROP TABLE IF EXISTS levels;
DROP TABLE IF EXISTS agency;
DROP TABLE IF EXISTS enum_table_names;
DROP TABLE IF EXISTS enum_pathway_mode;
DROP TABLE IF EXISTS enum_transfer_type;
DROP TABLE IF EXISTS enum_calendar_date;
DROP TABLE IF EXISTS enum_pickup_drop_off;
DROP TABLE IF EXISTS enum_route_types;
DROP TABLE IF EXISTS enum_location_type;
DROP TABLE IF EXISTS enum_tristate;
DROP TABLE IF EXISTS enum_boolean;

-- Let's define all the enums at the start.
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
  (4, 'Boarding area');

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

CREATE TABLE enum_table_names (
  enum_val TEXT PRIMARY KEY NOT NULL
);
INSERT INTO enum_table_names VALUES
  ('agency'),
  ('stops'),
  ('routes'),
  ('stop_times'),
  ('feed_info'),
  ('pathways'),
  ('levels'),
  ('attributions');

-- Standard tables of a GTFS definition

CREATE TABLE agency (
  agency_id TEXT PRIMARY KEY NOT NULL,
  agency_name TEXT NOT NULL,
  agency_url TEXT,
  agency_timezone TEXT NOT NULL,
  agency_lang TEXT,
  agency_phone TEXT,
  agency_fare_url TEXT,
  agency_email TEXT
);

CREATE TABLE levels (
  level_id TEXT PRIMARY KEY NOT NULL,
  level_index REAL NOT NULL,
  level_name TEXT
);

CREATE TABLE fare_zones (zone_id TEXT PRIMARY KEY);

CREATE TABLE stops (
  stop_id TEXT PRIMARY KEY NOT NULL,
  stop_code TEXT,
  stop_name TEXT,
  stop_desc TEXT,
  stop_lat REAL,
  stop_lon REAL,
  zone_id TEXT REFERENCES fare_zones,
  stop_url TEXT,
  location_type INTEGER NOT NULL REFERENCES enum_location_type DEFAULT 0,
  parent_station TEXT REFERENCES stops,
  stop_timezone TEXT,
  wheelchair_boarding INTEGER NOT NULL REFERENCES enum_tristate DEFAULT 0,
  level_id TEXT REFERENCES levels,
  platform_code TEXT
);

CREATE TABLE routes (
  route_id TEXT PRIMARY KEY NOT NULL,
  agency_id TEXT NOT NULL REFERENCES agency,
  route_short_name TEXT,
  route_long_name TEXT,
  route_desc TEXT,
  route_type INTEGER NOT NULL REFERENCES enum_route_types,
  route_url TEXT,
  route_color TEXT,
  route_text_color TEXT,
  route_sort_order INTEGER,
  continuous_pickup INTEGER NOT NULL REFERENCES enum_pickup_drop_off DEFAULT 1,
  continuous_drop_off INTEGER NOT NULL REFERENCES enum_pickup_drop_off DEFAULT 1
);

CREATE TABLE shape_ids (shape_id TEXT PRIMARY KEY NOT NULL);

CREATE TABLE shapes (
  shape_id TEXT NOT NULL REFERENCES shape_ids,
  shape_pt_lat REAL NOT NULL,
  shape_pt_lon REAL NOT NULL,
  shape_pt_sequence INTEGER NOT NULL,
  shape_dist_traveled INTEGER NOT NULL,
  PRIMARY KEY (shape_id, shape_pt_sequence)
);

CREATE TABLE calendar (
  service_id TEXT PRIMARY KEY NOT NULL,
  monday INTEGER NOT NULL REFERENCES enum_boolean,
  tuesday INTEGER NOT NULL REFERENCES enum_boolean,
  wednesday INTEGER NOT NULL REFERENCES enum_boolean,
  thursday INTEGER NOT NULL REFERENCES enum_boolean,
  friday INTEGER NOT NULL REFERENCES enum_boolean,
  saturday INTEGER NOT NULL REFERENCES enum_boolean,
  sunday INTEGER NOT NULL REFERENCES enum_boolean,
  "start_date" TEXT NOT NULL,
  "end_date" TEXT NOT NULL
);

CREATE TABLE trips (
  route_id TEXT NOT NULL REFERENCES routes,
  service_id TEXT NOT NULL REFERENCES calendar,
  trip_id TEXT PRIMARY KEY NOT NULL,
  trip_headsign TEXT,
  trip_short_name TEXT,
  direction_id INTEGER REFERENCES enum_boolean,
  block_id TEXT,
  shape_id TEXT REFERENCES shapes,
  wheelchair_accessible INTEGER NOT NULL REFERENCES enum_tristate DEFAULT 0,
  bikes_allowed INTEGER NOT NULL REFERENCES enum_tristate DEFAULT 0
);

CREATE TABLE stop_times (
  trip_id TEXT NOT NULL REFERENCES trips,
  arrival_time TEXT,
  departure_time TEXT,
  stop_id TEXT NOT NULL REFERENCES stops,
  stop_sequence INTEGER NOT NULL,
  stop_headsign TEXT,
  pickup_type INTEGER NOT NULL REFERENCES enum_pickup_drop_off DEFAULT 0,
  drop_off_type INTEGER NOT NULL REFERENCES enum_pickup_drop_off DEFAULT 0,
  continuous_pickup INTEGER NOT NULL REFERENCES enum_pickup_drop_off DEFAULT 1,
  continuous_drop_off INTEGER NOT NULL REFERENCES enum_pickup_drop_off DEFAULT 1,
  shape_dist_traveled REAL,
  timepoint INTEGER NOT NULL REFERENCES enum_boolean DEFAULT 1,
  PRIMARY KEY (trip_id, stop_sequence)
);

CREATE TABLE calendar_dates (
  service_id TEXT NOT NULL REFERENCES calendar,
  "date" TEXT NOT NULL,
  exception_type TEXT NOT NULL REFERENCES enum_calendar_date,
  PRIMARY KEY (service_id, "date")
);

CREATE TABLE fare_attributes (
  fare_id TEXT PRIMARY KEY NOT NULL,
  price REAL NOT NULL,
  currency_type TEXT NOT NULL,
  payment_method INTEGER NOT NULL REFERENCES enum_boolean,
  transfers INTEGER,
  agency_id TEXT NOT NULL REFERENCES agency,
  transfer_duration INTEGER
);

CREATE TABLE fare_rules (
  fare_id TEXT NOT NULL REFERENCES fare_attributes,
  route_id TEXT REFERENCES routes,
  origin_id TEXT REFERENCES fare_zones,
  destination_id TEXT REFERENCES fare_zones,
  contains_id TEXT REFERENCES fare_zones
);

CREATE TABLE frequencies (
  trip_id TEXT NOT NULL REFERENCES trips,
  start_time TEXT NOT NULL,
  end_time TEXT NOT NULL,
  headway_secs INTEGER NOT NULL,
  exact_times INTEGER NOT NULL REFERENCES enum_boolean DEFAULT 0
);

CREATE TABLE transfers (
  from_stop_id TEXT NOT NULL REFERENCES stops,
  to_stop_id TEXT NOT NULL REFERENCES stops,
  transfer_type INTEGER NOT NULL REFERENCES enum_transfer_type,
  min_transfer_time INTEGER
);

CREATE TABLE pathways (
  pathway_id TEXT PRIMARY KEY NOT NULL,
  from_stop_id TEXT NOT NULL REFERENCES stops,
  to_stop_id TEXT NOT NULL REFERENCES stops,
  pathway_mode INTEGER NOT NULL REFERENCES enum_pathway_mode,
  is_bidirectional INTEGER NOT NULL REFERENCES enum_boolean,
  "length" REAL,
  traversal_time INTEGER,
  stair_count INTEGER,
  max_slope REAL,
  min_width REAL,
  signposted_as TEXT,
  reversed_signposted_as TEXT
);

CREATE TABLE feed_info (
  feed_publisher_name TEXT NOT NULL,
  feed_publisher_url TEXT,
  feed_lang TEXT NOT NULL,
  default_lang TEXT,
  feed_start_date TEXT,
  feed_end_date TEXT,
  feed_version TEXT,
  feed_contact_email TEXT,
  feed_contact_url TEXT
);

CREATE TABLE translations (
  table_name TEXT NOT NULL REFERENCES enum_table_names,
  field_name TEXT NOT NULL,
  "language" TEXT NOT NULL,
  translation TEXT NOT NULL,
  record_id TEXT,
  record_sub_id TEXT,
  field_value TEXT
);

CREATE TABLE attributions (
  attribution_id TEXT PRIMARY KEY NOT NULL,
  agency_id TEXT REFERENCES agency (agency_id),
  route_id TEXT REFERENCES routes (route_id),
  trip_id TEXT REFERENCES trips (trip_id),
  organization_name TEXT NOT NULL,
  is_producer INTEGER NOT NULL REFERENCES enum_boolean DEFAULT 0,
  is_operator INTEGER NOT NULL REFERENCES enum_boolean DEFAULT 0,
  is_authority INTEGER NOT NULL REFERENCES enum_boolean DEFAULT 0,
  attribution_url TEXT,
  attribution_email TEXT,
  attribution_phone TEXT
);

-- Nix's GTFS extension proposal: Timetables

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
  (7, 'Next trip');

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

CREATE TABLE timetables (
  timetable_id TEXT PRIMARY KEY NOT NULL,
  timetable_name TEXT NOT NULL,
  column_order INTEGER NOT NULL REFERENCES enum_extt_column_row_order DEFAULT 0,
  row_order INTEGER NOT NULL REFERENCES enum_extt_column_row_order DEFAULT 0,
  transposed INTEGER NOT NULL REFERENCES enum_boolean DEFAULT 0
);

CREATE TABLE timetable_columns (
  timetable_id TEXT NOT NULL REFERENCES timetables,
  column_index INTEGER NOT NULL,
  column_type INTEGER NOT NULL REFERENCES enum_extt_column_type DEFAULT 0,
  column_header TEXT,
  column_prefix TEXT,
  prefix_condition INTEGER NOT NULL REFERENCES enum_extt_prefix_suffix_condition DEFAULT 2,
  column_value TEXT,
  column_suffix TEXT,
  suffix_condition INTEGER NOT NULL REFERENCES enum_extt_prefix_suffix_condition DEFAULT 2,
  skip_if_empty INTEGER NOT NULL REFERENCES enum_boolean DEFAULT 1,
  PRIMARY KEY (timetable_id, column_index)
);

CREATE TABLE timetable_column_stops (
  timetable_id TEXT NOT NULL,
  column_index INTEGER NOT NULL,
  stop_id TEXT NOT NULL REFERENCES stops,
  stop_precedence INTEGER NOT NULL,
  around_stop_id TEXT REFERENCES stops,
  around_direction TEXT NOT NULL REFERENCES enum_extt_around_stop_type DEFAULT 0,
  only_timepoint TEXT NOT NULL REFERENCES enum_boolean DEFAULT 0,
  FOREIGN KEY (timetable_id, column_index) REFERENCES timetable_columns (timetable_id, column_index),
  PRIMARY KEY (timetable_id, column_index, stop_precedence)
);

CREATE TABLE timetable_rows (
  timetable_id TEXT NOT NULL REFERENCES timetables,
  route_id TEXT REFERENCES routes,
  direction_id TEXT REFERENCES enum_boolean,
  through_stop_id TEXT REFERENCES stops,
  except_stop_id TEXT REFERENCES stops,
  timed_stop_id TEXT REFERENCES stops,
  timed_stop_offset TEXT,
  only_after_time TEXT,
  only_before_time TEXT,
  trip_id TEXT REFERENCES trips,
  sort_time TEXT,
  trip_note TEXT,
  except_trip_id TEXT REFERENCES trips
);

CREATE TABLE timetable_notes (
  timetable_id TEXT NOT NULL REFERENCES timetables,
  note_text TEXT NOT NULL,
  note_order INTEGER,
  note_time TEXT,
  service_id TEXT NOT NULL REFERENCES calendar
);

-- Nix's GTFS Expansion Proposal: Fare demographics

CREATE TABLE enum_exfd_demographic_inclusion (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_exfd_demographic_inclusion VALUES
  (0, 'Not checked'),
  (1, 'Any of'),
  (2, 'All of'),
  (3, 'None of');

CREATE TABLE fare_demographics (
  demographic_id TEXT PRIMARY KEY NOT NULL,
  demographic_name TEXT NOT NULL,
  demographic_detail TEXT NOT NULL
);

CREATE TABLE fare_demographics_properties (
  demographic_id TEXT NOT NULL REFERENCES fare_demographics,
  property_name TEXT NOT NULL,
  property_value INTEGER NOT NULL REFERENCES enum_exfd_demographic_inclusion,
  PRIMARY KEY (demographic_id, property_name)
);

CREATE TABLE fare_demographics_prices (
  demographic_id TEXT NOT NULL REFERENCES fare_demographics,
  fare_id TEXT NOT NULL REFERENCES fare_attributes,
  adjusted_price REAL NOT NULL,
  PRIMARY KEY (demographic_id, fare_id)
);

-- Nix's GTFS Extensions: Bulk Passes

INSERT INTO enum_location_type VALUES
  (5, 'Unmanned pass point of sale'),
  (6, 'Manned pass point of sale');

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

CREATE TABLE enum_exbp_days_of_week (
  enum_val INTEGER PRIMARY KEY,
  enum_prose TEXT NOT NULL
);
INSERT INTO enum_exbp_days_of_week VALUES
  (1, 'Monday'),
  (2, 'Tuesday'),
  (3, 'Wednesday'),
  (4, 'Thursday'),
  (5, 'Friday'),
  (6, 'Saturday'),
  (7, 'Sunday');

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

CREATE TABLE bulk_passes (
  pass_id TEXT PRIMARY KEY NOT NULL,
  pass_price REAL NOT NULL,
  pass_currency_type TEXT NOT NULL,
  pass_name TEXT NOT NULL,
  fares_to_pass INTEGER NOT NULL DEFAULT 0,
  fare_cap INTEGER NOT NULL REFERENCES enum_boolean DEFAULT 0,
  activation_time INTEGER NOT NULL REFERENCES enum_exbp_activation_time DEFAULT 0,
  years INTEGER,
  anchored_months INTEGER,
  rolling_months INTEGER,
  months INTEGER,
  weeks INTEGER,
  "days" INTEGER,
  "hours" INTEGER,
  "minutes" INTEGER,
  "seconds" INTEGER,
  offset_months INTEGER,
  offset_days INTEGER,
  offset_day_of_week INTEGER REFERENCES enum_exbp_days_of_week,
  offset_seconds INTEGER
);

CREATE TABLE bulk_pass_fares (
  pass_id TEXT NOT NULL REFERENCES bulk_passes,
  fare_id TEXT NOT NULL REFERENCES fare_attributes,
  covered_amount REAL,
  covered_factor REAL,
  quantity INTEGER,
  activates_timer INTEGER NOT NULL REFERENCES enum_exbp_timer_activation DEFAULT 1,
  transfers INTEGER,
  PRIMARY KEY (pass_id, fare_id)
);

CREATE TABLE bulk_pass_constraints (
  pass_id TEXT NOT NULL REFERENCES bulk_passes,
  hold_pass_id TEXT REFERENCES bulk_passes,
  purchase_pass_id TEXT REFERENCES bulk_passes,
  purchase_fare_id TEXT REFERENCES fare_attributes,
  use_pass_id TEXT REFERENCES bulk_passes,
  without_pass_id TEXT REFERENCES bulk_passes,
  constraint_group TEXT
);

CREATE TABLE bulk_pass_purchase_group_ids (location_group_id TEXT PRIMARY KEY NOT NULL);

CREATE TABLE bulk_pass_purchase_locations (
  location_id TEXT PRIMARY KEY NOT NULL,
  location_name TEXT,
  stop_id TEXT REFERENCES stops,
  location_group_id TEXT REFERENCES bulk_pass_purchase_group_ids,
  location_url TEXT,
  agency_id TEXT REFERENCES agency,
  location_type INTEGER NOT NULL REFERENCES enum_exbp_location_types DEFAULT 0,
  accepts_cash INTEGER NOT NULL REFERENCES enum_tristate DEFAULT 0,
  accepts_credit INTEGER NOT NULL REFERENCES enum_tristate DEFAULT 0,
  accepts_mobile_pay INTEGER NOT NULL REFERENCES enum_tristate DEFAULT 0,
  accepts_smart_card INTEGER NOT NULL REFERENCES enum_tristate DEFAULT 0,
  accepts_check INTEGER NOT NULL REFERENCES enum_tristate DEFAULT 0
);

CREATE TABLE bulk_pass_purchase_groups (
  pass_id TEXT NOT NULL REFERENCES bulk_passes,
  location_group_id TEXT NOT NULL REFERENCES bulk_pass_purchase_group_ids
);

CREATE TABLE bulk_pass_vehicle_purchase (
  route_id TEXT REFERENCES routes,
  trip_id TEXT REFERENCES trips,
  location_group_id TEXT NOT NULL REFERENCES bulk_pass_purchase_groups
);

-- And now combining the last two proposals above:

CREATE TABLE bulk_pass_demographics (
  demographic_id TEXT NOT NULL REFERENCES fare_demographics,
  pass_id TEXT NOT NULL REFERENCES bulk_passes,
  fare_id TEXT REFERENCES fare_attributes,
  adjusted_price REAL,
  adjusted_quantity INTEGER,
  quantity_group TEXT,
  "availability" INTEGER REFERENCES enum_boolean,
  pass_name TEXT
);

ALTER TABLE bulk_pass_constraints ADD COLUMN demographic_id TEXT REFERENCES fare_demographics;
ALTER TABLE bulk_pass_constraints ADD COLUMN not_demographic_id TEXT REFERENCES fare_demographics;

ALTER TABLE bulk_pass_purchase_groups ADD COLUMN demographic_id TEXT REFERENCES fare_demographics;
ALTER TABLE bulk_pass_purchase_groups ADD COLUMN exception_type INTEGER REFERENCES enum_boolean;

-- And one last quick proposal:

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

CREATE TABLE prepaid_fares (
  fare_id TEXT REFERENCES fare_attributes,
  stop_id TEXT REFERENCES stops,
  route_id TEXT REFERENCES routes,
  trip_id TEXT REFERENCES trips,
  exception_type INTEGER NOT NULL REFERENCES enum_expf_exception_type
);

ALTER TABLE fare_rules ADD COLUMN contains_group TEXT;
