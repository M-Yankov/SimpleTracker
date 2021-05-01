using Android.Locations;
using Android.OS;
using Android.Runtime;

using System;

namespace SimpleTracker
{
    public class SimpleGpsLocationListener : Java.Lang.Object, ILocationListener 
    {
        public event EventHandler<PositionEventArgs> PositionChanged;
        public event EventHandler<EventArgs> ProviderDisabled;

        public void OnLocationChanged(Location location)
        {
            PositionChanged?.Invoke(this, new PositionEventArgs() { Location = location });
        }

        public void OnProviderDisabled(string provider)
        {
            // If GPS is disabled from settings. 
            if (string.Equals(provider, LocationManager.GpsProvider, StringComparison.InvariantCultureIgnoreCase))
            {
                ProviderDisabled?.Invoke(this, new EventArgs());
            }
        }

        public void OnProviderEnabled(string provider)
        {
            // When enable GPS location from settings.
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            // First time when request for location changes is made with status = TemporarilyUnavailable.
            // Second time invoked with status = available.
         }
    }

    public class PositionEventArgs : EventArgs
    {
        public Location Location { get; set; }
    }
}