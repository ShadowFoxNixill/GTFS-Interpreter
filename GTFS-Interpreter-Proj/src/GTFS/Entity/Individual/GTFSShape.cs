using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Misc;
using Nixill.GTFS.Parsing;
using Nixill.SQLite;

namespace Nixill.GTFS.Entity {
  public class GTFSShape : GTFSEntity {
    internal override string TableName => "shapes";
    internal override string TableIDCol => "shape_id";

    internal GTFSShape(SqliteConnection conn, string id) : base(conn, id) { }

    public GTFSShapePoint FirstPoint {
      get {
        int seq = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT shape_pt_sequence FROM shapes WHERE shape_id = @p ORDER BY shape_pt_sequence ASC LIMIT 1;", ID)).Value;
        return new GTFSShapePoint(Conn, this, seq);
      }
    }
    public GTFSShapePoint LastPoint {
      get {
        int seq = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT shape_pt_sequence FROM shapes WHERE shape_id = @p ORDER BY shape_pt_sequence DESC LIMIT 1;", ID)).Value;
        return new GTFSShapePoint(Conn, this, seq);
      }
    }

    public IList<GTFSShapePoint> Points {
      get {
        List<GTFSShapePoint> ret = new List<GTFSShapePoint>();

        foreach (object obj in Conn.GetResultList("SELECT shape_pt_sequence FROM shapes WHERE shape_id = @p ORDER BY shape_pt_sequence ASC;", ID)) {
          int seq = GTFSObjectParser.GetInteger(obj).Value;
          ret.Add(new GTFSShapePoint(Conn, this, seq));
        }

        return ret.AsReadOnly();
      }
    }

    public int PointCount => GTFSObjectParser.GetInteger(Conn.GetResult("SELECT count(shape_pt_sequence) FROM shapes WHERE shape_id = @p;", ID)).Value;
    public double? Length => GTFSObjectParser.GetFloat(Conn.GetResult("SELECT max(shape_dist_traveled) - min(shape_dist_traveled) FROM shapes WHERE shape_id = @p", ID));
  }

  public class GTFSShapePoint {
    private SqliteConnection Conn;

    public readonly GTFSShape Shape;
    public readonly int Sequence;

    internal GTFSShapePoint(SqliteConnection conn, GTFSShape shape, int sequence) {
      Conn = conn;
      Shape = shape;
      Sequence = sequence;
    }

    public float Latitude => (float)GTFSObjectParser.GetFloat(Conn.GetResult("SELECT shape_pt_lat FROM shapes WHERE shape_id = @p AND shape_pt_sequence = " + Sequence, Shape.ID)).Value;
    public float Longitude => (float)GTFSObjectParser.GetFloat(Conn.GetResult("SELECT shape_pt_lon FROM shapes WHERE shape_id = @p AND shape_pt_sequence = " + Sequence, Shape.ID)).Value;
    public double? Distance => GTFSObjectParser.GetFloat(Conn.GetResult("SELECT shape_dist_traveled FROM shapes WHERE shape_id = @p AND shape_pt_sequence = " + Sequence, Shape.ID));

    public Coordinates Coordinates => new Coordinates(Latitude, Longitude);

    public GTFSShapePoint Next {
      get {
        int? next = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT shape_pt_sequence FROM shapes WHERE shape_id = @p AND shape_pt_sequence > " + Sequence + " ORDER BY shape_pt_sequence ASC LIMIT 1;", Shape.ID));
        if (next.HasValue) return new GTFSShapePoint(Conn, Shape, next.Value);
        return null;
      }
    }
    public GTFSShapePoint Prev {
      get {
        int? prev = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT shape_pt_sequence FROM shapes WHERE shape_id = @p AND shape_pt_sequence < " + Sequence + " ORDER BY shape_pt_sequence DESC LIMIT 1;", Shape.ID));
        if (prev.HasValue) return new GTFSShapePoint(Conn, Shape, prev.Value);
        return null;
      }
    }
  }
}