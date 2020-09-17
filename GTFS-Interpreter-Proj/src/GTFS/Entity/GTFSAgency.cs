using System;
using Microsoft.Data.Sqlite;
using NodaTime;

namespace Nixill.GTFS.Entity {
  public class GTFSAgency {
    private SqliteConnection Conn;
    private GTFSFile File;

    public string ID { get; internal set; }
    public string Name { get; internal set; }
    public Uri URL { get; internal set; }
    public DateTimeZone Timezone { get; internal set; }
    public string Lang { get; internal set; }
    public string Phone { get; internal set; }
    public Uri FareURL { get; internal set; }
    public string Email { get; internal set; }

    internal GTFSAgency(SqliteConnection conn, GTFSFile file) {
      Conn = conn;
      File = file;
    }
  }
}