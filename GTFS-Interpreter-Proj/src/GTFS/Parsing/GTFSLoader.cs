using System.IO;
using System.IO.Compression;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Entity;

namespace Nixill.GTFS.Parsing {
  public class GTFSLoader {

    public static GTFSFile Load(string path) {
      // First make sure the path itself exists
      if (!File.Exists(path)) {
        throw new FileNotFoundException("This file does not exist.", path);
      }

      // So now we have to populate the database pretty much from scratch.
      string connStr = new SqliteConnectionStringBuilder("") {
        DataSource = ":memory:",
        ForeignKeys = true
      }.ToString();

      using SqliteConnection conn = new SqliteConnection(connStr);
      conn.Open();

      // Open the zip file
      using ZipArchive file = ZipFile.OpenRead(path);

      // We need a warnings object too
      GTFSWarnings warnings = new GTFSWarnings();

      // And the file object
      GTFSFile ret = new GTFSFile(conn, warnings);

      // Now start actually creating tables.
      GTFSMaker.CreateFileInfoTable(conn, file, warnings);

      // And output! :D
      return ret;
    }
  }
}