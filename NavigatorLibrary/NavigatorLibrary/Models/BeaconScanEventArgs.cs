using System;
using System.Collections.Generic;
using System.Text;

namespace NavigatorLibrary.Models
{
    public class BeaconScanEventArgs : EventArgs
    {
        public List<BeaconSignalModel> _signals { get; set; }
    }
}
