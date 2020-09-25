using System;
using System.Drawing;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Enumerations;
using Nixill.GTFS.Parsing;
using Nixill.SQLite;

namespace Nixill.GTFS.Entity {
  public class GTFSRoute : GTFSEntity {
    internal override string TableIDCol => "route_id";
    internal override string TableName => "routes";

    internal GTFSRoute(SqliteConnection conn, string id) : base(conn, id) { }

    public string AgencyID => GTFSObjectParser.GetID(Conn.GetResult("SELECT agency_id FROM routes WHERE route_id = @p;", ID));
    public GTFSAgency Agency => new GTFSAgency(Conn, AgencyID);
    public string ShortName => GTFSObjectParser.GetText(Conn.GetResult("SELECT route_short_name FROM routes WHERE route_id = @p;", ID));
    public string LongName => GTFSObjectParser.GetText(Conn.GetResult("SELECT route_long_name FROM routes WHERE route_id = @p;", ID));
    public string Desc => GTFSObjectParser.GetText(Conn.GetResult("SELECT route_desc FROM routes WHERE route_id = @p;", ID));
    public GTFSRouteType RouteType => (GTFSRouteType)GTFSObjectParser.GetEnum(Conn.GetResult("SELECT route_type FROM routes WHERE route_id = @p;", ID));
    public Uri URL => GTFSObjectParser.GetUrl(Conn.GetResult("SELECT route_url FROM routes WHERE route_id = @p;", ID));
    public Color? Color => GTFSObjectParser.GetColor(Conn.GetResult("SELECT route_color FROM routes WHERE route_id = @p;", ID));
    public Color? TextColor => GTFSObjectParser.GetColor(Conn.GetResult("SELECT route_text_color FROM routes WHERE route_id = @p;", ID));
    public int? SortOrder => GTFSObjectParser.GetInteger(Conn.GetResult("SELECT route_sort_order FROM routes WHERE route_id = @p;", ID));
    public GTFSPickupDropoff ContinuousPickup => (GTFSPickupDropoff)GTFSObjectParser.GetEnum(Conn.GetResult("SELECT continuous_pickup FROM routes WHERE route_id = @p;", ID));
    public GTFSPickupDropoff ContinuousDropOff => (GTFSPickupDropoff)GTFSObjectParser.GetEnum(Conn.GetResult("SELECT continuous_drop_off FROM routes WHERE route_id = @p;", ID));
  }
}