using System;
using System.Collections.Generic;
using System.Text;

namespace NavigatorLibrary.Models
{
    public class Region
    {
        public Guid _id { get; set; }
        public IPSType _IPSType { get; set; }
        public string _name { get; set; }
        public int _floor { get; set; }
        public List<Guid> _neighbors { get; set; }
        public Dictionary<CategoryType, List<Waypoint>> _waypointsByCategory
        { get; set; }
    }

    public class RegionWaypointPoint
    {
        public RegionWaypointPoint(Guid regionID, Guid waypointID)
        {
            this._regionID = regionID;
            this._waypointID = waypointID;
        }

        public RegionWaypointPoint() { }

        public Guid _regionID { get; set; }
        public Guid _waypointID { get; set; }
    }
}
