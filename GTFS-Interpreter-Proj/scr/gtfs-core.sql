-- Drop tables if they exist
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

-- Standard tables of a GTFS definition

-- Imported
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

-- Imported
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
  shape_id TEXT REFERENCES shape_ids,
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
  table_name TEXT NOT NULL,
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

