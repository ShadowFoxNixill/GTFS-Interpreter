using System.Text.RegularExpressions;
using Nixill.Utils;

namespace Nixill.GTFS.Parsing {
  internal static class GTFSObjectParser {
    // All the regex defines
    private static Regex RgxColorCheck = new Regex(@"^(\#?)((?:[0-9A-Z]{3,4}){1,2})$", RegexOptions.IgnoreCase);
    private static Regex RgxCurrency = new Regex(@"^([A-Z]{3})$", RegexOptions.IgnoreCase);
    private static Regex RgxDate = new Regex(@"^(\d{4})(\d\d)(\d\d)$");
    private static Regex RgxEmail = new Regex(@"^\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b$", RegexOptions.IgnoreCase);
    private static Regex RgxLanguage = new Regex(@"")

    internal static object GetObject(GTFSDataType type, bool required, string value, ref string warning) {
      // Since we're working with SQLite, all non-numbers will be returned as text.
      if (type == GTFSDataType.Color) {
        if (Regex.IsMatch(value, @"^[0-9A-Fa-f]{6}$")) {
          return value.ToUpper();
        }
        else if (Regex.IsMatch(value, @"^[0-9A-Fa-f]{3}$")) {
          warning = "Color value using three hex digits (automatically expanded to six)";
          return new string(new char[] { value[0], value[0], value[1], value[1], value[2], value[2] }).ToUpper();
        }
        else if (Regex.IsMatch(value, @"^\#[0-9A-Fa-f]{6}$")) {
          warning = "Color value using preceding # (stripped out)";
          return value[1..6].ToUpper();
        }
        else if (Regex.IsMatch(value, @"^\#[0-9A-Fa-f]{6}$")) {
          warning = "Color value using preceding # (stripped out) and three hex digits (automatically expanded to six)";
          return new string(new char[] { value[1], value[1], value[2], value[2], value[3], value[3] }).ToUpper();
        }
        else if (Regex.IsMatch(value, @"^\#?([0-9A-Fa-f]{4}){1,2}$")) {
          warning = "Color value includes alpha - can't parse (unknown if RGBA or ARGB), using null instead.";
          return null;
        }
        else {
          warning = "Invalid color value. Using null instead.";
          return null;
        }
      }
      else if (type == GTFSDataType.Currency) {
        if (Regex.IsMatch(value, @"^[A-Z]{3}$")) {
          return value;
        }
        else if (Regex.IsMatch(value, @"^[A-Za-z]{3}$")) {
          warning = "Changing currency to all caps.";
          return value.ToUpper();
        }
      }
      else if (type == GTFSDataType.Date) {
        if (Regex.IsMatch(value, @"^\d{8}$")) {

        }
      }
    }
  }
}