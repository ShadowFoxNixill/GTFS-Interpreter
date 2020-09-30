using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Enumerations;
using Nixill.GTFS.Misc;
using Nixill.GTFS.Parsing;
using Nixill.SQLite;
using Nixill.Utils;
using NodaTime;

namespace Nixill.GTFS.Entity {
  /// <summary>
  /// Represents a single stop, platform, station, station entrance,
  /// generic node, boarding area, or ticket point-of-sale.
  /// </summary>
  public class GTFSStop : GTFSEntity {
    internal override string TableName => "stops";
    internal override string TableIDCol => "stop_id";

    internal GTFSStop(SqliteConnection conn, string id) : base(conn, id) { }

    public string Code => GTFSObjectParser.GetText(Conn.GetResult("SELECT stop_code FROM stops WHERE stop_id = @p0;", ID));
    public string Name => GTFSObjectParser.GetText(Conn.GetResult("SELECT stop_name FROM stops WHERE stop_id = @p0;", ID));
    public string Desc => GTFSObjectParser.GetText(Conn.GetResult("SELECT stop_desc FROM stops WHERE stop_id = @p0;", ID));
    public float? Lat => (float?)GTFSObjectParser.GetFloat(Conn.GetResult("SELECT stop_lat FROM stops WHERE stop_id = @p0;", ID));
    public float? Lon => (float?)GTFSObjectParser.GetFloat(Conn.GetResult("SELECT stop_lon FROM stops WHERE stop_id = @p0;", ID));
    public string ZoneID => GTFSObjectParser.GetID(Conn.GetResult("SELECT zone_id FROM stops WHERE stop_id = @p0;", ID));
    public Uri URL => GTFSObjectParser.GetUrl(Conn.GetResult("SELECT stop_url FROM stops WHERE stop_id = @p0;", ID));
    public GTFSLocationType Type => (GTFSLocationType)GTFSObjectParser.GetEnum(Conn.GetResult("SELECT location_type FROM stops WHERE stop_id = @p0;", ID));
    public string ParentStationID => GTFSObjectParser.GetID(Conn.GetResult("SELECT parent_station FROM stops WHERE stop_id = @p0;", ID));
    public DateTimeZone StopTimezone => GTFSObjectParser.GetTimezone(Conn.GetResult("SELECT stop_timezone FROM stops WHERE stop_id = @p0;", ID));
    public GTFSTristate WheelchairAccess => (GTFSTristate)GTFSObjectParser.GetEnum(Conn.GetResult("SELECT wheelchair_boarding FROM stops WHERE stop_id = @p0;", ID));
    public string LevelID => GTFSObjectParser.GetID(Conn.GetResult("SELECT level_id FROM stops WHERE stop_id = @p0;", ID));
    public string PlatformCode => GTFSObjectParser.GetText(Conn.GetResult("SELECT platform_code FROM stops WHERE stop_id = @p0;", ID));

    public GTFSFareZone Zone => new GTFSFareZone(Conn, ZoneID);
    public Coordinates? StopCoords {
      get {
        float? lat = Lat;
        float? lon = Lon;
        if (lat == null || lon == null) return null;
        return new Coordinates(lat.Value, lon.Value);
      }
    }

    public List<GTFSStopTime> ServiceTo(Duration? startTime = null, Duration? endTime = null, GTFSCalendar calendar = null, GTFSRoute route = null) {
      StringBuilder cmdText = new StringBuilder("SELECT stop_times.trip_id, stop_times.stop_sequence FROM stop_times JOIN trips ON stop_times.trip_id = trips.trip_id WHERE ");

      // For stops, we 
    }
  }

  public class GTFSFareZone : GTFSEntity {
    internal override string TableName => "fare_zones";
    internal override string TableIDCol => "zone_id";

    internal GTFSFareZone(SqliteConnection conn, string id) : base(conn, id) { }

    public List<GTFSStop> Stops => Conn.GetResultList("SELECT stop_id FROM stops WHERE zone_id = @p0;", ID)
      .Transform((obj) => new GTFSStop(Conn, GTFSObjectParser.GetID(obj)));
  }
}