using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Enumerations;
using Nixill.GTFS.Parsing;
using Nixill.SQLite;

namespace Nixill.GTFS.Entity {
  public class GTFSTrip : GTFSEntity {
    internal override string TableName => "trips";
    internal override string TableIDCol => "trip_id";

    internal GTFSTrip(SqliteConnection conn, string id) : base(conn, id) { }

    public string RouteID => GTFSObjectParser.GetID(Conn.GetResult("SELECT route_id FROM trips WHERE trip_id = @p;", ID));
    public string ServiceID => GTFSObjectParser.GetID(Conn.GetResult("SELECT service_id FROM trips WHERE trip_id = @p;", ID));
    public string Headsign => GTFSObjectParser.GetText(Conn.GetResult("SELECT trip_headsign FROM trips WHERE trip_id = @p;", ID));
    public string ShortName => GTFSObjectParser.GetText(Conn.GetResult("SELECT trip_short_name FROM trips WHERE trip_id = @p;", ID));
    public int? DirectionID => GTFSObjectParser.GetEnum(Conn.GetResult("SELECT direction_id FROM trips WHERE trip_id = @p;", ID));
    public string BlockID => GTFSObjectParser.GetID(Conn.GetResult("SELECT block_id FROM trips WHERE trip_id = @p;", ID));
    public string ShapeID => GTFSObjectParser.GetID(Conn.GetResult("SELECT shape_id FROM trips WHERE trip_id = @p;", ID));
    public GTFSTristate WheelchairAccessibility => (GTFSTristate)GTFSObjectParser.GetEnum(Conn.GetResult("SELECT wheelchair_accessible FROM trips WHERE trip_id = @p;", ID));
    public GTFSTristate BikesAllowed => (GTFSTristate)GTFSObjectParser.GetEnum(Conn.GetResult("SELECT bikes_allowed FROM trips WHERE trip_id = @p;", ID));

    public GTFSRoute Route => new GTFSRoute(Conn, RouteID);
    public GTFSCalendar Service => new GTFSCalendar(Conn, ServiceID);
    public GTFSBlock Block => new GTFSBlock(Conn, BlockID);
    public GTFSShape Shape => new GTFSShape(Conn, ShapeID);
  }

  public class GTFSBlock : GTFSEntity {
    internal override string TableName => "trips";
    internal override string TableIDCol => "trip_id";

    internal GTFSBlock(SqliteConnection conn, string id) : base(conn, id) { }

    public List<GTFSTrip> Trips {
      get {
        List<GTFSTrip> ret = new List<GTFSTrip>();

        foreach (object obj in Conn.GetResultList("SELECT trip_id FROM trips WHERE block_id = @p", ID)) {
          ret.Add(new GTFSTrip(Conn, GTFSObjectParser.GetID(obj)));
        }

        return ret;
      }
    }
  }
}