using System.IO;
namespace Nixill.GTFS {
  public class GTFSLoader {

    public static GTFSObject Load(string path) {
      // First make sure the path itself exists
      if (!File.Exists(path)) {
        throw new FileNotFoundException("This file does not exist.", path);
      }

      // If the path *is* a database, just load it as a database.
      if (path.EndsWith(".db")) {
        return LoadDB(path);
      }

      // Otherwise check if the database exists.
      if (File.Exists(path + ".db")) {
        // If so, is it newer?
        if (File.GetLastWriteTimeUtc(path + ".db") > File.GetLastWriteTimeUtc(path)) {
          return LoadDB(path + ".db");
        }

        // If it's not newer, we'll have to create the database ourself.
        // Let's delete the old one first 
      }
    }
  }
}