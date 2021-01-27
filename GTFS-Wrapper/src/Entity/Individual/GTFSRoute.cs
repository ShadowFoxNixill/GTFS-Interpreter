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

    public string AgencyID => GTFSObjectParser.GetID(Select("agency_id"));
    public string ShortName => GTFSObjectParser.GetText(Select("route_short_name"));
    public string LongName => GTFSObjectParser.GetText(Select("route_long_name"));
    public string Desc => GTFSObjectParser.GetText(Select("route_desc"));
    public GTFSRouteType RouteType => (GTFSRouteType)GTFSObjectParser.GetEnum(Select("route_type"));
    public Uri URL => GTFSObjectParser.GetUrl(Select("route_url"));
    public Color? Color => GTFSObjectParser.GetColor(Select("route_color"));
    public Color? TextColor => GTFSObjectParser.GetColor(Select("route_text_color"));
    public int? SortOrder => GTFSObjectParser.GetInteger(Select("route_sort_order"));
    public GTFSPickupDropoff ContinuousPickup => (GTFSPickupDropoff)GTFSObjectParser.GetEnum(Select("continuous_pickup"));
    public GTFSPickupDropoff ContinuousDropOff => (GTFSPickupDropoff)GTFSObjectParser.GetEnum(Select("continuous_drop_off"));

    public GTFSAgency Agency => new GTFSAgency(Conn, AgencyID);
  }
}