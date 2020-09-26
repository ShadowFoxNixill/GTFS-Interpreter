using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Parsing;
using Nixill.GTFS.Enumerations;
using Nixill.SQLite;

namespace Nixill.GTFS.Entity {
  /// <summary>
  /// Represents a container for the contents of a single GTFS file.
  /// </summary>
  /// <remarks>
  /// <c>GTFSFile</c> doesn't contain a public constructor; to get a
  /// reference to a <c>GTFSFile</c> object, use
  /// <link cref="GTFSLoader.Load(string)"><c>GTFSLoader.Load()</c></link>.
  /// </remarks>
  public class GTFSFile : IDisposable {
    internal SqliteConnection Conn;
    private HashSet<string> Files;

    internal GTFSFile(SqliteConnection conn, HashSet<string> files) {
      Conn = conn;
      Files = files;
    }

    private GTFSFeedInfo _FeedInfo = null;
    /// <value>
    /// An object corresponding to the <c>feed_info.txt</c> file of the
    /// GTFS.
    /// </value>
    public GTFSFeedInfo FeedInfo => GetFeedInfo();

    /// <value>
    /// A read-only dictionary of all the agencies in the GTFS. The keys
    /// are the agencies' IDs - if only one agency exists with a key not
    /// specified in <c>agency.txt</c>, the key is "<c>agency</c>".
    /// </value>
    public IDictionary<string, GTFSAgency> Agencies {
      get {
        Dictionary<string, GTFSAgency> ret = new Dictionary<string, GTFSAgency>();

        foreach (object obj in Conn.GetResultList("SELECT agency_id FROM agency;")) {
          string id = GTFSObjectParser.GetID(obj);
          ret.Add(id, new GTFSAgency(Conn, id));
        }

        return new ReadOnlyDictionary<string, GTFSAgency>(ret);
      }
    }

    /// <value>
    /// A read-only dictionary of all the routes in the GTFS. The keys are
    /// the routes' IDs.
    /// </value>
    public IDictionary<string, GTFSRoute> Routes {
      get {
        Dictionary<string, GTFSRoute> ret = new Dictionary<string, GTFSRoute>();

        foreach (object obj in Conn.GetResultList("SELECT route_id FROM routes ORDER BY route_sort_order;")) {
          string id = GTFSObjectParser.GetID(obj);
          ret.Add(id, new GTFSRoute(Conn, id));
        }

        return new ReadOnlyDictionary<string, GTFSRoute>(ret);
      }
    }

    /// <value>
    /// A read-only dictionary of all the stops in the GTFS. The keys are
    /// the stops' IDs.
    /// </value>
    public IDictionary<string, GTFSStop> Stops {
      get {
        Dictionary<string, GTFSStop> ret = new Dictionary<string, GTFSStop>();

        foreach (object obj in Conn.GetResultList("SELECT stop_id FROM stops;")) {
          string id = GTFSObjectParser.GetID(obj);
          ret.Add(id, new GTFSStop(Conn, id));
        }

        return new ReadOnlyDictionary<string, GTFSStop>(ret);
      }
    }

    /// <value>
    /// A read-only dictionary of all the fare zones in the GTFS. The keys
    /// are the zones' IDs.
    /// </value>
    public IDictionary<string, GTFSFareZone> FareZones {
      get {
        Dictionary<string, GTFSFareZone> ret = new Dictionary<string, GTFSFareZone>();

        foreach (object obj in Conn.GetResultList("SELECT zone_id FROM fare_zones;")) {
          string id = GTFSObjectParser.GetID(obj);
          ret.Add(id, new GTFSFareZone(Conn, id));
        }

        return new ReadOnlyDictionary<string, GTFSFareZone>(ret);
      }
    }

    /// <value>
    /// A read-only dictionary of all the shapes in the GTFS. The keys are
    /// the shapes' IDs.
    /// </value>
    public IDictionary<string, GTFSShape> Shapes {
      get {
        Dictionary<string, GTFSShape> ret = new Dictionary<string, GTFSShape>();

        foreach (object obj in Conn.GetResultList("SELECT shape_id FROM shape_ids;")) {
          string id = GTFSObjectParser.GetID(obj);
          ret.Add(id, new GTFSShape(Conn, id));
        }

        return new ReadOnlyDictionary<string, GTFSShape>(ret);
      }
    }

    /// <value>
    /// A read-only dictionary of all the calendars in the GTFS. The keys
    /// are the calendars' IDs.
    /// </value>
    public IDictionary<string, GTFSCalendar> Calendars {
      get {
        Dictionary<string, GTFSCalendar> ret = new Dictionary<string, GTFSCalendar>();

        foreach (object obj in Conn.GetResultList("SELECT service_id FROM calendar_services;")) {
          string id = GTFSObjectParser.GetID(obj);
          ret.Add(id, new GTFSCalendar(Conn, id));
        }

        return new ReadOnlyDictionary<string, GTFSCalendar>(ret);
      }
    }

