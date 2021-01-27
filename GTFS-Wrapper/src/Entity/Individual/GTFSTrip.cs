using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Enumerations;
using Nixill.GTFS.Parsing;
using Nixill.SQLite;
using Nixill.Utils;
using NodaTime;

namespace Nixill.GTFS.Entity {
  public class GTFSTrip : GTFSEntity {
    internal override string TableName => "trips";
    internal override string TableIDCol => "trip_id";

    internal GTFSTrip(SqliteConnection conn, string id) : base(conn, id) { }

    public string RouteID => GTFSObjectParser.GetID(Conn.GetResult("SELECT route_id FROM trips WHERE trip_id = @p0;", ID));
    public string ServiceID => GTFSObjectParser.GetID(Conn.GetResult("SELECT service_id FROM trips WHERE trip_id = @p0;", ID));
    public string Headsign => GTFSObjectParser.GetText(Conn.GetResult("SELECT trip_headsign FROM trips WHERE trip_id = @p0;", ID));
    public string ShortName => GTFSObjectParser.GetText(Conn.GetResult("SELECT trip_short_name FROM trips WHERE trip_id = @p0;", ID));
    public int? DirectionID => GTFSObjectParser.GetEnum(Conn.GetResult("SELECT direction_id FROM trips WHERE trip_id = @p0;", ID));
    public string BlockID => GTFSObjectParser.GetID(Conn.GetResult("SELECT block_id FROM trips WHERE trip_id = @p0;", ID));
    public string ShapeID => GTFSObjectParser.GetID(Conn.GetResult("SELECT shape_id FROM trips WHERE trip_id = @p0;", ID));
    public GTFSTristate WheelchairAccessibility => (GTFSTristate)GTFSObjectParser.GetEnum(Conn.GetResult("SELECT wheelchair_accessible FROM trips WHERE trip_id = @p0;", ID));
    public GTFSTristate BikesAllowed => (GTFSTristate)GTFSObjectParser.GetEnum(Conn.GetResult("SELECT bikes_allowed FROM trips WHERE trip_id = @p0;", ID));

    public GTFSRoute Route => new GTFSRoute(Conn, RouteID);
    public GTFSCalendar Service => new GTFSCalendar(Conn, ServiceID);
    public GTFSBlock Block => new GTFSBlock(Conn, BlockID);
    public GTFSShape Shape => new GTFSShape(Conn, ShapeID);

    public List<GTFSStopTime> Stops => Conn.GetResultList("SELECT stop_sequence FROM stop_times WHERE trip_id = @p0 ORDER BY stop_sequence ASC;")
      .Transform((obj) => new GTFSStopTime(Conn, this, GTFSObjectParser.GetInteger(obj).Value));
    public Duration StartTime => GTFSObjectParser.GetTime(Conn.GetResult("SELECT min(arrival_time) FROM stop_times WHERE trip_id = @p0", ID)).Value;
    public Duration EndTime => GTFSObjectParser.GetTime(Conn.GetResult("SELECT max(departure_time) FROM stop_times WHERE trip_id = @p0", ID)).Value;
    public Duration Length => EndTime - StartTime;

    public List<GTFSStopTime> TimesAtStop(GTFSStop stop) => Conn.GetResultList("SELECT stop_sequence FROM stop_times WHERE trip_id = @p0 AND stop_id = @p1;", ID)
      .Transform(obj => new GTFSStopTime(Conn, this, GTFSObjectParser.GetInteger(obj).Value));
  }

  public class GTFSBlock : GTFSEntity {
    internal override string TableName => "trips";
    internal override string TableIDCol => "trip_id";

    internal GTFSBlock(SqliteConnection conn, string id) : base(conn, id) { }

    public List<GTFSTrip> Trips => Conn.GetResultList("SELECT trip_id FROM trips WHERE block_id = @p0;", ID)
      .Transform((obj) => new GTFSTrip(Conn, GTFSObjectParser.GetID(obj)));
  }

  public class GTFSStopTime {
    private SqliteConnection Conn;

    public readonly GTFSTrip Trip;
    public readonly int StopSequence;

