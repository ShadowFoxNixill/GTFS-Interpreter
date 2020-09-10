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

      // If the path *is* a database, just load it as a database.
      if (path.EndsWith(".db")) {
        return new GTFSFile(path);
      }

      // Otherwise check if the database exists.
      if (File.Exists(path + ".db")) {
        // If so, is it newer?
        if (File.GetLastWriteTimeUtc(path + ".db") > File.GetLastWriteTimeUtc(path)) {
          return new GTFSFile(path + ".db");
        }

        // If it's not newer, we'll have to create the database ourself.
        // Let's delete the old one first.
        File.Delete(path + ".db");
      }

      return BuildDatabase(path);
    }

    private static GTFSFile BuildDatabase(string path) {
      // So now we have to populate the database pretty much from scratch.
      string connStr = new SqliteConnectionStringBuilder("") {
        DataSource = path + ".db",
        ForeignKeys = true
      }.ToString();

      using SqliteConnection conn = new SqliteConnection(connStr);
      conn.Open();

      // Open the zip file
      using ZipArchive file = ZipFile.OpenRead(path);

      // Now start actually creating tables.
      GTFSMaker.CreateAgencyTable(conn, file);
    }
  }
}