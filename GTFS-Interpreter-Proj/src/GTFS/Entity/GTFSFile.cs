using Microsoft.Data.Sqlite;
using Nixill.GTFS.Parsing;

namespace Nixill.GTFS.Entity {
  public class GTFSFile {
    private SqliteConnection Conn;
    private GTFSWarnings Warnings;

    internal GTFSFile(SqliteConnection conn, GTFSWarnings warnings) {
      Conn = conn;
      Warnings = warnings;
    }
  }
}