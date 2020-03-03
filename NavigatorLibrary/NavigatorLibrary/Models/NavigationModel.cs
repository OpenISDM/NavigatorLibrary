using System;
using System.Collections.Generic;
using System.Text;

namespace NavigatorLibrary.Models
{
    public class BeaconSignal
    {
         public Guid UUID { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public int RSSI { get; set; }
    }
    public class BeaconSignalModel : BeaconSignal
    {
        public int TxPower { get; set; }
        public DateTime Timestamp { get; set; }

        public BeaconSignalModel()
        {
            Timestamp = DateTime.Now;
        }
    }
}
