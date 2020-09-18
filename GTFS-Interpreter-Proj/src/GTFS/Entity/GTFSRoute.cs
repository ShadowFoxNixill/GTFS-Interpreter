using System;
using System.Drawing;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Enumerations;

namespace Nixill.GTFS.Entity {
  public class GTFSRoute {
    private SqliteConnection Conn;
    private GTFSFile File;

    internal GTFSRoute(SqliteConnection conn, GTFSFile file) {
      Conn = conn;
      File = file;
    }

    public string ID { get; internal set; }
    public string AgencyID { get; internal set; }
    public string ShortName { get; internal set; }
    public string LongName { get; internal set; }
    public string Desc { get; internal set; }
    public GTFSRouteType RouteType { get; internal set; }
    public Uri URL { get; internal set; }
    public Color? Color { get; internal set; }
    public Color? TextColor { get; internal set; }
    public int? SortOrder { get; internal set; }
    public GTFSPickupDropoff ContinuousPickup { get; internal set; }
    public GTFSPickupDropoff ContinuousDropOff { get; internal set; }

    public GTFSAgency Agency => File.GetAgencyById(AgencyID);
  }
}