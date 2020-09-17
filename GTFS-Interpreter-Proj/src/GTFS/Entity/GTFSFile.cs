using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Parsing;

namespace Nixill.GTFS.Entity {
  public class GTFSFile : IDisposable {
    internal SqliteConnection Conn;
    private GTFSWarnings Warnings;
    private HashSet<string> Files;

    private GTFSFeedInfo _FeedInfo = null;
    public GTFSFeedInfo FeedInfo => GetFeedInfo();

    private Dictionary<string, GTFSAgency> _Agencies = new Dictionary<string, GTFSAgency>();
    private IDictionary<string, GTFSAgency> _AgenciesRO = null;
    public IDictionary<string, GTFSAgency> Agencies => GetAgencies();

    internal GTFSFile(SqliteConnection conn, GTFSWarnings warnings, HashSet<string> files) {
      Conn = conn;
      Warnings = warnings;
      Files = files;
    }

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
      // If we've already cached the agencies, we don't need to go through this song and dance again.
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
  }
}