using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.SQLite;

namespace Nixill.GTFS.Entity {
  public abstract class GTFSEntity {
    internal SqliteConnection Conn;
    internal abstract string TableName { get; }
    internal abstract string TableIDCol { get; }

    public readonly string ID;

    internal GTFSEntity(SqliteConnection conn, string id) {
      Conn = conn;
      ID = id;
    }

    public override bool Equals(object other) {
      if (GetType() != other.GetType()) return false;
      return (ID == ((GTFSEntity)other).ID);
    }

    public override int GetHashCode() {
      return (TableName + " " + ID).GetHashCode();
    }

    public Dictionary<string, object> GetProperties() {
      return Conn.GetRowDict($"SELECT * FROM {TableName} WHERE {TableIDCol} = @p;", ID);
    }
  }
}