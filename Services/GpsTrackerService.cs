using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTracker.Services
{
    [Service]
    public class GpsTrackerService : Service
    {
        private Handler handler;
        private Action runnable;

        private LocationManager gpsManager;
        private SimpleGpsLocationListener gpsListener;

        private bool isStarted = false;

        private const int GpsNorificationId = 1012;
        public override void OnCreate()
        {
            gpsManager = Application.Context.GetSystemService(Context.LocationService) as LocationManager;
            gpsListener = new SimpleGpsLocationListener();
            base.OnCreate();
        }

        [return: GeneratedEnum]//????
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            switch (intent.Action.ToUpperInvariant())
            {
                case "START":
                    RegisterService();
                    break;
                case "STOP":
                    UnRegisterService();
                    break;
                default:
                    break;
            }
            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder that would
            // allow access to the GetFormattedStamp() method.
            return null;
        }

        private void RegisterService()
        {
            if (isStarted)
            {
                return;
            }

            // The constuctor is deprecated, but it's necessary for old andorids.
            var notification = new Notification.Builder(this)
               .SetContentTitle("Test")
               .SetContentText("This is some text")
               .SetSmallIcon(Resource.Drawable.Image) // This is required, otherwise defualt system text and message are displayed
               .SetOngoing(true)
               .Build();

            isStarted = true;
            StartForeground(GpsNorificationId, notification);
        }

        private void UnRegisterService()
        {
            StopForeground(true);
            StopSelf();
            isStarted = false;
        }
    }
}