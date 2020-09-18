using System.Globalization;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nixill.Utils;
using NodaTime;

namespace Nixill.GTFS.Parsing {
  internal enum GTFSDataType {
    Color, Currency, Date, Email, Enum, ID, Language, Latitude, Longitude, Float, NonNegativeFloat,
    PositiveFloat, Integer, NonNegativeInteger, PositiveInteger, Phone, Time, Text, Timezone, Url
  }

  internal static class GTFSObjectParser {
    // All the regex defines
    // Many won't be validated because of flexibility of the types
    private static Regex RgxColorCheck = new Regex(@"^(\#?)([0-9A-Z]{3}(?:[0-9A-Z](?:[0-9A-Z]{2}(?:[0-9A-Z]{2})?)?)?)$", RegexOptions.IgnoreCase);
    private static Regex RgxColorTest = new Regex(@"^([0-9A-F]{2})([0-9A-F]{2})([0-9A-F]{2})$");
    private static Regex RgxCurrency = new Regex(@"^([A-Z]{3})$", RegexOptions.IgnoreCase);
    private static Regex RgxDate = new Regex(@"^(\d{4})([-/\. ]?)(\d\d)([-/\. ]?)(\d\d)$");
    private static Regex RgxEmail = new Regex(@"^\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b$", RegexOptions.IgnoreCase);
    private static Regex RgxLanguage = new Regex(@"^[A-Z0-9]{1,8}(-[A-Z0-9]{1,8})*", RegexOptions.IgnoreCase);
    private static Regex RgxTime = new Regex(@"^(\d+):(\d\d):(\d\d)$");

    private static IDateTimeZoneProvider Tzdb = DateTimeZoneProviders.Tzdb;

    internal static object GetObject(GTFSDataType type, string value, ref List<GTFSWarning> warns) {
      // Let's get a match object and an output object ready
      Match match = null;
      string ret = null;
      if (warns == null) {
        warns = new List<GTFSWarning>();
      }

      // Blank values should just be null.
      if (value == "") {
        return null;
      }

      // Since we're working with SQLite, all non-numbers will be returned as text.
      if (type == GTFSDataType.Color) {
        if (RgxColorCheck.TryMatch(value, out match)) {
          // Drop a # if one is present.
          if (match.Groups[1].Value == "#") {
            warns.Add(new GTFSWarning("The # sign is not part of the GTFS standard and was removed."));
          }

          // Make sure we have a six-digit color code.
          string color = match.Groups[2].Value;
          if (color.Length == 3) {
            warns.Add(new GTFSWarning("Three-digit colors may not be read by all clients, and are converted to six-digit by this parser."));
            ret = "" + color[0] + color[0] + color[1] + color[1] + color[2] + color[2];
          }
          else if (color.Length == 4 || color.Length == 8) {
            warns.Add(new GTFSWarning("Four- or eight-digit colors (RGB plus alpha) cannot be read by this parser."));
            ret = null;
          }
          else {
            ret = color;
          }

          // Make sure capitalization is correct.
          string upVal = ret.ToUpper();
          if (ret != upVal) {
            warns.Add(new GTFSWarning("Color codes are normally uppercase."));
          }
          return upVal;
        }
        else {
          warns.Add(new GTFSWarning("This is not a valid color. Valid colors are six hex digits, without the preceding #."));
          return null;
        }
      }

      else if (type == GTFSDataType.Currency) {
        if (RgxCurrency.TryMatch(value, out match)) {
          string upVal = value.ToUpper();
          if (value != upVal) {
            warns.Add(new GTFSWarning("Currencies are automatically all-caps'd."));
          }
          return upVal;
        }
        else {
          warns.Add(new GTFSWarning("Valid currency values are three letters long."));
          return null;
        }
      }

