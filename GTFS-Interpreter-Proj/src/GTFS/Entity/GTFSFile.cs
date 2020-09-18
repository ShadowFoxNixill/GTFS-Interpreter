using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Parsing;
using Nixill.GTFS.Enumerations;

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

    private GTFSFeedInfo _FeedInfo = null;
    /// <value>
    /// An object corresponding to the <c>feed_info.txt</c> file of the
    /// GTFS.
    /// </value>
    public GTFSFeedInfo FeedInfo => GetFeedInfo();

    private Dictionary<string, GTFSAgency> _Agencies = new Dictionary<string, GTFSAgency>();
    private IDictionary<string, GTFSAgency> _AgenciesRO = null;
    /// <value>
    /// A read-only dictionary of all the agencies in the GTFS. The keys
    /// are the agencies' IDs - if only one agency exists with a key not
    /// specified in <c>agency.txt</c>, the key is "<c>agency</c>".
    /// </value>
    public IDictionary<string, GTFSAgency> Agencies => GetAgencies();

    private Dictionary<string, GTFSRoute> _Routes = new Dictionary<string, GTFSRoute>();
    private IDictionary<string, GTFSRoute> _RoutesRO = null;
    /// <value>
    /// A read-only dictionary of all the routes in the GTFS. The keys are
    /// the routes' IDs.
    /// </value>
    public IDictionary<string, GTFSRoute> Routes => GetRoutes();

    internal GTFSFile(SqliteConnection conn, HashSet<string> files) {
      Conn = conn;
      Files = files;
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

    private IDictionary<string, GTFSAgency> GetAgencies() {
      // If we've already cached the agencies, we don't need to go through
      // this song and dance again.
      if (_AgenciesRO != null) return _AgenciesRO;

      // Otherwise, let's go through this song and dance.
      SqliteCommand cmd = Conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM agency;";
      using SqliteDataReader reader = cmd.ExecuteReader();

      // Populate the list of agencies
      while (reader.Read()) {
        string agencyID = (string)reader["agency_id"];

        // An agency might already exist through the GetAgencyByID method.
        if (_Agencies.ContainsKey(agencyID)) {
          continue;
        }

        // Otherwise let's add it now.
        GTFSAgency agency = new GTFSAgency(Conn, this) {
          ID = agencyID,
          Name = GTFSObjectParser.GetText(reader["agency_name"]),
          URL = GTFSObjectParser.GetUrl(reader["agency_url"]),
          Timezone = GTFSObjectParser.GetTimezone(reader["agency_timezone"]),
          Lang = GTFSObjectParser.GetLanguage(reader["agency_lang"]),
          Phone = GTFSObjectParser.GetPhone(reader["agency_phone"]),
          FareURL = GTFSObjectParser.GetUrl(reader["agency_fare_url"]),
          Email = GTFSObjectParser.GetEmail(reader["agency_email"])
        };

        _Agencies.Add(agencyID, agency);
      }

      cmd.Dispose();

      // Make a read-only wrapper around the dictionary
      _AgenciesRO = new ReadOnlyDictionary<string, GTFSAgency>(_Agencies);

      // And return that.
      return _AgenciesRO;
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
      // First, if we've already retrieved it, just return that.
      if (_Agencies.ContainsKey(agencyID)) {
        return _Agencies[agencyID];
      }

      // Otherwise let's look for it.
      SqliteCommand cmd = Conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM agency WHERE agency_id = @id";
      cmd.Parameters.AddWithValue("@id", agencyID);
      cmd.Prepare();
      SqliteDataReader reader = cmd.ExecuteReader();

      // If we have no result, return null.
      if (!reader.HasRows) {
        return null;
      }

      // But otherwise, let's make the agency.
      reader.Read();
      GTFSAgency agency = new GTFSAgency(Conn, this) {
        ID = agencyID,
        Name = GTFSObjectParser.GetText(reader["agency_name"]),
        URL = GTFSObjectParser.GetUrl(reader["agency_url"]),
        Timezone = GTFSObjectParser.GetTimezone(reader["agency_timezone"]),
        Lang = GTFSObjectParser.GetLanguage(reader["agency_lang"]),
        Phone = GTFSObjectParser.GetPhone(reader["agency_phone"]),
        FareURL = GTFSObjectParser.GetUrl(reader["agency_fare_url"]),
        Email = GTFSObjectParser.GetEmail(reader["agency_email"])
      };

      cmd.Dispose();

      // And add it to the table.
      _Agencies.Add(agencyID, agency);

      // And then output it.
      return agency;
    }

    private IDictionary<string, GTFSRoute> GetRoutes() {
      // If we've already cached the routes, we don't need to go through
      // this song and dance again.
      if (_RoutesRO != null) return _RoutesRO;

      // Otherwise, let's go through this song and dance.
      SqliteCommand cmd = Conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM routes;";
      using SqliteDataReader reader = cmd.ExecuteReader();

      // Populate the list of routes
      while (reader.Read()) {
        string routeID = (string)reader["route_id"];

        // A route might already exist through the GetRouteByID method.
        if (_Routes.ContainsKey(routeID)) {
          continue;
        }

        // Otherwise let's add it now.
        GTFSRoute route = new GTFSRoute(Conn, this) {
          ID = GTFSObjectParser.GetID(reader["route_id"]),
          AgencyID = GTFSObjectParser.GetID(reader["agency_id"]),
          ShortName = GTFSObjectParser.GetText(reader["route_short_name"]),
          LongName = GTFSObjectParser.GetText(reader["route_long_name"]),
          Desc = GTFSObjectParser.GetText(reader["route_desc"]),
          RouteType = (GTFSRouteType)GTFSObjectParser.GetEnum(reader["route_type"]),
          URL = GTFSObjectParser.GetUrl(reader["route_url"]),
          Color = GTFSObjectParser.GetColor(reader["route_color"]),
          TextColor = GTFSObjectParser.GetColor(reader["route_text_color"]),
          SortOrder = GTFSObjectParser.GetInteger(reader["route_sort_order"]),
          ContinuousPickup = (GTFSPickupDropoff)GTFSObjectParser.GetEnum(reader["continuous_pickup"]),
          ContinuousDropOff = (GTFSPickupDropoff)GTFSObjectParser.GetEnum(reader["continuous_drop_off"])
        };

        _Routes.Add(routeID, route);
      }

      cmd.Dispose();

      // Make a read-only wrapper around the dictionary
      _RoutesRO = new ReadOnlyDictionary<string, GTFSRoute>(_Routes);

      // And return that.
      return _RoutesRO;
    }

    /// <summary>
    /// Returns the <c>GTFSRoute</c> defined by the given ID. If there is
    /// no such route, returns <c>null</c>.
    /// </summary>
    /// <param name="routeID">The ID to retrieve.</param>
    public GTFSRoute GetRouteById(string routeID) {
      // First, if we've already retrieved it, just return that.
      if (_Routes.ContainsKey(routeID)) {
        return _Routes[routeID];
      }

      // Otherwise let's look for it.
      SqliteCommand cmd = Conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM routes WHERE route_id = @id";
      cmd.Parameters.AddWithValue("@id", routeID);
      cmd.Prepare();
      SqliteDataReader reader = cmd.ExecuteReader();

      // If we have no result, return null.
      if (!reader.HasRows) {
        return null;
      }

      // But otherwise, let's make the agency.
      reader.Read();
      GTFSRoute route = new GTFSRoute(Conn, this) {
        ID = GTFSObjectParser.GetID(reader["route_id"]),
        AgencyID = GTFSObjectParser.GetID(reader["agency_id"]),
        ShortName = GTFSObjectParser.GetText(reader["route_short_name"]),
        LongName = GTFSObjectParser.GetText(reader["route_long_name"]),
        Desc = GTFSObjectParser.GetText(reader["route_desc"]),
        RouteType = (GTFSRouteType)GTFSObjectParser.GetEnum(reader["route_type"]),
        URL = GTFSObjectParser.GetUrl(reader["route_url"]),
        Color = GTFSObjectParser.GetColor(reader["route_color"]),
        TextColor = GTFSObjectParser.GetColor(reader["route_text_color"]),
        SortOrder = GTFSObjectParser.GetInteger(reader["route_sort_order"]),
        ContinuousPickup = (GTFSPickupDropoff)GTFSObjectParser.GetEnum(reader["continuous_pickup"]),
        ContinuousDropOff = (GTFSPickupDropoff)GTFSObjectParser.GetEnum(reader["continuous_drop_off"])
      };

      // And add it to the table.
      _Routes.Add(routeID, route);

      cmd.Dispose();

      // And then output it.
      return route;
    }
  }
}