using System.Collections.Generic;
namespace Nixill.GTFS.Parsing {
  public class GTFSWarnings {
    internal List<GTFSUnusableFileWarning> _UnusableFiles;
    public IList<GTFSUnusableFileWarning> UnusableFiles;
    internal List<string> _MissingTables;
    public IList<string> MissingTables;
    internal Dictionary<string, List<string>> _TableWarnings;

    private List<string> EmptyList;

    internal GTFSWarnings() {
      _UnusableFiles = new List<GTFSUnusableFileWarning>();
      UnusableFiles = _UnusableFiles.AsReadOnly();
      _MissingTables = new List<string>();
      MissingTables = _MissingTables.AsReadOnly();
      _TableWarnings = new Dictionary<string, List<string>>();

      EmptyList = new List<string>();
    }

    public IList<string> GetTableWarnings(string table) {
      if (_TableWarnings.ContainsKey(table)) {
        return _TableWarnings[table].AsReadOnly();
      }
      else {
        return EmptyList.AsReadOnly();
      }
    }
  }

  // Base class for all GTFS warnings
  public class GTFSWarning {
    public readonly string Message;

    public GTFSWarning(string msg) {
      Message = msg;
    }
  }

  public class GTFSUnusableFileWarning : GTFSWarning {
    public readonly string Filename;

    public GTFSUnusableFileWarning(string file, string msg) : base(msg) {
      Filename = file;
    }
  }
}