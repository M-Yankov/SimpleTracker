using Android.Locations;
using Android.OS;
using Android.Runtime;

namespace SimpleTracker
{
    public class GpsLocationListener : Java.Lang.Object, ILocationListener 
    {
        public void OnLocationChanged(Location location)
        {
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
        }
    }
}