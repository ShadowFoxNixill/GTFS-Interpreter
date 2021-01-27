using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Misc;
using Nixill.GTFS.Parsing;
using Nixill.SQLite;
using Nixill.Utils;

namespace Nixill.GTFS.Entity {
  public class GTFSShape : GTFSEntity {
    internal override string TableName => "shape_ids";
    internal override string TableIDCol => "shape_id";

    internal GTFSShape(SqliteConnection conn, string id) : base(conn, id) { }

    public GTFSShapePoint FirstPoint => new GTFSShapePoint(Conn, this,
      GTFSObjectParser.GetInteger(Conn.GetResult("SELECT min(shape_pt_sequence) FROM shapes WHERE shape_id = @p0;", ID)).Value);
    public GTFSShapePoint LastPoint => new GTFSShapePoint(Conn, this,
      GTFSObjectParser.GetInteger(Conn.GetResult("SELECT max(shape_pt_sequence) FROM shapes WHERE shape_id = @p0;", ID)).Value);

    public List<GTFSShapePoint> Points => Conn.GetResultList("SELECT shape_pt_sequence FROM shapes WHERE shape_id = @p0 ORDER BY shape_pt_sequence ASC;", ID)
      .Transform(obj => new GTFSShapePoint(Conn, this, GTFSObjectParser.GetInteger(obj).Value));

    public int PointCount => GTFSObjectParser.GetInteger(Conn.GetResult("SELECT count(shape_pt_sequence) FROM shapes WHERE shape_id = @p0;", ID)).Value;
    public double? Length => GTFSObjectParser.GetFloat(Conn.GetResult("SELECT max(shape_dist_traveled) - min(shape_dist_traveled) FROM shapes WHERE shape_id = @p0;", ID));
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

    public float Latitude => (float)GTFSObjectParser.GetFloat(Conn.GetResult("SELECT shape_pt_lat FROM shapes WHERE shape_id = @p0 AND shape_pt_sequence = @p1;", Shape.ID, Sequence)).Value;
    public float Longitude => (float)GTFSObjectParser.GetFloat(Conn.GetResult("SELECT shape_pt_lon FROM shapes WHERE shape_id = @p0 AND shape_pt_sequence = @p1;", Shape.ID, Sequence)).Value;
    public double? Distance => GTFSObjectParser.GetFloat(Conn.GetResult("SELECT shape_dist_traveled FROM shapes WHERE shape_id = @p0 AND shape_pt_sequence = @p1;", Shape.ID, Sequence));

    public Coordinates Coordinates => new Coordinates(Latitude, Longitude);

    public GTFSShapePoint Next {
      get {
        int? next = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT min(shape_pt_sequence) FROM shapes WHERE shape_id = @p0 AND shape_pt_sequence > @p1;", Shape.ID, Sequence));
        if (next.HasValue) return new GTFSShapePoint(Conn, Shape, next.Value);
        return null;
      }
    }
    public GTFSShapePoint Prev {
      get {
        int? prev = GTFSObjectParser.GetInteger(Conn.GetResult("SELECT max(shape_pt_sequence) FROM shapes WHERE shape_id = @p0 AND shape_pt_sequence < @p1;", Shape.ID, Sequence));
        if (prev.HasValue) return new GTFSShapePoint(Conn, Shape, prev.Value);
        return null;
      }
    }
  }
}