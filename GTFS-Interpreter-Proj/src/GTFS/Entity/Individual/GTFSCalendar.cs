using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Misc;
using Nixill.GTFS.Parsing;
using Nixill.SQLite;
using NodaTime;

namespace Nixill.GTFS.Entity {
  public class GTFSCalendar : GTFSEntity {
    internal override string TableName => "calendar_services";
    internal override string TableIDCol => "service_id";

    internal GTFSCalendar(SqliteConnection conn, string id) : base(conn, id) { }

    public bool Sunday => GTFSObjectParser.GetEnum(Conn.GetResult("SELECT sunday FROM calendar WHERE service_id = @p;", ID)) == 1;
    public bool Monday => GTFSObjectParser.GetEnum(Conn.GetResult("SELECT monday FROM calendar WHERE service_id = @p;", ID)) == 1;
    public bool Tuesday => GTFSObjectParser.GetEnum(Conn.GetResult("SELECT tuesday FROM calendar WHERE service_id = @p;", ID)) == 1;
    public bool Wednesday => GTFSObjectParser.GetEnum(Conn.GetResult("SELECT wednesday FROM calendar WHERE service_id = @p;", ID)) == 1;
    public bool Thursday => GTFSObjectParser.GetEnum(Conn.GetResult("SELECT thursday FROM calendar WHERE service_id = @p;", ID)) == 1;
    public bool Friday => GTFSObjectParser.GetEnum(Conn.GetResult("SELECT friday FROM calendar WHERE service_id = @p;", ID)) == 1;
    public bool Saturday => GTFSObjectParser.GetEnum(Conn.GetResult("SELECT saturday FROM calendar WHERE service_id = @p;", ID)) == 1;

    public HashSet<IsoDayOfWeek> WeeklyServices {
      get {
        HashSet<IsoDayOfWeek> ret = new HashSet<IsoDayOfWeek>();
        Dictionary<string, object> dict = Conn.GetRowDict("SELECT monday, tuesday, wednesday, thursday, friday, saturday, sunday FROM calendar WHERE service_id = @p;", ID);

        if (dict != null) {
          if (GTFSObjectParser.GetEnum(dict["sunday"]) == 1) ret.Add(IsoDayOfWeek.Sunday);
          if (GTFSObjectParser.GetEnum(dict["monday"]) == 1) ret.Add(IsoDayOfWeek.Monday);
          if (GTFSObjectParser.GetEnum(dict["tuesday"]) == 1) ret.Add(IsoDayOfWeek.Tuesday);
          if (GTFSObjectParser.GetEnum(dict["wednesday"]) == 1) ret.Add(IsoDayOfWeek.Wednesday);
          if (GTFSObjectParser.GetEnum(dict["thursday"]) == 1) ret.Add(IsoDayOfWeek.Thursday);
          if (GTFSObjectParser.GetEnum(dict["friday"]) == 1) ret.Add(IsoDayOfWeek.Friday);
          if (GTFSObjectParser.GetEnum(dict["saturday"]) == 1) ret.Add(IsoDayOfWeek.Saturday);
        }

        return ret;
      }
    }

    public LocalDate? Start => GTFSObjectParser.GetDate(Conn.GetResult("SELECT start_date FROM calendar WHERE service_id = @p;", ID));
    public LocalDate? End => GTFSObjectParser.GetDate(Conn.GetResult("SELECT end_date FROM calendar WHERE service_id = @p;", ID));

    public List<LocalDate> AddedDates {
      get {
        List<LocalDate> ret = new List<LocalDate>();

        foreach (object obj in Conn.GetResultList("SELECT date FROM calendar_dates WHERE service_id = @p AND exception_type = 1;", ID)) {
          ret.Add(GTFSObjectParser.GetDate(obj).Value);
        }

        return ret;
      }
    }

    public List<LocalDate> RemovedDates {
      get {
        List<LocalDate> ret = new List<LocalDate>();

        foreach (object obj in Conn.GetResultList("SELECT date FROM calendar_dates WHERE service_id = @p AND exception_type = 2;", ID)) {
          ret.Add(GTFSObjectParser.GetDate(obj).Value);
        }

        return ret;
      }
    }

    public List<LocalDate> AllActiveDates {
      get {
        List<LocalDate> ret = AddedDates;

        LocalDate? nStart = Start;

        if (nStart != null) {
          HashSet<IsoDayOfWeek> days = WeeklyServices;
          LocalDate start = nStart.Value;
          LocalDate end = End.Value;
          List<LocalDate> removed = RemovedDates;

          for (LocalDate firstOfWeekday = start; firstOfWeekday <= end && firstOfWeekday < start.PlusDays(7); firstOfWeekday = firstOfWeekday.PlusDays(1)) {
            if (days.Contains(firstOfWeekday.DayOfWeek)) {
              for (LocalDate week = firstOfWeekday; week <= end; week = week.PlusDays(7)) {
                if (!removed.Contains(week)) {
                  if (!ret.Contains(week)) {
                    ret.Add(week);
                  }
                }
                else {
                  removed.Remove(week);
                }
              }
            }
          }
        }

        return ret;
      }
    }

    public bool IsActive(LocalDate date) {
      string day = date.DayOfWeek.ToString().ToLower();
      string datestr = GTFSStatics.GTFSDatePattern.Format(date);

      bool validWeekly = GTFSObjectParser.GetEnum(Conn.GetResult("SELECT " + day + " FROM calendar WHERE service_id = @p AND start_date <= '" + datestr + "' AND end_date >= '" + datestr + "';", ID)) == 1;
      int? exception = GTFSObjectParser.GetEnum(Conn.GetResult("SELECT exception_type FROM calendar_dates WHERE service_id = @p AND date = '" + datestr + "';", ID));

      return (validWeekly && exception != 2) || (exception == 1);
    }
  }
}