    private object Select(string col) => Conn.GetResult($"SELECT {col} FROM stop_times WHERE trip_id = @p0 AND stop_sequence = @p1;", Trip.ID, StopSequence);

    internal GTFSStopTime(SqliteConnection conn, GTFSTrip trip, int sequence) {
      Conn = conn;
      Trip = trip;
      StopSequence = sequence;
    }

    public Duration? ArrivalTime => GTFSObjectParser.GetTime(Select("arrival_time"));
    public Duration? DepartureTime => GTFSObjectParser.GetTime(Select("departure_time"));
    public string StopID => GTFSObjectParser.GetID(Select("stop_id"));
    public string StopHeadsign => GTFSObjectParser.GetID(Select("stop_headsign"));
    public GTFSPickupDropoff PickupType => (GTFSPickupDropoff)GTFSObjectParser.GetEnum(Select("pickup_type"));
    public GTFSPickupDropoff DropoffType => (GTFSPickupDropoff)GTFSObjectParser.GetEnum(Select("drop_off_type"));
    public GTFSPickupDropoff ContinuousPickup => (GTFSPickupDropoff)GTFSObjectParser.GetEnum(Select("continuous_pickup"));
    public GTFSPickupDropoff ContinuousDropoff => (GTFSPickupDropoff)GTFSObjectParser.GetEnum(Select("continuous_drop_off"));
    public double? ShapeDistTraveled => GTFSObjectParser.GetFloat(Select("shape_dist_traveled"));
    public bool Timepoint => GTFSObjectParser.GetEnum(Select("timepoint")) == 1;

    public GTFSStop Stop => new GTFSStop(Conn, StopID);
    public int StopOrder => GTFSObjectParser.GetInteger(Conn.GetResult("SELECT count(stop_sequence) FROM stop_times WHERE trip_id = @p0 AND stop_sequence < @p1;", Trip.ID, StopSequence)).Value;

    public GTFSStopTime Prev {
      get {
        int? prev = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT max(stop_sequence) FROM stop_times WHERE trip_id = @p0 AND stop_sequence < @p1;", Trip.ID, StopSequence));
        if (prev.HasValue) return new GTFSStopTime(Conn, Trip, prev.Value);
        return null;
      }
    }

    public GTFSStopTime Next {
      get {
        int? next = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT min(stop_sequence) FROM stop_times WHERE trip_id = @p0 AND stop_sequence > @p1;", Trip.ID, StopSequence));
        if (next.HasValue) return new GTFSStopTime(Conn, Trip, next.Value);
        return null;
      }
    }

    public GTFSStopTime PrevTimed {
      get {
        int? prev = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT max(stop_sequence) FROM stop_times WHERE trip_id = @p0 AND stop_sequence < @p1 AND arrival_time IS NOT NULL;", Trip.ID, StopSequence));
        if (prev.HasValue) return new GTFSStopTime(Conn, Trip, prev.Value);
        return null;
      }
    }

    public GTFSStopTime NextTimed {
      get {
        int? next = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT min(stop_sequence) FROM stop_times WHERE trip_id = @p0 AND stop_sequence > @p1 AND arrival_time IS NOT NULL;", Trip.ID, StopSequence));
        if (next.HasValue) return new GTFSStopTime(Conn, Trip, next.Value);
        return null;
      }
    }

    public GTFSStopTime PrevTimepoint {
      get {
        int? prev = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT max(stop_sequence) FROM stop_times WHERE trip_id = @p0 AND stop_sequence < @p1 AND timepoint == 1;", Trip.ID, StopSequence));
        if (prev.HasValue) return new GTFSStopTime(Conn, Trip, prev.Value);
        return null;
      }
    }

    public GTFSStopTime NextTimepoint {
      get {
        int? next = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT min(stop_sequence) FROM stop_times WHERE trip_id = @p0 AND stop_sequence > @p1 AND timepoint == 1;", Trip.ID, StopSequence));
        if (next.HasValue) return new GTFSStopTime(Conn, Trip, next.Value);
        return null;
      }
    }

    public Duration ApproximateArrivalTime() => ApproximateTimes().Item1;
    public Duration ApproximateDepartureTime() => ApproximateTimes().Item2;

