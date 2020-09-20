using System.Collections.Generic;
using System;
using Nixill.GTFS.Entity;
using Nixill.GTFS.Parsing;

namespace Nixill.Testing {
  public class GTFSReaderTest {
    static void Main(string[] args) {
      Console.WriteLine("Loading file...");
      GTFSFile file = GTFSLoader.Load("rest-2020-09-17.gtfs");
      Console.WriteLine("File loaded.");

      GTFSFeedInfo info = file.FeedInfo;
      Console.WriteLine("Feed created by " + info.FeedPublisherName);
      Console.WriteLine("Valid from " + info.StartDate + " to " + info.EndDate);
      Console.WriteLine();

      IDictionary<string, GTFSAgency> agencies = file.Agencies;
      Console.WriteLine("Agencies:");
      foreach (GTFSAgency agency in agencies.Values) {
        Console.WriteLine("(" + agency.ID + ") " + agency.Name);
      }
      Console.WriteLine();

      IDictionary<string, GTFSRoute> routes = file.Routes;
      Console.WriteLine("Routes:");
      foreach (GTFSRoute route in routes.Values) {
        Console.WriteLine("(" + route.ID + ") " + route.RouteType + " " + route.ShortName + " - " + route.LongName);
      }
      Console.WriteLine();

      file.Dispose();
    }
  }
}