using System;
using NodaTime;

namespace Nixill.GTFS.Entity {
  /// <summary>
  /// Presents the info contained in the <c>feed_info.txt</c> file of the
  /// GTFS.
  /// </summary>
  public class GTFSFeedInfo {
    internal GTFSFeedInfo() { }

    /// <summary>
    /// The value of <c>feed_info.feed_publisher_name</c>.
    /// <para/>
    /// Full name of the organization that publishes the dataset. This
    /// might be the same as one of the <c>agency.agency_name</c> values.
    /// </summary>
    public string FeedPublisherName { get; internal set; }

    /// <summary>
    /// The value of <c>feed_info.feed_publisher_url</c>.
    /// <para/>
    /// URL of the dataset publishing organization's website. This may be
    /// the same as one of the <c>agency.agency_url</c> values.
    /// </summary>
    /// <remarks>
    /// The GTFS spec marks this as a required field; however, this parser
    /// treates this field as optional.
    /// </remarks>
    public Uri FeedPublisherUrl { get; internal set; }

    /// <summary>
    /// The value of <c>feed_info.feed_lang</c>.
    /// <para/>
    /// Default language for the text in this dataset. This setting helps
    /// GTFS consumers choose capitalization rules and other
    /// language-specific settings for the dataset.
    /// </summary>
    public string FeedLanguage { get; internal set; }

    /// <summary>
    /// The value of <c>feed_info.default_lang</c>.
    /// <para/>
    /// Defines the language used when the data consumer doesn’t know the
    /// language of the rider. It's often defined as <c>en</c>, English.
    /// </summary>
    public string DefaultLanguage { get; internal set; }

    /// <summary>
    /// The value of <c>feed_info.feed_start_date</c>.
    /// <para/>
    /// The dataset provides complete and reliable schedule information
    /// for service in the period from the beginning of the
    /// <c>feed_start_date</c> day to the end of the <c>feed_end_date</c>
    /// day.
    /// </summary>
    public LocalDate? StartDate { get; internal set; }

    /// <summary>
    /// The value of <c>feed_info.feed_end_date</c>.
    /// <para/>
    /// The dataset provides complete and reliable schedule information
    /// for service in the period from the beginning of the
    /// <c>feed_start_date</c> day to the end of the <c>feed_end_date</c>
    /// day.
    /// </summary>
    public LocalDate? EndDate { get; internal set; }

    /// <summary>
    /// The value of <c>feed_info.feed_version</c>.
    /// <para/>
    /// String that indicates the current version of their GTFS dataset.
    /// </summary>
    public string FeedVersion { get; internal set; }

    /// <summary>
    /// The value of <c>feed_info.feed_contact_email</c>.
    /// <para/>
    /// Email address for communication regarding the GTFS dataset and
    /// data publishing practices.
    /// </summary>
    public string FeedContactEmail { get; internal set; }

    /// <summary>
    /// The value of <c>feed_info.feed_contact_url</c>.
    /// <para/>
    /// URL for contact information, a web-form, support desk, or other
    /// tools for communication regarding the GTFS dataset and data
    /// publishing practices.
    /// </summary>
    public Uri FeedContactUrl { get; internal set; }
  }
}