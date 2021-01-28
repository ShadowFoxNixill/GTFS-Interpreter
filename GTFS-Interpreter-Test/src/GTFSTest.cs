using System;
using System.Collections.Generic;
using Nixill.GTFS.Parsing;

namespace Nixill.Testing {
  public class GTFSReaderTest {
    static void Main(string[] args) {
      Console.WriteLine("Loading file...");
      GTFSFile file = GTFSLoader.Load("ddot.gtfs");
      Console.WriteLine("File loaded.");

      object count = file.GetResult("SELECT COUNT(*) FROM trips;");
      Console.WriteLine("There are " + count + " trip(s).");

      Dictionary<string, object> tripsPerTime = file.GetResultDict("SELECT service_id, COUNT(trip_id) FROM trips GROUP BY service_id;");
      foreach (string key in tripsPerTime.Keys) {
        Console.WriteLine(key + ": " + tripsPerTime[key]);
      }

      file.Dispose();
    }
  }
}