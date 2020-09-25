using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Parsing;
using Nixill.SQLite;

namespace Nixill.GTFS.Entity {
  public class GTFSFareZone : GTFSEntity {
    internal override string TableName => "fare_zones";
    internal override string TableIDCol => "zone_id";

    internal GTFSFareZone(SqliteConnection conn, string id) : base(conn, id) { }

    public IList<GTFSStop> Stops {
      get {
        List<GTFSStop> ret = new List<GTFSStop>();

        foreach (object obj in Conn.GetResultList($"SELECT stop_id FROM stops WHERE zone_id = @p;", ID)) {
          ret.Add(new GTFSStop(Conn, GTFSObjectParser.GetID(obj)));
        }

        return ret.AsReadOnly();
      }
    }
  }
}