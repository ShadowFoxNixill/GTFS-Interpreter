using System.IO;
using System.IO.Compression;
using Microsoft.Data.Sqlite;

namespace Nixill.GTFS {
  public class GTFSLoader {

    public static GTFSObject Load(string path) {
      // First make sure the path itself exists
      if (!File.Exists(path)) {
        throw new FileNotFoundException("This file does not exist.", path);
      }

      // If the path *is* a database, just load it as a database.
      if (path.EndsWith(".db")) {
        return new GTFSObject(path);
      }

      // Otherwise check if the database exists.
      if (File.Exists(path + ".db")) {
        // If so, is it newer?
        if (File.GetLastWriteTimeUtc(path + ".db") > File.GetLastWriteTimeUtc(path)) {
          return new GTFSObject(path + ".db");
        }

        // If it's not newer, we'll have to create the database ourself.
        // Let's delete the old one first.
        File.Delete(path + ".db");
      }

      // So now we have to populate the database pretty much from scratch.
      // Fortunately, we've got a template database to start with.
      File.Copy(@"res\gtfs.db", path + ".db");

      string connStr = new SqliteConnectionStringBuilder("") {
        DataSource = path + ".db",
        ForeignKeys = true
      }.ToString();

      using SqliteConnection conn = new SqliteConnection(connStr);

      // Open the zip file
      using ZipArchive file = ZipFile.OpenRead(path);


    }
  }
}