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

      /*
      Console.Write("Enter an agency ID: ");
      string id = Console.ReadLine();

      GTFSAgency agencyById = file.GetAgencyById(id);
      Console.WriteLine("That is the agency named " + ((agencyById?.Name) ?? "(null)") + ".");
      //*/

      file.Dispose();
    }
  }
}