      else if (type == GTFSDataType.Date) {
        if (RgxDate.TryMatch(value, out match)) {
          // Remove separators between the numbers.
          if (match.Groups[2].Value != "" || match.Groups[4].Value != "") {
            warns.Add(new GTFSWarning("Separators are not valid in GTFS dates and have been automatically removed."));
          }

          // Get the components.
          int year = int.Parse(match.Groups[1].Value);
          int month = int.Parse(match.Groups[3].Value);
          int day = int.Parse(match.Groups[5].Value);

          // Validate the actual date entered.
          if (month > 12 || month == 0) {
            warns.Add(new GTFSWarning("Valid months are 1-12."));
            return null;
          }

          if (day > 31 || day == 0) {
            warns.Add(new GTFSWarning("Valid days are 1-31."));
            return null;
          }

          if (day == 31 && (month == 4 || month == 6 || month == 9 || month == 11)) {
            warns.Add(new GTFSWarning("Valid days for month " + month + " are 1-30."));
            return null;
          }

          if (month == 2) {
            if (day == 31 || day == 30) {
              warns.Add(new GTFSWarning("Valid days for month 2 are 1-28 with 29 permitted some years."));
              return null;
            }

            if (day == 29) {
              // Leap year logic
              if ((year % 400 != 0 && year % 100 == 0) || year % 4 != 0) {
                warns.Add(new GTFSWarning("Valid days for month 2 in year " + year + " are 1-28."));
                return null;
              }
            }
          }

          // Return the date as YYYYMMDD
          return match.Groups[1].Value + match.Groups[3].Value + match.Groups[5].Value;
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid date!"));
          return null;
        }
      }

      else if (type == GTFSDataType.Email) {
        if (RgxEmail.TryMatch(value, out match)) {
          return value;
        }
        else {
          warns.Add(new GTFSWarning(value + " doesn't appear to be a valid email address, but has been added anyway."));
          return value;
        }
      }

      else if (type == GTFSDataType.Enum || type == GTFSDataType.NonNegativeInteger) {
        if (int.TryParse(value, out int val)) {
          if (val >= 0) {
            return val;
          }
          else {
            warns.Add(new GTFSWarning(value + " is not a valid non-negative integer (is negative)."));
            return null;
          }
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid non-negative integer (is not a number)."));
          return null;
        }
      }

      else if (type == GTFSDataType.Float) {
        if (double.TryParse(value, out double val)) {
          return val;
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid floating-point number."));
          return null;
        }
      }

      else if (type == GTFSDataType.ID) {
        return value;
      }

      else if (type == GTFSDataType.Integer) {
        if (int.TryParse(value, out int val)) {
          return val;
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid integer."));
          return null;
        }
      }

      else if (type == GTFSDataType.Language) {
        if (RgxLanguage.IsMatch(value)) {
          return value;
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid language code."));
          return null;
        }
      }

      else if (type == GTFSDataType.Latitude) {
        if (float.TryParse(value, out float lat)) {
          if (lat < -90 || lat > 90) {
            warns.Add(new GTFSWarning(value + " is not a valid latitude (out of range)."));
            return null;
          }
          else {
            return lat;
          }
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid latitude (not a number)."));
          return null;
        }
      }

      else if (type == GTFSDataType.Longitude) {
        if (float.TryParse(value, out float lon)) {
          if (lon < -180 || lon > 180) {
            lon %= 360;
            if (lon > 180) {
              lon -= 360;
            }
            warns.Add(new GTFSWarning("Longitude was corrected to " + lon + " (was " + value + ")"));
          }
          return lon;
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid longitude (not a number)."));
          return null;
        }
      }

      else if (type == GTFSDataType.NonNegativeFloat) {
        if (double.TryParse(value, out double val)) {
          if (val >= 0) {
            return double.Parse(value);
          }
          else {
            warns.Add(new GTFSWarning(value + " is not a valid non-negative floating-point number (is negative)."));
          }
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid non-negative floating-point number (is not a number)."));
          return null;
        }
      }

      // GTFSDataType.NonNegativeInteger: See GTFSDataType.Enum.

      else if (type == GTFSDataType.Phone) {
        // I do *not* feel like doing a single ounce of validation here.
        // If someone wishes to change this, a pull request is welcomed.
        return value;
      }

      else if (type == GTFSDataType.PositiveFloat) {
        if (float.TryParse(value, out float val)) {
          if (val > 0) {
            return val;
          }
          else if (val == 0) {
            warns.Add(new GTFSWarning(value + " is not a valid positive floating-point number (is zero)."));
            return null;
          }
          else {
            warns.Add(new GTFSWarning(value + " is not a valid positive floating-point number (is negative)."));
            return null;
          }
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid positive floating-point number (is not a number)."));
          return null;
        }
      }

