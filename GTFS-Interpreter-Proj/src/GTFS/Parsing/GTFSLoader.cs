using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Data.Sqlite;

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
      List<GTFSWarning> warnings = new List<GTFSWarning>();

      // And the list of loaded files
      HashSet<string> files = new HashSet<string>();

      // And the file object
      GTFSFile ret = new GTFSFile(conn, files);

      // Now start actually creating tables.
      GTFSEnumerableMaker.Make(conn);
      if (GTFSMaker.CreateAgencyTable(ret, file, warnings)) files.Add("agency");
      if (GTFSMaker.CreateLevelsTable(ret, file, warnings)) files.Add("levels");
      if (GTFSMaker.CreateRoutesTable(ret, file, warnings)) files.Add("routes");
      if (GTFSMaker.CreateStopsTable(ret, file, warnings)) files.Add("stops");
      if (GTFSMaker.CreateShapesTable(ret, file, warnings)) files.Add("shapes");
      if (GTFSMaker.CreateCalendarTable(ret, file, warnings)) files.Add("calendar");
      if (GTFSMaker.CreateCalendarDatesTable(ret, file, warnings)) files.Add("calendar_dates");
      if (GTFSMaker.CreateTripsTable(ret, file, warnings)) files.Add("trips");
      if (GTFSMaker.CreateStopTimesTable(ret, file, warnings)) files.Add("stop_times");
      if (GTFSMaker.CreateFareAttributesTable(ret, file, warnings)) files.Add("fare_attributes");
      if (GTFSMaker.CreateFareRulesTable(ret, file, warnings)) files.Add("fare_rules");
      if (GTFSMaker.CreateFeedInfoTable(ret, file, warnings)) files.Add("feed_info");
      GTFSMaker.CreateWarningsTable(ret, warnings);

      // And output! :D
      return ret;
    }
  }
}