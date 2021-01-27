using System;
using Nixill.GTFS.Parsing;

namespace Nixill.Testing {
  public class GTFSReaderTest {
    static void Main(string[] args) {
      Console.WriteLine("Loading file...");
      GTFSFile file = GTFSLoader.Load("rest-2020-09-17.gtfs");
      Console.WriteLine("File loaded.");

      object count = file.GetResult("SELECT COUNT(*) FROM agency;");
      Console.WriteLine("There are " + count + " agency(ies).");

      file.Dispose();
    }
  }
}