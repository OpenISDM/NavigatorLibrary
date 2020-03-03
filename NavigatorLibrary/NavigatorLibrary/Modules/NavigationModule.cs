using IndoorNavigation.Modules;
using NavigatorLibrary.Models;
using NavigatorLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
namespace NavigatorLibrary.Modules
{
    public class NavigationModule:IDisposable
    {
        private Session _session;

        private string _navigationGraphName;

        private Guid _destinationRegionID;
        private Guid _destinationWaypointID;

        private EventHandler _navigationResultEventHandler;
        public NavigationEvent _event { get; private set; }

        public NavigationModule(string navigationGraphName,
                                Guid destinationRegionID,
                                Guid destinationWaypointID)
        {
            _event = new NavigationEvent();
            
            _navigationGraphName = navigationGraphName;
            _destinationRegionID = destinationRegionID;
            _destinationWaypointID = destinationWaypointID;

            ConstructSession();
        }

        /// <summary>
        /// If it is the first time to get waypoint then get the value of 
        /// route options and start the corresponding session.
        /// </summary>
        private void ConstructSession()
        {
            List<ConnectionType> avoidList = new List<ConnectionType>();

            Console.WriteLine("-- setup preference --- ");
            if (Application.Current.Properties.ContainsKey("AvoidStair"))
            {
                avoidList.Add(
                        (bool)Application.Current.Properties["AvoidStair"] ?
                         ConnectionType.Stair : ConnectionType.NormalHallway);
                avoidList.Add(
                        (bool)Application.Current.Properties["AvoidElevator"] ?
                        ConnectionType.Elevator : ConnectionType.NormalHallway);
                avoidList.Add(
                        (bool)Application.Current.Properties["AvoidEscalator"] ?
                        ConnectionType.Escalator : ConnectionType.NormalHallway);

                avoidList = avoidList.Distinct().ToList();
                avoidList.Remove(ConnectionType.NormalHallway);
            }
            Console.WriteLine("-- end of setup preference --- ");

            // Start the session
            _session = new Session(
                    NavigraphStorage.LoadNavigationGraphXML(
                            PhoneInformation.
                                GetCurrentMapName(_navigationGraphName)
                        ),
                    _destinationRegionID,
                    _destinationWaypointID,
                    avoidList.ToArray());

            _navigationResultEventHandler =
                new EventHandler(HandleNavigationResult);
            _session._event._eventHandler += _navigationResultEventHandler;

        }

        /// <summary>
        /// Get the navigation result from the session and 
        /// raise event to notify the NavigatorPageViewModel.
        /// </summary>
        private void HandleNavigationResult(object sender, EventArgs args)
        {
            _event.OnEventCall(args);
        }

        public void Stop()
        {
            _session.CloseSession();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                }
                // Free unmanaged resources (unmanaged objects) and override a 
                //finalizer below.
                // Set large fields to null.
                _session._event._eventHandler -= _navigationResultEventHandler;

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