    public Tuple<Duration, Duration> ApproximateTimes() {
      // We're going to use custom database calls here so that we don't need to call the database more than necessary.

      // If we just already have times, let's return them.
      Dictionary<string, object> ourTimes = Conn.GetRowDict("SELECT arrival_time, departure_time FROM stop_times WHERE trip_id = @p0 AND stop_sequence = @p1;", Trip.ID, StopSequence);
      Duration? arrivalTime = GTFSObjectParser.GetTime(ourTimes["arrival_time"]);
      if (arrivalTime.HasValue) return new Tuple<Duration, Duration>(arrivalTime.Value, GTFSObjectParser.GetTime(ourTimes["departure_time"]).Value);

      // Otherwise let's grab our properties as well as those of the previous and next stops.
      double? ourDistance = ShapeDistTraveled;

      GTFSStopTime prev = PrevTimed;
      Dictionary<string, object> prevProps = Conn.GetRowDict("SELECT arrival_time, departure_time, shape_dist_traveled FROM stop_times WHERE trip_id = @p0 AND stop_sequence = @p1", Trip.ID, prev.StopSequence);
      GTFSStopTime next = NextTimed;
      Dictionary<string, object> nextProps = Conn.GetRowDict("SELECT arrival_time, departure_time, shape_dist_traveled FROM stop_times WHERE trip_id = @p0 AND stop_sequence = @p1", Trip.ID, next.StopSequence);

      // If we have the distance of all three stops, we can use that for the lerp
      // Otherwise, we need the count of stops instead
      double? prevDistance = GTFSObjectParser.GetFloat(prevProps["shape_dist_traveled"]);
      double? nextDistance = GTFSObjectParser.GetFloat(nextProps["shape_dist_traveled"]);

      // Let's get the arrival and departure times, as well as their average and their difference
      Duration prevArrive = GTFSObjectParser.GetTime(prevProps["arrival_time"]).Value;
      Duration prevDepart = GTFSObjectParser.GetTime(prevProps["departure_time"]).Value;
      Duration prevAverage = (prevDepart + prevArrive) / 2;
      Duration prevDifference = prevDepart - prevArrive;

      Duration nextArrive = GTFSObjectParser.GetTime(nextProps["arrival_time"]).Value;
      Duration nextDepart = GTFSObjectParser.GetTime(nextProps["departure_time"]).Value;
      Duration nextAverage = (nextDepart + nextArrive) / 2;
      Duration nextDifference = nextDepart - nextArrive;

      if (ourDistance.HasValue && prevDistance.HasValue && nextDistance.HasValue) {
        Duration lerpAverage = Duration.FromSeconds(TempUtils.LerpDouble(prevDistance.Value, prevAverage.TotalSeconds, nextDistance.Value, nextAverage.TotalSeconds, ourDistance.Value));
        Duration lerpDifference = Duration.FromSeconds(TempUtils.LerpDouble(prevDistance.Value, prevDifference.TotalSeconds, nextDistance.Value, nextDifference.TotalSeconds, ourDistance.Value));
        Duration lerpArrive = lerpAverage - (lerpDifference / 2);
        Duration lerpDepart = lerpAverage + (lerpDifference / 2);

        return new Tuple<Duration, Duration>(lerpArrive, lerpDepart);
      }
      // Otherwise, we'll just use stop counts instead.
      else {
        double prevCount = prev.StopOrder;
        double ourCount = StopOrder;
        double nextCount = next.StopOrder;

        Duration lerpAverage = Duration.FromSeconds(TempUtils.LerpDouble(prevCount, prevAverage.TotalSeconds, nextCount, nextAverage.TotalSeconds, ourCount));
        Duration lerpDifference = Duration.FromSeconds(TempUtils.LerpDouble(prevCount, prevDifference.TotalSeconds, nextCount, nextDifference.TotalSeconds, ourCount));
        Duration lerpArrive = lerpAverage - (lerpDifference / 2);
        Duration lerpDepart = lerpAverage + (lerpDifference / 2);

        return new Tuple<Duration, Duration>(lerpArrive, lerpDepart);
      }
    }
  }
}