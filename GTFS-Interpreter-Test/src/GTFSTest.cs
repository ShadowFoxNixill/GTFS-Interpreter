using System;
using Nixill.GTFS.Entity;
using Nixill.GTFS.Parsing;

namespace Nixill.Testing {
  public class GTFSReaderTest {
    static void Main(string[] args) {
      Console.WriteLine("Loading file...");
      GTFSFile file = GTFSLoader.Load("nxs-2020-01-06v4.gtfs");
      Console.WriteLine("File loaded.");

      GTFSFeedInfo info = file.GetFeedInfo();
      Console.WriteLine("Feed created by " + info.FeedPublisherName);
      Console.WriteLine("Valid from " + info.StartDate + " to " + info.EndDate);

      file.Dispose();
    }
  }
}