using System;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Enumerations;
using NodaTime;

namespace Nixill.GTFS.Entity {
  /// <summary>
  /// Represents a single stop, platform, station, station entrance,
  /// generic node, boarding area, or ticket point-of-sale.
  /// </summary>
  public class GTFSStop {
    private SqliteConnection Conn;
    private GTFSFile File;

    internal GTFSStop(SqliteConnection conn, GTFSFile file) {
      Conn = conn;
      File = file;
    }

    public string ID { get; internal set; }
    public string Code { get; internal set; }
    public string Name { get; internal set; }
    public string Desc { get; internal set; }
    public float Lat { get; internal set; }
    public float Lon { get; internal set; }
    public string ZoneID { get; internal set; }
    public Uri URL { get; internal set; }
    public GTFSLocationType Type { get; internal set; }
    public string ParentStationID { get; internal set; }
    public DateTimeZone StopTimezone { get; internal set; }
    public GTFSTristate WheelchairAccess { get; internal set; }
    public string LevelID { get; internal set; }
    public string PlatformCode { get; internal set; }
  }
}