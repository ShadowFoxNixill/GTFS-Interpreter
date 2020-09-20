using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Nixill.GTFS.Parsing {
  internal class GTFSValidation {
    internal static void ValidateAgency(SqliteConnection conn, List<GTFSWarning> warnings) {
      // So the thing about the agency table is that a timezone has to be
      // specified by at least one agency, and all of them have to match.
      // Our error handling will be as follows:
      // • No timezone specified: Fail to load.
      // • Some timezones missing: Warn, then fill in default.
      // • Multiple timezones specified: Warn, then select one for all
      //   agencies.
      SqliteCommand cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT DISTINCT agency_timezone FROM agency;";
      SqliteDataReader reader = cmd.ExecuteReader();

      // We can guarantee there will be at least one result, because this
      // point in the code wouldn't be reached if there were nothing.
      bool nullTimezone = false;
      bool multiTimezone = false;
      string timezone = null;
      while (reader.Read()) {
        var tz = reader["agency_timezone"];
        if (tz is DBNull) { nullTimezone = true; }
        else {
          string tzone = (string)tz;
          if (timezone == null) {
            timezone = tzone;
          }
          else {
            multiTimezone = true;
          }
        }
      }

      cmd.Dispose();

      // If no timezone was specified at all...
      if (timezone == null) {
        throw new GTFSParseException("agency_timezone cannot be null for all agencies.");
      }

      // If not all values were the same timezone...
      if (nullTimezone || multiTimezone) {
        // Find all the agencies we're changing
        cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT agency_id, agency_timezone FROM agency WHERE agency_timezone != @tz;";
        cmd.Parameters.AddWithValue("@tz", timezone);
        cmd.Prepare();
        reader = cmd.ExecuteReader();

        // Log their old values
        while (reader.Read()) {
          string agencyID = GTFSObjectParser.GetID(reader["agency_id"]);
          // We don't need to validate or use the actual zone at this
          // point, so text should suffice.
          string oldZone = GTFSObjectParser.GetText(reader["agency_timezone"]);

          if (oldZone == null) {
            oldZone = "null";
          }

          warnings.Add(new GTFSWarning("Timezone " + oldZone + " was changed to conform to the GTFS requirement that all agencies have the same zone.") {
            Table = "agency",
            Record = agencyID,
            Field = "agency_timezone"
          });
        }

        cmd.Dispose();

        // And then...
        cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE agency SET agency_timezone = @tz;";
        cmd.Parameters.AddWithValue("@tz", timezone);
        cmd.Prepare();
        cmd.ExecuteNonQuery();
        cmd.Dispose();
      }
    }
  }
}