      else if (type == GTFSDataType.PositiveInteger) {
        if (int.TryParse(value, out int val)) {
          if (val > 0) {
            return val;
          }
          else if (val == 0) {
            warns.Add(new GTFSWarning(value + " is not a valid positive integer (is zero)."));
            return null;
          }
          else {
            warns.Add(new GTFSWarning(value + " is not a valid positive integer (is negative)."));
            return null;
          }
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid positive integer (is not a number)."));
          return null;
        }
      }

      else if (type == GTFSDataType.Text) {
        return value;
      }

      else if (type == GTFSDataType.Time) {
        if (RgxTime.TryMatch(value, out match)) {
          if (match.Groups[1].Length == 1) {
            // I'm not going to add warnings here
            // Because that would completely flood the log
            return "0" + value;
          }
          else {
            return value;
          }
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid time."));
          return null;
        }
      }

      else if (type == GTFSDataType.Timezone) {
        // Note that this is case sensitive.
        // I don't want it to be, but I also don't feel like doing a
        // binary search on all possible capitalizations until I find one
        // that matches.
        if (DateTimeZoneProviders.Tzdb.GetZoneOrNull(value) != null) {
          return value;
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid timezone."));
          return null;
        }
      }

      else if (type == GTFSDataType.Url) {
        Uri uriTest;
        if (Uri.TryCreate(value, UriKind.Absolute, out uriTest)) {
          if (uriTest.Scheme == Uri.UriSchemeHttp) {
            warns.Add(new GTFSWarning(value + " is an http (unsecured) URL."));
            return value;
          }
          if (uriTest.Scheme == Uri.UriSchemeHttps) {
            return value;
          }
          else {
            warns.Add(new GTFSWarning(value + " is not a valid URL."));
            return null;
          }
        }
        else {
          warns.Add(new GTFSWarning(value + " is not a valid URI."));
        }
      }

      return null;
    }

    internal static Color? GetColor(object input) {
      if (input == null || input is DBNull) return null;
      Match match = RgxColorTest.Match((string)input);
      int red = NumberUtils.StringToInt(match.Groups[1].Value, 16);
      int green = NumberUtils.StringToInt(match.Groups[2].Value, 16);
      int blue = NumberUtils.StringToInt(match.Groups[3].Value, 16);
      return Color.FromArgb(red, green, blue);
    }

    internal static string GetCurrency(object input) {
      if (input == null || input is DBNull) return null;
      return (string)input;
    }

    internal static LocalDate? GetDate(object input) {
      if (input == null || input is DBNull) return null;
      Match match = RgxDate.Match((string)input);
      int year = int.Parse(match.Groups[1].Value);
      int month = int.Parse(match.Groups[3].Value);
      int day = int.Parse(match.Groups[5].Value);
      return new LocalDate(year, month, day);
    }

    internal static string GetEmail(object input) {
      if (input == null || input is DBNull) return null;
      return (string)input;
    }

    internal static int? GetEnum(object input) {
      if (input == null || input is DBNull) return null;
      return (int)(long)input;
    }

    internal static string GetID(object input) {
      if (input == null || input is DBNull) return null;
      return (string)input;
    }

    internal static string GetLanguage(object input) {
      if (input == null || input is DBNull) return null;
      return (string)input;
    }

    internal static double? GetFloat(object input) {
      if (input == null || input is DBNull) return null;
      return (double)input;
    }

    internal static int? GetInteger(object input) {
      if (input == null || input is DBNull) return null;
      return (int)(long)input;
    }

    internal static string GetPhone(object input) {
      if (input == null || input is DBNull) return null;
      return (string)input;
    }

    internal static string GetText(object input) {
      if (input == null || input is DBNull) return null;
      return (string)input;
    }

    internal static Duration? GetTime(object input) {
      if (input == null || input is DBNull) return null;
      Match match = RgxTime.Match((string)input);
      int hour = int.Parse(match.Groups[1].Value);
      int minute = int.Parse(match.Groups[2].Value);
      int second = int.Parse(match.Groups[3].Value);
      return Duration.FromSeconds((hour * 60 + minute) * 60 + second);
    }

    internal static DateTimeZone GetTimezone(object input) {
      if (input == null || input is DBNull) return null;
      return Tzdb.GetZoneOrNull((string)input);
    }

    internal static Uri GetUrl(object input) {
      if (input == null || input is DBNull) return null;
      return new Uri((string)input, UriKind.Absolute);
    }
  }
}
