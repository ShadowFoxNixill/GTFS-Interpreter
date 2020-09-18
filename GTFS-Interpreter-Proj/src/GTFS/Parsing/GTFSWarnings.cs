using System.Collections.Generic;
namespace Nixill.GTFS.Parsing {
  // GTFS warning class
  public class GTFSWarning {
    public string Table { get; internal set; }
    public string Record { get; internal set; }
    public string Field { get; internal set; }
    public string Message { get; internal set; }

    internal GTFSWarning(string msg) {
      Message = msg;
    }
  }
}