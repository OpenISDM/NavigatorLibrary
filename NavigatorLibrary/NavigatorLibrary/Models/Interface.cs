using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace NavigatorLibrary.Models
{
    #region Interface for dependency service of iOS and Android version.
    public interface IBeaconScan
    {
        void StartScan();
        void StopScan();
        void Close();
        NavigationEvent _event { get; }
    }

    public interface LBeaconScan
    {
        void StartScan();
        void StopScan();
        void Close();
        NavigationEvent _event { get; }
    }

    public interface IQrCodeDecoder
    {
        Task<string> ScanAsync();
    }

    public interface ITextToSpeech
    {
        void Speak(string text, string language);
    }

    public interface IAddToolbarItem
    {
        event EventHandler ToolbarItemAdded;
        Color CellBackgroundColor { get; }
        Color CellTextColor { get; }
        Color MenuBackgroundColor { get; }
        float RowHeight { get; }
        Color ShadowColor { get; }
        float ShadowOpacity { get; }
        float ShadowRadius { get; }
        float ShadowOffsetDimension { get; }
        float TableWidth { get; }
    }
    public interface INetworkSetting
    {
        Task<bool> CheckInternetConnect();
        void OpenSettingPage();
    }
    #endregion
    #region Interface for IPS Client
    public interface IIPSClient
    {
        void DetectWaypoints();
        void SetWaypointList(List<WaypointBeaconsMapping> WaypointList);
        void Stop();

        NavigationEvent _event { get; }
    }

    public class WaypointBeaconsMapping
    {
        public RegionWaypointPoint _WaypointIDAndRegionID { get; set; }
        public List<Guid> _Beacons { get; set; }
        public Dictionary<Guid, int> _BeaconThreshold { get; set; }
    }
    #endregion
}
