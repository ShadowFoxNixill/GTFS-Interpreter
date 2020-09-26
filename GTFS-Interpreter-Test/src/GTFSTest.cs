using System.Collections.Generic;
using System;
using Nixill.GTFS.Entity;
using Nixill.GTFS.Parsing;
using Nixill.GTFS.Misc;
using NodaTime.Text;
using NodaTime;

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

      int count = 0;

      IDictionary<string, GTFSAgency> agencies = file.Agencies;
      Console.WriteLine("Agencies:");
      foreach (GTFSAgency agency in agencies.Values) {
        if (count == 10) {
          Console.WriteLine("... and " + (agencies.Count - 10) + " more.");
          break;
        }
        else {
          Console.WriteLine("(" + agency.ID + ") " + agency.Name);
          count++;
        }
      }
      Console.WriteLine();

      count = 0;

      IDictionary<string, GTFSRoute> routes = file.Routes;
      Console.WriteLine("Routes:");
      foreach (GTFSRoute route in routes.Values) {
        if (count == 10) {
          Console.WriteLine("... and " + (routes.Count - 10) + " more.");
          break;
        }
        else {
          Console.WriteLine("(" + route.ID + ") " + route.RouteType + " " + route.ShortName + " - " + route.LongName);
          count++;
        }
      }
      Console.WriteLine();

      count = 0;

      IDictionary<string, GTFSStop> stops = file.Stops;
      Console.WriteLine("Stops:");
      foreach (GTFSStop stop in stops.Values) {
        if (count == 10) {
          Console.WriteLine("... and " + (stops.Count - 10) + " more.");
          break;
        }
        else {
          Console.Write("(" + stop.ID + ") " + stop.Name + " is a " + stop.Type);
          Coordinates? coords = stop.StopCoords;
          if (coords.HasValue) {
            Console.Write(" at " + coords.Value.Latitude + ", " + coords.Value.Longitude);
          }
          Console.WriteLine();
          count++;
        }
      }
      Console.WriteLine();

      count = 0;

      IDictionary<string, GTFSShape> shapes = file.Shapes;
      Console.WriteLine("Shapes:");
      foreach (GTFSShape shape in shapes.Values) {
        if (count == 10) {
          Console.WriteLine("... and " + (shapes.Count - 10) + " more.");
          break;
        }
        else {
          Console.WriteLine(shape.ID + ": " + shape.PointCount + " point(s), " + shape.Length + " in length.");
          count++;
        }
      }

      count = 0;

      Console.WriteLine();

      IDictionary<string, GTFSCalendar> calendars = file.Calendars;
      LocalDatePattern iso = LocalDatePattern.Iso;
      LocalDate today = SystemClock.Instance.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()).LocalDateTime.Date;
      Console.WriteLine("Today is " + iso.Format(today));
      Console.WriteLine("Calendars:");
      foreach (GTFSCalendar calendar in calendars.Values) {
        if (count == 10) {
          Console.WriteLine("... and " + (calendars.Count - 10) + " more.");
          break;
        }
        else {
          Console.Write(calendar.ID + ": ");
          if (calendar.IsActive(today)) {
            Console.WriteLine("Active today");
          }
          else {
            Console.WriteLine("Inactive today");
          }
          count++;
        }
      }

      count = 0;

      file.Dispose();
    }
  }
}