    /// <summary>
    /// Disposes this <c>GTFSFile</c>, closing its internal database
    /// connection.
    /// </summary>
    public void Dispose() {
      Conn.Dispose();
    }

    private GTFSFeedInfo GetFeedInfo() {
      // If we already cached it, we don't need it again.
      if (_FeedInfo != null) return _FeedInfo;

      // If it doesn't exist, we can't return anything.
      if (!Files.Contains("feed_info")) return null;

      // Otherwise, let's build and store it.
      _FeedInfo = new GTFSFeedInfo();

      SqliteCommand cmd = Conn.CreateCommand();
      // We'll just retrieve one row because that's all feed_info is
      // supposed to have.
      cmd.CommandText = "SELECT * FROM feed_info LIMIT 1;";

      SqliteDataReader reader = cmd.ExecuteReader();
      if (reader.HasRows) {
        reader.Read();
        _FeedInfo.FeedPublisherName = GTFSObjectParser.GetText(reader["feed_publisher_name"]);
        _FeedInfo.FeedPublisherUrl = GTFSObjectParser.GetUrl(reader["feed_publisher_url"]);
        _FeedInfo.FeedLanguage = GTFSObjectParser.GetLanguage(reader["feed_lang"]);
        _FeedInfo.DefaultLanguage = GTFSObjectParser.GetLanguage(reader["default_lang"]);
        _FeedInfo.StartDate = GTFSObjectParser.GetDate(reader["feed_start_date"]);
        _FeedInfo.EndDate = GTFSObjectParser.GetDate(reader["feed_end_date"]);
        _FeedInfo.FeedVersion = GTFSObjectParser.GetText(reader["feed_version"]);
        _FeedInfo.FeedContactEmail = GTFSObjectParser.GetEmail(reader["feed_contact_email"]);
        _FeedInfo.FeedContactUrl = GTFSObjectParser.GetUrl(reader["feed_contact_url"]);
      }

      reader.Close();
      cmd.Dispose();

      return _FeedInfo;
    }

    /// <summary>
    /// Returns the <c>GTFSAgency</c> defined by the given ID. If there is
    /// no such agency, returns <c>null</c>.
    /// <para/>
    /// Note that for GTFS files with only one agency, which don't use an
    /// <c>agency_id</c> column, this parser assigns the ID of
    /// "<c>agency</c>" to that agency.
    /// </summary>
    /// <param name="agencyID">The ID to retrieve.</param>
    public GTFSAgency GetAgencyById(string agencyID) {
      if (Conn.GetResult("SELECT agency_id FROM agency WHERE agency_id = @p;", agencyID) != null) {
        return new GTFSAgency(Conn, agencyID);
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Returns the <c>GTFSRoute</c> defined by the given ID. If there is
    /// no such route, returns <c>null</c>.
    /// </summary>
    /// <param name="routeID">The ID to retrieve.</param>
    public GTFSRoute GetRouteById(string routeID) {
      if (Conn.GetResult("SELECT route_id FROM routes WHERE route_id = @p;", routeID) != null) {
        return new GTFSRoute(Conn, routeID);
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Returns the <c>GTFSStop</c> defined by the given ID. If there is
    /// no such route, returns <c>null</c>.
    /// </summary>
    /// <param name="stopID">The ID to retrieve.</param>
    public GTFSStop GetStopById(string stopID) {
      if (Conn.GetResult("SELECT stop_id FROM stop WHERE stop_id = @p;", stopID) != null) {
        return new GTFSStop(Conn, stopID);
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Returns the <c>GTFSFareZone</c> defined by the given ID. If there
    /// is no such fare zone, returns <c>null</c>.
    /// </summary>
    /// <param name="zoneID">The ID to retrieve.</param>
    public GTFSFareZone GetFareZoneById(string zoneID) {
      if (Conn.GetResult("SELECT zone_id FROM fare_zones WHERE zone_id = @p;", zoneID) != null) {
        return new GTFSFareZone(Conn, zoneID);
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Returns the <c>GTFSShape</c> defined by the given ID. If there is
    /// no such shape, returns <c>null</c>.
    /// </summary>
    /// <param name="shapeID">The ID to retrieve.</param>
    public GTFSShape GetShapeById(string shapeID) {
      if (Conn.GetResult("SELECT shape_id FROM shape_ids WHERE shape_id = @p;", shapeID) != null) {
        return new GTFSShape(Conn, shapeID);
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Returns the <c>GTFSCalendar</c> defined by the given ID. If there
    /// is no such calendar, returns <c>null</c>.
    /// </summary>
    /// <param name="calendarID">The ID to retrieve.</param>
    public GTFSCalendar GetCalendarById(string calendarID) {
      if (Conn.GetResult("SELECT service_id FROM calendar_services WHERE service_id = @p;", calendarID) != null) {
        return new GTFSCalendar(Conn, calendarID);
      }
      else {
        return null;
      }
    }
  }
}