using System;
using System.Collections.Generic;
using System.Text;

namespace NavigatorLibrary.Models
{
    public class Waypoint
    {
        public Guid _id { get; set; }
        public string _name { get; set; }
        public LocationType _type { get; set; }
        public CategoryType _category { get; set; }
        public List<Guid> _neighbors { get; set; }

        // We should save lon/lat inforamtion for calculating distance of
        // WaypointEdge later while parsing <edge> in XML
        public double _lon { get; set; }
        public double _lat { get; set; }
    }

    public class PortalWaypoints
    {
        public Guid _portalWaypoint1 { get; set; }
        public Guid _portalWaypoint2 { get; set; }
    }

    public class GroupWaypoint
    {
        public List<RegionWaypointPoint> _regionsAndWaypoints;
        public waypointDecisionOrIgnore _decisionOrIgnore;
    }
}
