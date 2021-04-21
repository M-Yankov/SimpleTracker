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
        /*
         * private Handler handler;
            private Action runnable;
        */
        private LocationManager gpsManager;
        private SimpleGpsLocationListener gpsListener;
        private NotificationManager notificationManager;

        private bool isStarted = false;

        private const int GpsNotificationId = 1012;
        public override void OnCreate()
        {
            gpsManager = (LocationManager)GetSystemService(LocationService);
            notificationManager = (NotificationManager)GetSystemService(NotificationService);

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

        /// <summary>
        /// Copied from https://github.com/xamarin/monodroid-samples/blob/0b301e8fd2da65ff442b5f1ed236c73ba3b963c2/ApplicationFundamentals/ServiceSamples/ForegroundServiceDemo/TimestampService.cs#L90
        /// </summary>
        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder that would
            // allow access to the GetFormattedStamp() method.
            return null;
        }

        public override void OnDestroy()
        {
            gpsListener.PositionChanged -= Current_PositionChanged;
            gpsListener.ProviderDisabled -= GpsListener_ProviderDisabled;
            gpsManager.RemoveUpdates(gpsListener);
            this.UnRegisterService();
            base.OnDestroy();
        }

        private void RegisterService()
        {
            if (isStarted)
            {
                return;
            }

            isStarted = true;

            // The constructor is deprecated, but it's necessary for old androids.
            var notification = new Notification.Builder(this)
               .SetContentTitle("Test")
               .SetContentText("This is some text")
               .SetSmallIcon(Resource.Drawable.Image) // This is required, otherwise default system text and message are displayed
               .SetOngoing(true);

            StartForeground(GpsNotificationId, notification.Build());

            gpsListener.PositionChanged += Current_PositionChanged;
            gpsListener.ProviderDisabled += GpsListener_ProviderDisabled;
            gpsManager.RequestLocationUpdates(LocationManager.GpsProvider, minTime: 1000, minDistance: 5, gpsListener);

            // This is how to update a notification text
            /*
             * notification
                .SetContentText("UpdatedText");

                notificationManager.Notify(GpsNotificationId, notification.Build());
            */
        }

        private void Current_PositionChanged(object sender, PositionEventArgs e)
        {
            Android.Util.Log.Debug("LOG:", $"PostionChanged L:{e.Location.Latitude}");
        }

        private void GpsListener_ProviderDisabled(object sender, EventArgs e)
        {
            if (!gpsManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                Intent gpsOptionsIntent = new Intent(
                    Android.Provider.Settings.ActionLocationSourceSettings);

                StartActivity(gpsOptionsIntent);
            }
        }

        /// <summary>
        /// I wonder when this method is invoked...
        /// </summary>
        private void UnRegisterService()
        {
            StopForeground(true);
            StopSelf();
            isStarted = false;
        }
    }
}