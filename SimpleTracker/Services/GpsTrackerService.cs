using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;

using SimpleDatabase;

using SimpleTracker.Binders;

namespace SimpleTracker.Services
{
    [Service]
    public class GpsTrackerService : Service
    {
        private LocationManager gpsManager;
        private SimpleGpsLocationListener gpsListener;
        private NotificationManager notificationManager;

        private Notification.Builder notification;
        private bool isStarted = false;

        private List<SimpleGpsLocation> locations;

        private const int GpsNotificationId = 1012;

        public IBinder Binder { get; set; }

        private SimpleGpsDatabase database;
        private int? currentRouteId;

        private double distanceTraveled = 0;

        public bool IsStarted => this.isStarted;

        public override void OnCreate()
        {
            this.gpsManager = (LocationManager)GetSystemService(LocationService);
            this.notificationManager = (NotificationManager)GetSystemService(NotificationService);

            this.gpsListener = new SimpleGpsLocationListener();
            this.locations = new List<SimpleGpsLocation>();

            this.database = SimpleGpsDatabase.Instance;

            base.OnCreate();
        }

        [return: GeneratedEnum]
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
            // A pure started service should return null.
            // A hybrid service would return a binder that ...?

            this.Binder = new GpsTrackerServiceBinder(this);
            return this.Binder;
        }

        public override void OnDestroy()
        {
            this.gpsListener.PositionChanged -= Current_PositionChanged;
            this.gpsListener.ProviderDisabled -= GpsListener_ProviderDisabled;
            this.gpsManager.RemoveUpdates(gpsListener);
            this.UnRegisterService();

            this.gpsManager = null;
            this.notificationManager = null;

            this.gpsListener = null;
            this.Binder = null;
            this.distanceTraveled = 0;

            if (this.locations?.Any() == true)
            {
                this.database.Add(this.locations);
            }

            this.locations = null;

            base.OnDestroy();
        }

        public IEnumerable<SimpleGpsLocation> GetStoredLocations()
        {
            return this.locations ?? Enumerable.Empty<SimpleGpsLocation>();
        }

        private void RegisterService()
        {
            if (this.isStarted)
            {
                return;
            }

            this.isStarted = true;

            // The constructor is deprecated, but it's necessary for old androids.
            this.notification = new Notification.Builder(this)
               .SetContentTitle("SimpleTracker recording...")
               .SetContentText($"Distance {this.distanceTraveled / 1000:N3} km")
               .SetSmallIcon(Resource.Drawable.Image) // This is required, otherwise default system text and message are displayed
               .SetOngoing(true);

            StartForeground(GpsNotificationId, this.notification.Build());

            SimpleGpsRoute route = new SimpleGpsRoute()
            { 
                Name = $"Route: {DateTime.Now:dd/MMM/yyyy HH:mm}"
            };

            this.database.Add(route);
            
            this.currentRouteId = route.Id;

            this.gpsListener.PositionChanged += Current_PositionChanged;
            this.gpsListener.ProviderDisabled += GpsListener_ProviderDisabled;
            this.gpsManager.RequestLocationUpdates(LocationManager.GpsProvider, minTime: 1000, minDistance: 5, this.gpsListener);
        }

        private void Current_PositionChanged(object sender, PositionEventArgs e)
        {
            // Check the speed and time properties
            this.locations.Add(new SimpleGpsLocation()
            {
                Altitude = e.Location.Altitude,
                DateTime = DateTime.UtcNow,
                Latitude = e.Location.Latitude,
                Longitude = e.Location.Longitude,
                SimpleGpsRouteId = currentRouteId.Value
            });

            if (this.locations.Count >= 25)
            {
                this.database.Add(this.locations);

                float meters = 0;
                for (int i = 0; i < this.locations.Count - 1; i++)
                {
                    SimpleGpsLocation previousPoint = this.locations[i];
                    SimpleGpsLocation nextPoint = this.locations[i + 1];

                    // Extract this point in helper class with method
                    float[] results = new float[3];
                    Location.DistanceBetween(
                        previousPoint.Latitude,
                        previousPoint.Longitude,
                        nextPoint.Latitude, 
                        nextPoint.Longitude,
                        results);

                    meters += results[0];
                }

                distanceTraveled += meters;

                this.locations = new List<SimpleGpsLocation>();
                this.notification.SetContentText($"Distance {this.distanceTraveled / 1000:N3} km");
                notificationManager.Notify(GpsNotificationId, this.notification.Build());
            }

#if DEBUG
            Android.Util.Log.Debug("LOG:", $"PostionChanged L:{e.Location.Latitude}");
#endif
        }

        private void GpsListener_ProviderDisabled(object sender, EventArgs e)
        {
            if (!gpsManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                Intent gpsOptionsIntent = new Intent(
                    Android.Provider.Settings.ActionLocationSourceSettings);

                gpsOptionsIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

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
            this.isStarted = false;
        }
    }
}