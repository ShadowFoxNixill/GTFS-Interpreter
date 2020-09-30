using System;
using Microsoft.Data.Sqlite;
using NodaTime;
using System.Collections.Generic;
using Nixill.GTFS.Parsing;
using Nixill.SQLite;
using Nixill.Utils;

namespace Nixill.GTFS.Entity {
  /// <summary>
  /// Represents a single transit agency defined in a GTFS file.
  /// </summary>
  public sealed class GTFSAgency : GTFSEntity {
    internal override string TableName => "agency";
    internal override string TableIDCol => "agency_id";

    internal GTFSAgency(SqliteConnection conn, string id) : base(conn, id) { }

    /// <summary>
    /// The value of <c>agency.agency_name</c> for this agency.
    /// <para/>
    /// Definition: Full name of the transit agency.
    /// </summary>
    public string Name => GTFSObjectParser.GetText(Select("agency_name"));

    /// <summary>
    /// The value of <c>agency.agency_url</c> for this agency.
    /// <para/>
    /// Definition: URL of the transit agency.
    /// </summary>
    /// <remarks>
    /// The GTFS spec mandates that all agencies have a url. However, this
    /// parser allows agencies to not have URLs.
    /// </remarks>
    public Uri URL => GTFSObjectParser.GetUrl(Select("agency_url"));

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
    public DateTimeZone Timezone => GTFSObjectParser.GetTimezone(Select("agency_timezone"));

    /// <summary>
    /// The value of <c>agency.agency_lang</c> for this agency.
    /// <para/>
    /// Definition: Primary language used by this transit agency.
    /// </summary>
    public string Lang => GTFSObjectParser.GetLanguage(Select("agency_lang"));

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
    public string Phone => GTFSObjectParser.GetPhone(Select("agency_phone"));

    /// <summary>
    /// The value of <c>agency.agency_fare_url</c> for this agency.
    /// <para/>
    /// Definition: URL of a web page that allows a rider to purchase
    /// tickets or other fare instruments for that agency online.
    /// </summary>
    public Uri FareURL => GTFSObjectParser.GetUrl(Select("agency_fare_url"));

    /// <summary>
    /// The value of <c>agency.agency_email</c> for this agency.
    /// <para/>
    /// Definition: Email address actively monitored by the agency’s
    /// customer service department. This email address should be a direct
    /// contact point where transit riders can reach a customer service
    /// representative at the agency.
    /// </summary>
    public string Email => GTFSObjectParser.GetEmail(Select("agency_email"));

    /// <summary>
    /// A list of all the routes operated by this agency.
    /// </summary>
    public IList<GTFSRoute> Routes => Conn.GetResultList($"SELECT route_id FROM routes WHERE agency_id = @p0;", ID)
      .Transform((obj) => new GTFSRoute(Conn, GTFSObjectParser.GetID(obj)));
  }
}