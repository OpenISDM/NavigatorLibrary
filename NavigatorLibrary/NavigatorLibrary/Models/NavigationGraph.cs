
using Dijkstra.NET.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace NavigatorLibrary.Models
{
    public class NavigationGraph
    {
        private double EARTH_RADIUS;

        private string _country;
        private string _cityCounty;
        private string _industryService;
        private string _ownerOrganization;
        private string _buildingName;
        private double _version;
        //Guid is region's Guid
        private Dictionary<Guid, Region> _regions;
        private XmlDocument document;

        public NavigationGraph(XmlDocument document)
        {
            this.document = document;
        }

        private Dictionary<Tuple<Guid, Guid>, List<RegionEdge>> _edges { get; set; }
        private Dictionary<Guid, Navigraph> _navigraphs { get; set; }

        public class Navigraph
        {
            public Guid _regionID { get; set; }
            //Guid is waypoint's Guid
            public Dictionary<Guid, Waypoint> _waypoints { get; set; }
            public Dictionary<Tuple<Guid, Guid>, WaypointEdge> _edges { get; set; }
            //Guid is waypoint's Guid
            public Dictionary<Guid, List<Guid>> _beacons { get; set; }
            public Dictionary<Guid, int> _beaconRSSIThreshold { get; set; }
        }       

        private static double Rad(double d)
        {
            return (double)d * Math.PI / 180d;
        }

        private RegionEdge GetRegionEdgeMostNearSourceWaypoint
            (Guid sourceRegionID,
             Guid sourceWaypointID,
             Guid sinkRegionID,
             ConnectionType[] avoidConnectionTypes)
        {
            RegionEdge regionEdgeItem = new RegionEdge();

            Waypoint sourceWaypoint =
                _navigraphs[sourceRegionID]._waypoints[sourceWaypointID];

            // compare the normal case (R1, R2)
            Tuple<Guid, Guid> edgeKeyFromNode1 =
                new Tuple<Guid, Guid>(sourceRegionID, sinkRegionID);

            int distance = Int32.MaxValue;
            int indexEdge = -1;
            if (_edges.ContainsKey(edgeKeyFromNode1))
            {
                for (int i = 0; i < _edges[edgeKeyFromNode1].Count(); i++)
                {
                    RegionEdge edgeItem = _edges[edgeKeyFromNode1][i];

                    if (!avoidConnectionTypes.Contains(edgeItem._connectionType))
                    {
                        if (DirectionalConnection.BiDirection ==
                            edgeItem._biDirection ||
                            (DirectionalConnection.OneWay ==
                            edgeItem._biDirection &&
                            1 == edgeItem._source))
                        {
                            Waypoint sinkWaypoint =
                                _navigraphs[sourceRegionID].
                                _waypoints[edgeItem._waypoint1];
                            double distanceFromSource =
                                GetDistance(sourceWaypoint._lon,
                                            sourceWaypoint._lat,
                                            sinkWaypoint._lon,
                                            sinkWaypoint._lat);
                            int edgeDistance =
                                System.Convert.ToInt32(distanceFromSource);

                            if (edgeDistance < distance)
                            {
                                distance = edgeDistance;
                                indexEdge = i;
                            }
                        }
                    }
                }
            }
            if (-1 != indexEdge)
            {
                regionEdgeItem = _edges[edgeKeyFromNode1][indexEdge];
                return regionEdgeItem;
            }

            // compare the reverse case (R2, R1) because normal case (R1, R2) 
            // cannot find regionEdge
            Tuple<Guid, Guid> edgeKeyFromNode2 =
                new Tuple<Guid, Guid>(sinkRegionID, sourceRegionID);

            if (_edges.ContainsKey(edgeKeyFromNode2))
            {

                for (int i = 0; i < _edges[edgeKeyFromNode2].Count(); i++)
                {

                    RegionEdge edgeItem = _edges[edgeKeyFromNode2][i];

                    if (!avoidConnectionTypes.Contains
                        (edgeItem._connectionType))
                    {

                        if (DirectionalConnection.BiDirection ==
                            edgeItem._biDirection ||
                            (DirectionalConnection.OneWay ==
                            edgeItem._biDirection &&
                            2 == edgeItem._source))
                        {



                            Waypoint sinkWaypoint =
                                _navigraphs[sinkRegionID].
                                    _waypoints[edgeItem._waypoint1];

                            double distanceFromSource =
                                GetDistance(sourceWaypoint._lon,
                                            sourceWaypoint._lat,
                                            sinkWaypoint._lon,
                                            sinkWaypoint._lat);

                            int edgeDistance =
                                System.Convert.ToInt32(distanceFromSource);

                            if (edgeDistance < distance)
                            {
                                distance = edgeDistance;
                                indexEdge = i;
                            }
                        }
                    }
                }
            }
            if (-1 != indexEdge)
            {

                // need to reverse the resulted regionEdge from (R1/W1, R2/W2) 
                //pair to (R2/W2, R1/W1) pair before returning to caller
                regionEdgeItem._region1 =
                    _edges[edgeKeyFromNode2][indexEdge]._region2;
                regionEdgeItem._region2 =
                    _edges[edgeKeyFromNode2][indexEdge]._region1;
                regionEdgeItem._waypoint1 =
                    _edges[edgeKeyFromNode2][indexEdge]._waypoint2;
                regionEdgeItem._waypoint2 =
                    _edges[edgeKeyFromNode2][indexEdge]._waypoint1;
                regionEdgeItem._biDirection =
                    _edges[edgeKeyFromNode2][indexEdge]._biDirection;
                if (2 == _edges[edgeKeyFromNode2][indexEdge]._source)
                    regionEdgeItem._source = 1;
                regionEdgeItem._distance =
                    _edges[edgeKeyFromNode2][indexEdge]._distance;
                if (System.Convert.ToInt32
                    (_edges[edgeKeyFromNode2][indexEdge]._direction) + 4 < 8)
                {
                    regionEdgeItem._direction = (CardinalDirection)
                        (4 + _edges[edgeKeyFromNode2][indexEdge]._direction);
                }
                else
                {
                    regionEdgeItem._direction =
                        (CardinalDirection)
                        (4 + _edges[edgeKeyFromNode2][indexEdge]._direction - 8);
                }
                regionEdgeItem._connectionType =
                    _edges[edgeKeyFromNode2][indexEdge]._connectionType;

                return regionEdgeItem;

            }
            return regionEdgeItem;
        }

        private WaypointEdge GetWaypointEdgeInRegion
                                        (Guid regionID,
                                         Guid sourceWaypoindID,
                                         Guid sinkWaypointID,
                                         ConnectionType[] avoidConnectionTypes)
        {
            WaypointEdge waypointEdge = new WaypointEdge();

            Tuple<Guid, Guid> edgeKeyFromNode1 =
                new Tuple<Guid, Guid>(sourceWaypoindID, sinkWaypointID);

            Tuple<Guid, Guid> edgeKeyFromNode2 =
                new Tuple<Guid, Guid>(sinkWaypointID, sourceWaypoindID);

            if (_navigraphs[regionID]._edges.ContainsKey(edgeKeyFromNode1))
            {
                // XML file contains (W1, W2) and the query input is (W1, W2) 
                // as well.
                waypointEdge = _navigraphs[regionID]._edges[edgeKeyFromNode1];
            }
            else if (_navigraphs[regionID]._edges.ContainsKey(edgeKeyFromNode2))
            {
                // XML file contains (W1, W2) but the query string is (W2, W1).
                waypointEdge = _navigraphs[regionID]._edges[edgeKeyFromNode2];

                if (System.Convert.ToInt32(waypointEdge._direction) + 4 < 8)
                {
                    waypointEdge._direction = (CardinalDirection)
                        (4 + waypointEdge._direction);
                }
                else
                {
                    waypointEdge._direction = (CardinalDirection)
                        (4 + waypointEdge._direction - 8);
                }

            }
            return waypointEdge;
        }

        public int GetDistanceOfLongHallway(RegionWaypointPoint currentGuid,
                                            int nextStep,
                                            List<RegionWaypointPoint> allRoute,
                                            ConnectionType[] avoidConnectionType)
        {
            int distance = 0;
            if (nextStep <= 0)
            {
                nextStep = 1;
            }
            for (int i = nextStep - 1; i < allRoute.Count(); i++)
            {

                if (allRoute[i]._regionID != allRoute[i + 1]._regionID)
                {
                    if (_regions[allRoute[i]._regionID]._floor ==
                            _regions[allRoute[i + 1]._regionID]._floor)
                    {
                        RegionEdge regionEdge =
                            GetRegionEdgeMostNearSourceWaypoint
                                (allRoute[i].
                                 _regionID, allRoute[i].
                                 _waypointID, allRoute[i + 1].
                                 _regionID, avoidConnectionType);
                        distance = System.Convert.ToInt32(regionEdge._distance);
                    }
                    else
                    {
                        break;
                    }

                }
                else
                {
                    WaypointEdge waypointEdge =
                        GetWaypointEdgeInRegion(allRoute[i]._regionID,
                                                allRoute[i]._waypointID,
                                                allRoute[i + 1]._waypointID,
                                                avoidConnectionType);

                    distance =
                        distance + System.Convert.ToInt32(waypointEdge._distance);

                    if (i + 2 >= allRoute.Count())
                    {
                        break;
                    }
                    else
                    {
                        WaypointEdge currentWaypointEdge =
                            GetWaypointEdgeInRegion(allRoute[i]._regionID,
                                                    allRoute[i]._waypointID,
                                                    allRoute[i + 1]._waypointID,
                                                    avoidConnectionType);
                        WaypointEdge nextWaypointEdge =
                            GetWaypointEdgeInRegion(allRoute[i + 1]._regionID,
                                                    allRoute[i + 1]._waypointID,
                                                    allRoute[i + 2]._waypointID,
                                                    avoidConnectionType);
                        if (currentWaypointEdge._direction
                                != nextWaypointEdge._direction)
                        {
                            break;
                        }
                    }
                }

            }
            return distance;
        }
        private double GetDistance(double lon1,
                                   double lat1,
                                   double lon2,
                                   double lat2)
        {
            double radLat1 = Rad(lat1);
            double radLng1 = Rad(lon1);
            double radLat2 = Rad(lat2);
            double radLng2 = Rad(lon2);
            double a = radLat1 - radLat2;
            double b = radLng1 - radLng2;
            double result =
                2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
                Math.Cos(radLat1) *
                Math.Cos(radLat2) *
                Math.Pow(Math.Sin(b / 2), 2))) *
                EARTH_RADIUS;
            return result;
        }
        public string GetIndustryServer()
        {
            return _industryService;
        }

        public string GetBuildingName()
        {
            return _buildingName;
        }

        public double GetVersion()
        {
            return _version;
        }

        public Dictionary<Guid, Region> GetRegions()
        {
            return _regions;
        }

        public List<Guid> GetAllBeaconIDInOneWaypointOfRegion(Guid regionID,
                                                              Guid waypointID)
        {
            List<Guid> beaconIDs = new List<Guid>();

            beaconIDs = _navigraphs[regionID]._beacons[waypointID];

            return beaconIDs;
        }

        public List<Guid> GetAllWaypointIDInOneRegion(Guid regionID)
        {
            List<Guid> waypointIDs = new List<Guid>();

            foreach (KeyValuePair<Guid, Waypoint> waypointItem
                     in _navigraphs[regionID]._waypoints)
            {
                waypointIDs.Add(waypointItem.Key);
            }
            return waypointIDs;
        }

        public List<Guid> GetAllRegionIDs()
        {
            List<Guid> regionIDs = new List<Guid>();
            foreach (KeyValuePair<Guid, Region> regionItems in _regions)
            {
                regionIDs.Add(regionItems.Key);
            }

            return regionIDs;
        }

        public LocationType GetWaypointTypeInRegion(Guid regionID,
                                                    Guid waypointID)
        {
            return _navigraphs[regionID]._waypoints[waypointID]._type;
        }

        public string GetWaypointNameInRegion(Guid regionID, Guid waypointID)
        {
            return _navigraphs[regionID]._waypoints[waypointID]._name;
        }

        public IPSType GetRegionIPSType(Guid regionID)
        {
            return _regions[regionID]._IPSType;
        }

        public int GetBeaconRSSIThreshold(Guid regionGuid, Guid beaconGuid)
        {
            return _navigraphs[regionGuid]._beaconRSSIThreshold[beaconGuid];
        }

        public PortalWaypoints GetPortalWaypoints
                                         (Guid sourceRegionID,
                                          Guid sourceWaypointID,
                                          Guid sinkRegionID,
                                          ConnectionType[] avoidConnectionTypes)
        {
            // for regionEdge, we need to handle following two cases:
            // case 1. R1/W1 -> R2/W2
            // case 2. R2/W2 -> R1/W1
            // When we parse the XML file, we may store either one of these two 
            // cases into C# structure with its bi-direction, connectiontype, 
            // and source properties.
            // While this edge is queried, we should serve both (R1, R2) and 
            // (R2, R1) cases with corresponding portal waypoints.

            PortalWaypoints portalWaypoints = new PortalWaypoints();

            RegionEdge regionEdge =
                GetRegionEdgeMostNearSourceWaypoint(sourceRegionID,
                                                    sourceWaypointID,
                                                    sinkRegionID,
                                                    avoidConnectionTypes);

            portalWaypoints._portalWaypoint1 = regionEdge._waypoint1;
            portalWaypoints._portalWaypoint2 = regionEdge._waypoint2;

            return portalWaypoints;
        }

        public RegionWaypointPoint GiveNeighborWaypointInNeighborRegion
                                                        (Guid sourceRegionID,
                                                         Guid sourceWaypointID,
                                                         Guid sinkRegionID)
        {
            RegionWaypointPoint regionWaypointPoint = new RegionWaypointPoint();
            ConnectionType[] emptyAvoid = new ConnectionType[0];
            RegionEdge regionEdge =
                GetRegionEdgeMostNearSourceWaypoint(sourceRegionID,
                                                    sourceWaypointID,
                                                    sinkRegionID,
                                                    emptyAvoid);
            regionWaypointPoint._waypointID = regionEdge._waypoint2;
            regionWaypointPoint._regionID = regionEdge._region2;
            return regionWaypointPoint;
        }

        public List<Guid> GetNeighbor(Guid regionID, Guid waypointID)
        {
            return _navigraphs[regionID]._waypoints[waypointID]._neighbors;
        }

        public double StraightDistanceBetweenWaypoints(Guid region,
                                                       Guid waypointID1,
                                                       Guid waypointID2)
        {

            double lat1 = _navigraphs[region]._waypoints[waypointID1]._lat;
            double lon1 = _navigraphs[region]._waypoints[waypointID1]._lon;
            double lat2 = _navigraphs[region]._waypoints[waypointID2]._lat;
            double lon2 = _navigraphs[region]._waypoints[waypointID2]._lon;
            double distance = GetDistance(lon1, lat1, lon2, lat2);
            return distance;
        }

        public InstructionInformation GetInstructionInformation(
            int currentNavigationStep,
            Guid currentRegionID,
            Guid currentWaypointID,
            Guid previousRegionID,
            Guid previousWaypointID,
            Guid nextRegionID,
            Guid nextWaypointID,
            ConnectionType[] avoidConnectionTypes)
        {
            InstructionInformation information = new InstructionInformation();

            information._floor = _regions[nextRegionID]._floor;
            information._regionName = _regions[nextRegionID]._name;

            if (!currentRegionID.Equals(nextRegionID))
            {
                // currentWaypoint and nextWaypoint are in different regions

                if (!_regions[currentRegionID].
                        _floor.Equals(_regions[nextRegionID]._floor))
                {
                    // currentWaypoint and nextWaypoint are in different regions
                    // with different floors 
                    if (_regions[nextRegionID]._floor >
                            _regions[currentRegionID]._floor)
                    {
                        information._turnDirection = TurnDirection.Up;
                    }
                    else
                    {
                        information._turnDirection = TurnDirection.Down;
                    }

                    RegionEdge currentEdge =
                        GetRegionEdgeMostNearSourceWaypoint(currentRegionID,
                                                            currentWaypointID,
                                                            nextRegionID,
                                                          avoidConnectionTypes);

                    information._connectionType = currentEdge._connectionType;
                    information._distance = System.Convert
                                            .ToInt32(currentEdge._distance);
                }
                else
                {
                    // currentWaypoint and nextWaypoint are across regions
                    // but on the same floor 

                    // When step==0, it means that the turn direction is first 
                    // direction.
                    if (0 == currentNavigationStep)
                    {
                        // currentWaypoint is the first waypoing from the 
                        // beginning need to refine the turndirection in this 
                        // case
                        information._turnDirection =
                            TurnDirection.FirstDirection;

                        RegionEdge currentEdge =
                            GetRegionEdgeMostNearSourceWaypoint
                                                         (currentRegionID,
                                                          currentWaypointID,
                                                          nextRegionID,
                                                          avoidConnectionTypes);
                        information._relatedDirectionOfFirstDirection =
                            currentEdge._direction;

                        information._connectionType =
                            currentEdge._connectionType;

                        information._distance = System.Convert
                                                .ToInt32(currentEdge._distance);
                    }
                    else
                    {
                        if (!previousRegionID.Equals(currentRegionID))
                        {
                            // previouWaypoint and currentWaypoint are acrss 
                            // regions
                            if (!_regions[previousRegionID]._floor.Equals(
                                _regions[currentRegionID]._floor))
                            {
                                if (!currentRegionID.Equals(nextRegionID))
                                {

                                    information._turnDirection =
                                        TurnDirection.FirstDirection;
                                    RegionEdge regionEdge =
                                        GetRegionEdgeMostNearSourceWaypoint
                                                        (currentRegionID,
                                                         currentWaypointID,
                                                         nextRegionID,
                                                         avoidConnectionTypes);
                                    information._connectionType =
                                        regionEdge._connectionType;

                                    information._distance =
                                        System.Convert.ToInt32
                                                        (regionEdge._distance);

                                    information.
                                        _relatedDirectionOfFirstDirection
                                            = regionEdge._direction;
                                }
                                else
                                {
                                    information._turnDirection =
                                        TurnDirection.FirstDirection;
                                    WaypointEdge currentEdge =
                                        GetWaypointEdgeInRegion
                                                         (currentRegionID,
                                                          currentWaypointID,
                                                          nextWaypointID,
                                                          avoidConnectionTypes);
                                    information._connectionType =
                                        currentEdge._connectionType;
                                    information.
                                        _relatedDirectionOfFirstDirection =
                                            currentEdge._direction;
                                    information._distance =
                                        System.Convert
                                              .ToInt32(currentEdge._distance);

                                }
                                // previousWaypoint and currentWaypoint are on 
                                // different floor need to refine the 
                                // turndirection in this case

                            }
                            else
                            {

                                // previousWaypoint and currentWaypoint are on the same floor
                                RegionEdge prevEdge =
                                    GetRegionEdgeMostNearSourceWaypoint
                                                         (previousRegionID,
                                                          previousWaypointID,
                                                          currentRegionID,
                                                          avoidConnectionTypes);
                                CardinalDirection prevEdgeDirection =
                                    prevEdge._direction;

                                RegionEdge currentEdge =
                                    GetRegionEdgeMostNearSourceWaypoint
                                                         (currentRegionID,
                                                          currentWaypointID,
                                                          nextRegionID,
                                                          avoidConnectionTypes);
                                CardinalDirection currentEdgeDirection =
                                    currentEdge._direction;

                                int prevDirection =
                                    System.Convert.ToInt32(prevEdgeDirection);

                                int currentDirection =
                                    System.Convert.ToInt32(currentEdgeDirection);

                                if (currentDirection - prevDirection >= 0)
                                {
                                    information._turnDirection =
                                        (TurnDirection)
                                        (currentDirection - prevDirection);
                                }
                                else
                                {
                                    information._turnDirection =
                                        (TurnDirection)
                                        (currentDirection - prevDirection + 8);
                                }
                                information._connectionType =
                                    currentEdge._connectionType;

                                information._distance =
                                    System.Convert
                                          .ToInt32(currentEdge._distance);
                            }
                        }
                        else
                        {
                            // previousWaypoint and currentWaypoint are in the 
                            // same region
                            WaypointEdge prevEdge =
                                GetWaypointEdgeInRegion(previousRegionID,
                                                        previousWaypointID,
                                                        currentWaypointID,
                                                        avoidConnectionTypes);
                            CardinalDirection prevEdgeDirection =
                                prevEdge._direction;

                            RegionEdge currentEdge =
                                GetRegionEdgeMostNearSourceWaypoint
                                                        (currentRegionID,
                                                         currentWaypointID,
                                                         nextRegionID,
                                                         avoidConnectionTypes);
                            CardinalDirection currentEdgeDirection =
                                currentEdge._direction;

                            int prevDirection =
                                System.Convert.ToInt32(prevEdgeDirection);

                            int currentDirection =
                                System.Convert.ToInt32(currentEdgeDirection);

                            if (currentDirection - prevDirection >= 0)
                            {
                                information._turnDirection =
                                    (TurnDirection)
                                    (currentDirection - prevDirection);
                            }
                            else
                            {
                                information._turnDirection =
                                    (TurnDirection)
                                    (currentDirection - prevDirection + 8);
                            }
                            information._connectionType =
                                currentEdge._connectionType;
                            information._distance =
                                System.Convert.ToInt32(currentEdge._distance);
                        }
                    }
                }
            }
            else
            {
                // currentWaypoint and nextWaypoint are in the same region

                if (0 == currentNavigationStep)
                {
                    // first waypoint from the beginning
                    // need to refine the turndirection in this case
                    information._turnDirection = TurnDirection.FirstDirection;

                    WaypointEdge currentEdge =
                        GetWaypointEdgeInRegion(currentRegionID,
                                                currentWaypointID,
                                                nextWaypointID,
                                                avoidConnectionTypes);

                    information._connectionType = currentEdge._connectionType;

                    information._relatedDirectionOfFirstDirection =
                        currentEdge._direction;

                    information._distance = System.Convert
                                            .ToInt32(currentEdge._distance);
                }
                else
                {
                    Console.WriteLine("current = next case");
                    if (!previousRegionID.Equals(currentRegionID))
                    {
                        Console.WriteLine("previous != current case");
                        // currentWaypoint and nextWaypoint are in the same 
                        // region.
                        // previouWaypoint and currentWaypoint are acrss regions
                        if (!_regions[previousRegionID]._floor.Equals(
                            _regions[currentRegionID]._floor))
                        {
                            // previousWaypoint and currentWaypoint are on 
                            // different floor need to refine the turndirection 
                            // in this case
                            information._turnDirection =
                                TurnDirection.FirstDirection;

                            WaypointEdge currentEdge =
                                GetWaypointEdgeInRegion(currentRegionID,
                                                        currentWaypointID,
                                                        nextWaypointID,
                                                        avoidConnectionTypes);
                            information._connectionType =
                                currentEdge._connectionType;

                            information._relatedDirectionOfFirstDirection =
                                currentEdge._direction;

                            information._distance =
                                System.Convert
                                      .ToInt32(currentEdge._distance);
                        }
                        else
                        {
                            // previousWaypoint and currentWaypoint are on the 
                            // same floor
                            RegionEdge prevEdge =
                                GetRegionEdgeMostNearSourceWaypoint
                                                         (previousRegionID,
                                                          previousWaypointID,
                                                          currentRegionID,
                                                          avoidConnectionTypes);
                            CardinalDirection prevEdgeDirection =
                                prevEdge._direction;

                            WaypointEdge currentEdge =
                                GetWaypointEdgeInRegion(currentRegionID,
                                                        currentWaypointID,
                                                        nextWaypointID,
                                                        avoidConnectionTypes);
                            CardinalDirection currentEdgeDirection =
                                currentEdge._direction;

                            int prevDirection =
                                System.Convert.ToInt32(prevEdgeDirection);

                            int currentDirection =
                                System.Convert.ToInt32(currentEdgeDirection);

                            if (currentDirection - prevDirection >= 0)
                            {
                                information._turnDirection =
                                    (TurnDirection)
                                    (currentDirection - prevDirection);
                            }
                            else
                            {
                                information._turnDirection =
                                    (TurnDirection)
                                    (currentDirection - prevDirection + 8);
                            }
                            information._connectionType =
                                currentEdge._connectionType;
                            information._distance =
                                System.Convert.ToInt32(currentEdge._distance);
                        }
                    }
                    else
                    {
                        Console.WriteLine("previous = current case");
                        // currentWaypoint and nextWaypoint are in the same 
                        // region

                        // previousWaypoint and currentWaypoint are in the same 
                        // region

                        WaypointEdge prevEdge =
                            GetWaypointEdgeInRegion(previousRegionID,
                                                    previousWaypointID,
                                                    currentWaypointID,
                                                    avoidConnectionTypes);
                        CardinalDirection prevEdgeDirection =
                            prevEdge._direction;

                        WaypointEdge currentEdge =
                            GetWaypointEdgeInRegion(currentRegionID,
                                                    currentWaypointID,
                                                    nextWaypointID,
                                                    avoidConnectionTypes);
                        CardinalDirection currentEdgeDirection =
                            currentEdge._direction;

                        int prevDirection =
                            System.Convert.ToInt32(prevEdgeDirection);
                        int currentDirection =
                            System.Convert.ToInt32(currentEdgeDirection);

                        if (currentDirection - prevDirection >= 0)
                        {
                            information._turnDirection =
                                (TurnDirection)
                                (currentDirection - prevDirection);
                        }
                        else
                        {
                            information._turnDirection =
                                (TurnDirection)
                                (currentDirection - prevDirection + 8);
                        }
                        information._connectionType =
                            currentEdge._connectionType;

                        information._distance =
                            System.Convert.ToInt32(currentEdge._distance);
                    }
                }
            }
            return information;
        }

        public Graph<Guid, string> GenerateRegionGraph
                                         (ConnectionType[] avoidConnectionTypes)
        {
            Graph<Guid, string> graph = new Graph<Guid, string>();

            foreach (KeyValuePair<Guid, Region> regionItem in _regions)
            {
                graph.AddNode(regionItem.Key);
            }

            foreach (KeyValuePair<Tuple<Guid, Guid>,
                List<RegionEdge>> regionEdgeItem in _edges)
            {
                Guid node1 = regionEdgeItem.Key.Item1;
                Guid node2 = regionEdgeItem.Key.Item2;

                uint node1Key = graph.Where(node => node.Item.Equals(node1))
                                .Select(node => node.Key).First();
                uint node2Key = graph.Where(node => node.Item.Equals(node2))
                                .Select(node => node.Key).First();

                int node1EdgeDistance = Int32.MaxValue;
                int node1EdgeIndex = -1;
                int node2EdgeDistance = Int32.MaxValue;
                int node2EdgeIndex = -1;
                for (int i = 0; i < regionEdgeItem.Value.Count(); i++)
                {
                    RegionEdge edgeItem = regionEdgeItem.Value[i];
                    if (!avoidConnectionTypes.Contains(edgeItem._connectionType))
                    {

                        if (
                            DirectionalConnection.BiDirection ==
                            edgeItem._biDirection ||
                            (DirectionalConnection.OneWay ==
                            edgeItem._biDirection && 1 == edgeItem._source))
                        {
                            int edgeDistance =
                                System.Convert.ToInt32(edgeItem._distance);

                            if (edgeDistance < node1EdgeDistance)
                            {
                                node1EdgeDistance = edgeDistance;
                                node1EdgeIndex = i;
                            }
                        }

                        if (DirectionalConnection.BiDirection ==
                            edgeItem._biDirection ||
                           (DirectionalConnection.OneWay ==
                            edgeItem._biDirection && 2 == edgeItem._source))
                        {
                            int edgeDistance =
                                System.Convert.ToInt32(edgeItem._distance);
                            if (edgeDistance < node2EdgeDistance)
                            {
                                node2EdgeDistance = edgeDistance;
                                node2EdgeIndex = i;
                            }
                        }

                    }
                }
                if (-1 != node1EdgeIndex)
                {
                    graph.Connect(node1Key,
                                  node2Key,
                                  node1EdgeDistance,
                                  String.Empty);
                }
                if (-1 != node2EdgeIndex)
                {
                    graph.Connect(node2Key,
                                  node1Key,
                                  node2EdgeDistance,
                                  String.Empty);
                }
            }

            return graph;
        }

        public Graph<Guid, string> GenerateNavigraph
                                         (Guid regionID,
                                          ConnectionType[] avoidConnectionTypes)
        {
            Graph<Guid, string> graph = new Graph<Guid, string>();

            foreach (KeyValuePair<Guid, Waypoint> waypointItem
                     in _navigraphs[regionID]._waypoints)
            {
                graph.AddNode(waypointItem.Key);
            }

            foreach (KeyValuePair<Tuple<Guid, Guid>, WaypointEdge> waypointEdgeItem
                     in _navigraphs[regionID]._edges)
            {
                Guid node1 = waypointEdgeItem.Key.Item1;
                Guid node2 = waypointEdgeItem.Key.Item2;
                uint node1Key = graph.Where(node => node.Item.Equals(node1))
                                  .Select(node => node.Key).First();
                uint node2Key = graph.Where(node => node.Item.Equals(node2))
                                  .Select(node => node.Key).First();

                // should refine distance, bi-direction, direction, connection 
                // type later
                int distance = Int32.MaxValue;
                Tuple<Guid, Guid> edgeKey = new Tuple<Guid, Guid>(node1, node2);
                WaypointEdge edgeItem = _navigraphs[regionID]._edges[edgeKey];
                if (!avoidConnectionTypes.Contains(edgeItem._connectionType))
                {
                    distance = System.Convert.ToInt32(edgeItem._distance);

                    if (DirectionalConnection.BiDirection
                            == edgeItem._biDirection)
                    {
                        // Graph.Connect is on-way, not bi-drectional
                        graph.Connect(node1Key,
                                      node2Key,
                                      distance,
                                      String.Empty);

                        graph.Connect(node2Key,
                                      node1Key,
                                      distance,
                                      String.Empty);
                    }
                    else if (DirectionalConnection.OneWay ==
                                edgeItem._biDirection)
                    {
                        if (1 == edgeItem._source)
                        {
                            graph.Connect(node1Key,
                                          node2Key,
                                          distance,
                                          String.Empty);
                        }
                        else if (2 == edgeItem._source)
                        {
                            graph.Connect(node2Key,
                                          node1Key,
                                          distance,
                                          String.Empty);
                        }
                    }
                }
            }

            return graph;
        }

        #region struct of graph
        public struct RegionEdge
        {
            public Guid _region1 { get; set; }
            public Guid _region2 { get; set; }
            public Guid _waypoint1 { get; set; }
            public Guid _waypoint2 { get; set; }
            public DirectionalConnection _biDirection { get; set; }
            public int _source { get; set; }
            public double _distance { get; set; }
            public CardinalDirection _direction { get; set; }
            public ConnectionType _connectionType { get; set; }
        }

        public struct WaypointEdge
        {
            public Guid _node1 { get; set; }
            public Guid _node2 { get; set; }
            public DirectionalConnection _biDirection { get; set; }
            public int _source { get; set; }
            public CardinalDirection _direction { get; set; }
            public ConnectionType _connectionType { get; set; }
            public double _distance { get; set; }
        }        
        #endregion
        
    }



    #region enums and struct of graph
    public struct InstructionInformation
    {
        public TurnDirection _turnDirection { get; set; }
        public CardinalDirection _relatedDirectionOfFirstDirection { get; set; }
        public ConnectionType _connectionType { get; set; }
        public int _floor { get; set; }
        public string _regionName { get; set; }
        public int _distance { get; set; }
    }
    public enum LocationType
    {
        landmark = 0,
        junction, //junction_branch, 
        midpath,
        terminal, //terminal_destination,
        portal
    }

    public enum CardinalDirection
    {
        North = 0,
        Northeast,
        East,
        Southeast,
        South,
        Southwest,
        West,
        Northwest
    }

    public enum TurnDirection
    {
        FirstDirection = -1, // Exception: used only in the first step
        Forward = 0,
        Forward_Right,
        Right,
        Backward_Right,
        Backward,
        Backward_Left,
        Left,
        Forward_Left,
        Up,
        Down
    }

    public enum IPSType
    {
        LBeacon = 0,
        iBeacon,
        GPS
    }

    public enum DirectionalConnection
    {
        OneWay = 1,
        BiDirection = 2
    }

    public enum ConnectionType
    {
        NormalHallway = 0,
        Stair,
        Elevator,
        Escalator,
        VirtualHallway
    }

    public enum CategoryType
    {
        Others = 0,
        Clinics,
        Cashier,
        Exit,
        ExaminationRoom,
        Pharmacy,
        ConvenienceStore,
        Bathroom,
        BloodCollectionCounter,
        Elevator,
        Parking,
        Office,
        ConferenceRoom,
        Stair
    }

    public enum waypointDecisionOrIgnore
    {
        DecisionWaypoint,
        IgnoreWaypoint
    }
    #endregion
}
