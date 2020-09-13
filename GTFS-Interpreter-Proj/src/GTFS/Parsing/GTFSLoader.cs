using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Entity;

namespace Nixill.GTFS.Parsing {
  public static class GTFSLoader {
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

      SqliteConnection conn = new SqliteConnection(connStr);
      conn.Open();

      // Open the zip file
      using ZipArchive file = ZipFile.OpenRead(path);

      // We need a warnings object too
      GTFSWarnings warnings = new GTFSWarnings();

      // And the list of loaded files
      HashSet<string> files = new HashSet<string>();

      // And the file object
      GTFSFile ret = new GTFSFile(conn, warnings, files);

      // Now start actually creating tables.
      if (GTFSMaker.CreateFeedInfoTable(conn, file, warnings)) files.Add("feed_info");

      // And output! :D
      return ret;
    }
  }
}