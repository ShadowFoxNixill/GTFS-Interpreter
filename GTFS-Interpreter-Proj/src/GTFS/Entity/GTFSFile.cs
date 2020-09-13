using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Parsing;

namespace Nixill.GTFS.Entity {
  public class GTFSFile : IDisposable {
    private SqliteConnection Conn;
    private GTFSWarnings Warnings;
    private HashSet<string> Files;

    private GTFSFeedInfo FeedInfo = null;

    internal GTFSFile(SqliteConnection conn, GTFSWarnings warnings, HashSet<string> files) {
      Conn = conn;
      Warnings = warnings;
      Files = files;
    }

    public void Dispose() {
      Conn.Dispose();
    }

    public GTFSFeedInfo GetFeedInfo() {
      // If we already cached it, we don't need it again.
      if (FeedInfo != null) return FeedInfo;

      // If it doesn't exist, we can't return anything.
      if (!Files.Contains("feed_info")) return null;

      // Otherwise, let's build and store it.
      FeedInfo = new GTFSFeedInfo();

      SqliteCommand cmd = Conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM feed_info LIMIT 1;";

      SqliteDataReader reader = cmd.ExecuteReader();
      if (reader.HasRows) {
        reader.Read();
        FeedInfo.FeedPublisherName = GTFSObjectParser.GetText(reader["feed_publisher_name"]);
        FeedInfo.FeedPublisherUrl = GTFSObjectParser.GetUrl(reader["feed_publisher_url"]);
        FeedInfo.FeedLanguage = GTFSObjectParser.GetLanguage(reader["feed_lang"]);
        FeedInfo.DefaultLanguage = GTFSObjectParser.GetLanguage(reader["default_lang"]);
        FeedInfo.StartDate = GTFSObjectParser.GetDate(reader["feed_start_date"]);
        FeedInfo.EndDate = GTFSObjectParser.GetDate(reader["feed_end_date"]);
        FeedInfo.FeedVersion = GTFSObjectParser.GetText(reader["feed_version"]);
        FeedInfo.FeedContactEmail = GTFSObjectParser.GetEmail(reader["feed_contact_email"]);
        FeedInfo.FeedContactUrl = GTFSObjectParser.GetUrl(reader["feed_contact_url"]);
      }

      reader.Close();
      cmd.Dispose();

      return FeedInfo;
    }
  }
}