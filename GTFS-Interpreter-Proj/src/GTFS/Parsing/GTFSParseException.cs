namespace Nixill.GTFS.Parsing {
  [System.Serializable]
  public class GTFSParseException : System.Exception {
    public GTFSParseException() { }
    public GTFSParseException(string message) : base(message) { }
    public GTFSParseException(string message, System.Exception inner) : base(message, inner) { }
    protected GTFSParseException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
  }
}