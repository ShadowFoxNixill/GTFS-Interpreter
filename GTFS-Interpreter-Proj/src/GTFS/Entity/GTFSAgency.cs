using System;
using Microsoft.Data.Sqlite;
using NodaTime;
using System.Collections.Generic;
using Nixill.GTFS.Parsing;

namespace Nixill.GTFS.Entity {
  /// <summary>
  /// Represents a single transit agency defined in a GTFS file.
  /// </summary>
  public class GTFSAgency {
    private SqliteConnection Conn;
    private GTFSFile File;

    internal GTFSAgency(SqliteConnection conn, GTFSFile file) {
      Conn = conn;
      File = file;
    }

    /// <summary>
    /// The value of <c>agency.agency_id</c> for this agency.
    /// <para/>
    /// Definition: Identifies a transit brand, which is often synonymous
    /// with a transit agency.
    /// <para/>
    /// If using a single-agency GTFS file that doesn't specify the ID,
    /// the parser will assign the ID of "<c>agency</c>" to the agency.
    /// </summary>
    public string ID { get; internal set; }

    /// <summary>
    /// The value of <c>agency.agency_name</c> for this agency.
    /// <para/>
    /// Definition: Full name of the transit agency.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// The value of <c>agency.agency_url</c> for this agency.
    /// <para/>
    /// Definition: URL of the transit agency.
    /// </summary>
    /// <remarks>
    /// The GTFS spec mandates that all agencies have a url. However, this
    /// parser allows agencies to not have URLs.
    /// </remarks>
    public Uri URL { get; internal set; }

    /// <summary>
    /// The value of <c>agency.agency_timezone</c> for this agency.
    /// <para/>
    /// Definition: Timezone where the transit agency is located.
    /// </summary>
    /// <remarks>
    /// All agencies in a single file need to use the same timezone, which
    /// means that the value here might not match what was entered in the
    /// <c>agency.txt</c> file.
    /// </remarks>
    public DateTimeZone Timezone { get; internal set; }

    /// <summary>
    /// The value of <c>agency.agency_lang</c> for this agency.
    /// <para/>
    /// Definition: Primary language used by this transit agency.
    /// </summary>
    public string Lang { get; internal set; }

    /// <summary>
    /// The value of <c>agency.agency_phone</c> for this agency.
    /// <para/>
    /// Definition: A voice telephone number for the specified agency.
    /// This field is a string value that presents the telephone number as
    /// typical for the aggency's service area. It can and should contain
    /// punctuation marks to group the digits of the number. Dialable text
    /// (for example, TriMet's "<c>503-238-RIDE</c>") is permitted, but
    /// the field must not contain any other descriptive text.
    /// </summary>
    public string Phone { get; internal set; }

    /// <summary>
    /// The value of <c>agency.agency_fare_url</c> for this agency.
    /// <para/>
    /// Definition: URL of a web page that allows a rider to purchase
    /// tickets or other fare instruments for that agency online.
    /// </summary>
    public Uri FareURL { get; internal set; }

    /// <summary>
    /// The value of <c>agency.agency_email</c> for this agency.
    /// <para/>
    /// Definition: Email address actively monitored by the agency’s
    /// customer service department. This email address should be a direct
    /// contact point where transit riders can reach a customer service
    /// representative at the agency.
    /// </summary>
    public string Email { get; internal set; }

    private IList<GTFSRoute> _Routes;
    /// <summary>
    /// A list of all the routes operated by this agency.
    /// </summary>
    public IList<GTFSRoute> Routes {
      get {
        // If we've already cached it, just use that.
        if (_Routes != null) {
          return _Routes;
        }

        // Otherwise, we'll need to remake it from scratch.
        List<GTFSRoute> routes = new List<GTFSRoute>();

        SqliteCommand cmd = Conn.CreateCommand();
        cmd.CommandText = "SELECT route_id FROM routes WHERE agency_id = @id ORDER BY route_sort_order;";
        cmd.Parameters.AddWithValue("@id", ID);
        cmd.Prepare();
        SqliteDataReader reader = cmd.ExecuteReader();

        while (reader.Read()) {
          string routeID = GTFSObjectParser.GetID(reader["route_id"]);
          routes.Add(File.GetRouteById(routeID));
        }

        cmd.Dispose();

        // Store an immutable list
        _Routes = routes.AsReadOnly();

        // And return it
        return _Routes;
      }
    }
  }
}