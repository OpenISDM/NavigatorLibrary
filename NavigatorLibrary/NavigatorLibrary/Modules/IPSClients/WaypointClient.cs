using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NavigatorLibrary.Models;
using NavigatorLibrary.Utilities;
using Xamarin.Forms;
namespace NavigatorLibrary.Modules.IPSClients
{
    class WaypointClient:IIPSClient
    {
        private List<WaypointBeaconsMapping> _waypointBeaconsList =
           new List<WaypointBeaconsMapping>();

        private object _bufferLock;// = new object();
        private readonly EventHandler _beaconScanEventHandler;
        private const int _clockResetTime = 90000;
        public NavigationEvent _event { get; private set; }
        private List<BeaconSignalModel> _beaconSignalBuffer =
            new List<BeaconSignalModel>();

        private int rssiOption;
        private System.Diagnostics.Stopwatch watch =
            new System.Diagnostics.Stopwatch();

        public WaypointClient()
        {
            _event = new NavigationEvent();
            Utility._lbeaconScan = DependencyService.Get<LBeaconScan>();
            _beaconScanEventHandler = new EventHandler(HandleBeaconScan);
            Utility._lbeaconScan._event._eventHandler += _beaconScanEventHandler;
            _waypointBeaconsList = new List<WaypointBeaconsMapping>();
            rssiOption = 0;
            _bufferLock = new object();
            watch.Start();
        }

        public void SetWaypointList
            (List<WaypointBeaconsMapping> waypointBeaconsList)
        {

            if (Application.Current.Properties.ContainsKey("StrongRssi"))
            {
                if ((bool)Application.Current.Properties["StrongRssi"] == true)
                {
                    rssiOption = 5;
                }
                else if ((bool)Application.Current.Properties["WeakRssi"] == true)
                {
                    rssiOption = -5;
                }
                else if ((bool)Application.Current.Properties["MediumRssi"] == true)
                {
                    rssiOption = 0;
                }
            }

            this._waypointBeaconsList = waypointBeaconsList;
            Utility._lbeaconScan.StartScan();
        }

        public void DetectWaypoints()

        {

            Console.WriteLine(">> In DetectWaypoints");
            //Utility._beaconScan.StartScan();
            // Remove the obsolete data from buffer
            List<BeaconSignalModel> removeSignalBuffer =
            new List<BeaconSignalModel>();

            lock (_bufferLock)
            {
                removeSignalBuffer.AddRange(
                _beaconSignalBuffer.Where(c =>
                c.Timestamp < DateTime.Now.AddMilliseconds(-500)));

                if (watch.Elapsed.TotalMilliseconds >= _clockResetTime)
                {
                    watch.Stop();
                    watch.Reset();
                    watch.Start();
                    Utility._ibeaconScan.StopScan();
                    Utility._ibeaconScan.StartScan();

                }

                foreach (var obsoleteBeaconSignal in removeSignalBuffer)
                    _beaconSignalBuffer.Remove(obsoleteBeaconSignal);

                //Sort beacons through their RSSI, to let the stronger beacon 
                //can get in first
                _beaconSignalBuffer.Sort
                (
                    (x, y) =>
                    {
                        return y.RSSI.CompareTo(x.RSSI);
                    }
                );


                foreach (BeaconSignalModel beacon in _beaconSignalBuffer)
                {
                    foreach (WaypointBeaconsMapping waypointBeaconsMapping
                                in _waypointBeaconsList)
                    {
                        foreach (Guid beaconGuid in
                                    waypointBeaconsMapping._Beacons)
                        {
                            if (beacon.UUID.Equals(beaconGuid))
                            {
                                Console.WriteLine("Matched waypoint: {0} by" +
                                                  "detected Beacon {1}",
                                waypointBeaconsMapping._WaypointIDAndRegionID
                                                      ._waypointID,
                                beaconGuid);
                                if (beacon.RSSI >
                                    (waypointBeaconsMapping
                                     ._BeaconThreshold[beacon.UUID] - rssiOption))
                                {
                                    watch.Stop();
                                    watch.Reset();
                                    watch.Start();
                                    _event.OnEventCall(new WaypointSignalEventArgs
                                    {
                                        _detectedRegionWaypoint =
                                            waypointBeaconsMapping.
                                                _WaypointIDAndRegionID
                                    });
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("<< In DetectWaypoints");
        }

        private void HandleBeaconScan(object sender, EventArgs e)
        {
            IEnumerable<BeaconSignalModel> signals =
            (e as BeaconScanEventArgs)._signals;

            foreach (BeaconSignalModel signal in signals)
            {
                Console.WriteLine("Detected Beacon UUID : " + signal.UUID +
                                  " RSSI = " + signal.RSSI);
            }

            lock (_bufferLock)
                _beaconSignalBuffer.AddRange(signals);

        }

        public void Stop()
        {
            _bufferLock = new object();
            Utility._lbeaconScan.StopScan();
            _beaconSignalBuffer.Clear();
            _waypointBeaconsList.Clear();
            Utility._lbeaconScan._event._eventHandler -= _beaconScanEventHandler;
            watch.Stop();
        }
    }


    public class WaypointSignalEventArgs : EventArgs
    {
        public RegionWaypointPoint _detectedRegionWaypoint { get; set; }
    }
}
