namespace Nixill.GTFS.Enumerations {
  public enum GTFSRouteType {
    LightRail = 0,
    Subway = 1,
    Rail = 2,
    Bus = 3,
    Ferry = 4,
    CableTram = 5,
    Aerialway = 6,
    Funicular = 7,
    Trolleybus = 11,
    Monorail = 12
  }

  public enum GTFSPickupDropoff {
    Regular = 0,
    None = 1,
    PhoneAgency = 2,
    AskDriver = 3,
    Events = 4
  }

  public enum GTFSLocationType {
    Platform = 0,
    Station = 1,
    Entrance = 2,
    GenericNode = 3,
    BoardingArea = 4,
    UnmannedPOS = 5,
    MannedPOS = 6
  }

  public enum GTFSTristate {
    None = 0,
    True = 1,
    False = 2
  }
}