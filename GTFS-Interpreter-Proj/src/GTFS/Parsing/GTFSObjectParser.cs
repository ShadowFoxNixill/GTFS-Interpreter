using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nixill.Utils;

namespace Nixill.GTFS.Parsing {
  internal static class GTFSObjectParser {
    // All the regex defines
    // Many won't be validated because of flexibility of the types
    private static Regex RgxColorCheck = new Regex(@"^(\#?)([0-9A-Z]{3}(?:[0-9A-Z](?:[0-9A-Z]{2}(?:[0-9A-Z]{2})?)?)?)$", RegexOptions.IgnoreCase);
    private static Regex RgxCurrency = new Regex(@"^([A-Z]{3})$", RegexOptions.IgnoreCase);
    private static Regex RgxDate = new Regex(@"^(\d{4})([-/\. ]?)(\d\d)([-/\. ]?)(\d\d)$");
    private static Regex RgxEmail = new Regex(@"^\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b$", RegexOptions.IgnoreCase);
    private static Regex RgxLanguage = new Regex(@"^[A-Z0-9]{1,8}(-[A-Z0-9]{1,8})*", RegexOptions.IgnoreCase);
    private static Regex RgxFloat = new Regex(@"^-?(0|[1-9]\d*)(\.\d+)?$");
    private static Regex RgxNonNegFloat = new Regex(@"^(0|[1-9]\d*)(\.\d+)?$");
    private static Regex RgxInt = new Regex(@"^-?(0|[1-9]\d*)$");
    private static Regex RgxNonNegInt = new Regex(@"^(0|[1-9]\d*)$");
    private static Regex RgxPosInt = new Regex(@"^([1-9]\d*)$");
    private static Regex RgxTime = new Regex(@"^-?(\d\d?):(\d\d):(\d\d)$");

    internal static object GetObject(GTFSDataType type, string value, ref List<string> warns) {
      // Let's get a match object and an output object ready
      Match match = null;
      string ret = null;
      if (warns == null) {
        warns = new List<string>();
      }

      // Since we're working with SQLite, all non-numbers will be returned as text.
      if (type == GTFSDataType.Color) {
        if (RgxColorCheck.TryMatch(value, out match)) {
          // Drop a # if one is present.
          if (match.Groups[1].Value == "#") {
            warns.Add("The # sign is not part of the GTFS standard and was removed.");
          }

          // Make sure we have a six-digit color code.
          string color = match.Groups[2].Value;
          if (color.Length == 3) {
            warns.Add("Three-digit colors may not be read by all clients, and are converted to six-digit by this parser.");
            ret = "" + color[0] + color[0] + color[1] + color[1] + color[2] + color[2];
          }
          else if (color.Length == 4 || color.Length == 8) {
            warns.Add("Four- or eight-digit colors (RGB plus alpha) cannot be read by this parser.");
            ret = null;
          }
          else {
            ret = color;
          }

          // Make sure capitalization is correct.
          string upVal = ret.ToUpper();
          if (ret != upVal) {
            warns.Add("Color codes are normally uppercase.");
          }
          return upVal;
        }
        else {
          warns.Add("This is not a valid color. Valid colors are six hex digits, without the preceding #.");
          return null;
        }
      }

      else if (type == GTFSDataType.Currency) {
        if (RgxCurrency.TryMatch(value, out match)) {
          string upVal = value.ToUpper();
          if (value != upVal) {
            warns.Add("Currencies are automatically all-caps'd.");
          }
          return upVal;
        }
        else {
          warns.Add("Valid currency values are three letters long.");
          return null;
        }
      }

      else if (type == GTFSDataType.Date) {
        if (RgxDate.TryMatch(value, out match)) {
          // Remove separators between the numbers.
          if (match.Groups[2].Value != "" || match.Groups[4].Value != "") {
            warns.Add("Separators are not valid in GTFS dates and have been automatically removed.");
          }

          // Get the components.
          int year = int.Parse(match.Groups[1].Value);
          int month = int.Parse(match.Groups[3].Value);
          int day = int.Parse(match.Groups[5].Value);

          // Validate the actual date entered.
          if (month > 12 || month == 0) {
            warns.Add("Valid months are 1-12.");
            return null;
          }

          if (day > 31 || day == 0) {
            warns.Add("Valid days are 1-31.");
            return null;
          }

          if (day == 31 && (month == 4 || month == 6 || month == 9 || month == 11)) {
            warns.Add("Valid days for month " + month + " are 1-30.");
            return null;
          }

          if (month == 2) {
            if (day == 31 || day == 30) {
              warns.Add("Valid days for month 2 are 1-28 with 29 permitted some years.");
              return null;
            }

            if (day == 29) {
              // Leap year logic
              if ((year % 400 != 0 && year % 100 == 0) || year % 4 != 0) {
                warns.Add("Valid days for month 2 in year " + year + " are 1-28.");
                return null;
              }
            }
          }

          // Return the date as YYYYMMDD
          return match.Groups[1].Value + match.Groups[3].Value + match.Groups[5].Value;
        }
        else {
          warns.Add(value + " is not a valid date!");
          return null;
        }
      }

      else if (type == GTFSDataType.Email) {
        if (RgxEmail.TryMatch(value, out match)) {
          return value;
        }
        else {
          warns.Add(value + " doesn't appear to be a valid email address, but has been added anyway.");
          return value;
        }
      }

      else if (type == GTFSDataType.Enum || type == GTFSDataType.NonNegativeInteger) {
        if (RgxPosInt.IsMatch(value)) {
          return int.Parse(value);
        }
        else {
          warns.Add(value + " is not a valid non-negative integer.");
          return null;
        }
      }
    }
  }
}
