using System;
using NodaTime;

namespace Nixill.GTFS.Entity {
  public class GTFSFeedInfo {
    internal GTFSFeedInfo() { }

    public string FeedPublisherName { get; internal set; }
    public Uri FeedPublisherUrl { get; internal set; }
    public string FeedLanguage { get; internal set; }
    public string DefaultLanguage { get; internal set; }
    public LocalDate? StartDate { get; internal set; }
    public LocalDate? EndDate { get; internal set; }
    public string FeedVersion { get; internal set; }
    public string FeedContactEmail { get; internal set; }
    public Uri FeedContactUrl { get; internal set; }
  }
}