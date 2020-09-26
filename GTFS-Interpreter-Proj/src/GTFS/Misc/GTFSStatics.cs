using NodaTime.Text;

namespace Nixill.GTFS.Misc {
  public static class GTFSStatics {
    public static LocalDatePattern GTFSDatePattern = LocalDatePattern.CreateWithInvariantCulture("yyyyMMdd");
  }
}