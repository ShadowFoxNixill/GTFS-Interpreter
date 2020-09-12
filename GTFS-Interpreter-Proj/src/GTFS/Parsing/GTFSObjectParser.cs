using System.Text.RegularExpressions;
using Nixill.Utils;

namespace Nixill.GTFS.Parsing {
  internal static class GTFSObjectParser {
    // All the regex defines
    // Many won't be validated because of flexibility of the types
    private static Regex RgxColorCheck = new Regex(@"^(\#?)((?:[0-9A-Z]{3,4}){1,2})$", RegexOptions.IgnoreCase);
    private static Regex RgxCurrency = new Regex(@"^([A-Z]{3})$", RegexOptions.IgnoreCase);
    private static Regex RgxDate = new Regex(@"^(\d{4})(\d\d)(\d\d)$");
    private static Regex RgxEmail = new Regex(@"^\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b$", RegexOptions.IgnoreCase);
    private static Regex RgxLanguage = new Regex(@"^[A-Z0-9]{1,8}(-[A-Z0-9]{1,8})*", RegexOptions.IgnoreCase);
    private static Regex RgxFloat = new Regex(@"^-?(0|[1-9]\d*)(\.\d+)?$");
    private static Regex RgxNonNegFloat = new Regex(@"^(0|[1-9]\d*)(\.\d+)?$");
    private static Regex RgxInt = new Regex(@"^-?(0|[1-9]\d*)$");
    private static Regex RgxNonNegInt = new Regex(@"^(0|[1-9]\d*)$");
    private static Regex RgxPosInt = new Regex(@"^([1-9]\d*)$");
    private static Regex RgxTime = new Regex(@"^-?(\d\d?):(\d\d):(\d\d)$");

    internal static object GetObject(GTFSDataType type, string value, ref string warning) {
      // Let's get a match object ready
      Match match = null;

      // Since we're working with SQLite, all non-numbers will be returned as text.
      switch (type) {
        case GTFSDataType.Color:
          if (RgxColorCheck.TryMatch(value, out match)) {

          }
      }
    }
  }
}