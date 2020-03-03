using System;
using System.Collections.Generic;
using System.Text;

namespace NavigatorLibrary.Models
{
    public class NavigationEvent
    {
        public event EventHandler _eventHandler;

        public void OnEventCall(EventArgs e)
        {
            _eventHandler?.Invoke(this, e);
        }
    }    
}
