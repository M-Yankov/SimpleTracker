using Android.OS;

using SimpleTracker.Services;

namespace SimpleTracker.Binders
{
    public class GpsTrackerServiceBinder : Binder
    {
        public GpsTrackerServiceBinder(GpsTrackerService service)
        {
            this.Service = service;
        }

        public GpsTrackerService Service { get; set; }
    }
}