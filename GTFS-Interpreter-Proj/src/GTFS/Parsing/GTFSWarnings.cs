using System.Collections.Generic;
namespace Nixill.GTFS.Parsing {
  public class GTFSWarnings {
    internal List<GTFSUnusableFileWarning> _UnusableFiles;
    public IList<GTFSUnusableFileWarning> UnusableFiles;
    internal List<string> _MissingTables;
    public IList<string> MissingTables;

    internal GTFSWarnings() {
      _UnusableFiles = new List<GTFSUnusableFileWarning>();
      UnusableFiles = _UnusableFiles.AsReadOnly();
      _MissingTables = new List<string>();
      MissingTables = _MissingTables.AsReadOnly();
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