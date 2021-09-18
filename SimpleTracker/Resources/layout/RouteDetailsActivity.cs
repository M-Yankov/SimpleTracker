using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Support.V7.App;
using Android.Text;
using Android.Widget;

using Java.Lang;

using SimpleTracker.Database;

using V7 = Android.Support.V7.Widget;

namespace SimpleTracker.Resources.layout
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class RouteDetailsActivity : AppCompatActivity
    {
        private Database.SimpleGpsDatabase database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.route_details);

            this.database = Database.SimpleGpsDatabase.Instance;

            V7.Toolbar toolbar = FindViewById<V7.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            int id = Intent.Extras.GetInt("id");

            List<SimpleGpsLocation> gpsLocations = this.database.GetPath(id);
            SimpleGpsRoute route = this.database.GetRoute(id);

            // statistics calculated may not be accurate, due to incorrect locations provided by GPS provider
            float distanceInMeters = 0;

            double maxElevation = double.MinValue;
            double minElevation = double.MaxValue;

            double climbing = 0;
            double descending = 0;

            double maxSpeed = double.MinValue;

            List<double> speeds = new List<double>();
            if (gpsLocations.Count > 1)
            {
                for (int i = 0; i < gpsLocations.Count - 1; i++)
            {
                SimpleGpsLocation previousPoint = gpsLocations[i];
                SimpleGpsLocation nextPoint = gpsLocations[i + 1];

                float[] results = new float[3];
                Location.DistanceBetween(
                    previousPoint.Latitude,
                    previousPoint.Longitude,
                    nextPoint.Latitude,
                    nextPoint.Longitude,
                    results);

                distanceInMeters += results[0];

                ElevationDirection elevationDirection;

                double higherPoint;
                double lowerPoint;
                
                if (previousPoint.Altitude > nextPoint.Altitude)
                {
                    higherPoint = previousPoint.Altitude;
                    lowerPoint = nextPoint.Altitude;
                    elevationDirection = ElevationDirection.Descending;
                }
                else
                {
                    lowerPoint = previousPoint.Altitude;
                    higherPoint = nextPoint.Altitude;
                    elevationDirection = ElevationDirection.Climbing;
                }

                if (higherPoint > maxElevation)
                {
                    maxElevation = higherPoint;
                }

                if (lowerPoint < minElevation)
                {
                    minElevation = lowerPoint;
                }

                switch (elevationDirection)
                {
                    case ElevationDirection.Climbing:
                        climbing += higherPoint - lowerPoint;
                        break;
                    case ElevationDirection.Descending:
                        descending += lowerPoint - higherPoint;
                        break;
                    case ElevationDirection.None:
                    default:
                        break;
                }

                // S = V * T
                // distance = speed * time 
                // 3km/h * 2 (h) = 6 km

                // speed = distance / time
                // 3km/h = 6km / 2

                long elapsedTicks = nextPoint.DateTime.Ticks - previousPoint.DateTime.Ticks;
                TimeSpan timeInterval = new TimeSpan(elapsedTicks);
                float distaneBetweenPoints = results[0];

                double speed = (distaneBetweenPoints / 1000.0) / (timeInterval.TotalSeconds / 3600.0);
                speeds.Add(speed);

                if (speed > maxSpeed)
                {
                    maxSpeed = speed;
                }
            }
            }

            double average = speeds.Any() ? speeds.Average() : 0;
            string durationText = "Duration: ";

            if (gpsLocations.Count > 1)
            {
                TimeSpan duration = gpsLocations[gpsLocations.Count - 1].DateTime.Subtract(gpsLocations[0].DateTime);
                // Total hours are used, because the track could take longer than 24 hours.
                durationText += $"{(int)System.Math.Floor(duration.TotalHours):D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
            }

            FindViewById<TextView>(Resource.Id.routeDetailsName).Text = route.Name;
            FindViewById<TextView>(Resource.Id.routeDetailsDistance).Text = $"Distance: { distanceInMeters / 1000:N3} km.";
            FindViewById<TextView>(Resource.Id.routeDetailsTime).Text = durationText;
            FindViewById<TextView>(Resource.Id.routeDetailsMaxElevation).Text = $"Max Elevation: { maxElevation:F0} m.";
            FindViewById<TextView>(Resource.Id.routeDetailsMinElevation).Text = $"Min Elevation: { minElevation:F0} m.";
            FindViewById<TextView>(Resource.Id.routeDetailsClimbing).Text = $"Elevation gain ↑: { climbing:F0} m.";
            FindViewById<TextView>(Resource.Id.routeDetailsDescending).Text = $"Elevation lost ↓: { descending:F0} m.";
            FindViewById<TextView>(Resource.Id.routeDetailsAvgSpeed).Text = $"Average speed: { average:F0} km/h.";
            FindViewById<TextView>(Resource.Id.routeDetailsMaxSpeed).Text = $"Max speed: { maxSpeed:F0} km/h.";
            FindViewById<TextView>(Resource.Id.routeDetailsId).Text = $"{id}";

            FindViewById<Button>(Resource.Id.delete_route_button).Click += DeleteRoute_Click;
        }

        private void DeleteRoute_Click(object sender, EventArgs e)
        {
            int routeId = int.Parse(FindViewById<TextView>(Resource.Id.routeDetailsId).Text);
            database.DeleteRouteWithPath(routeId);

            Toast.MakeText(this, $"Route deleted!", ToastLength.Short).Show();

            Finish();
        }
    }

    public enum ElevationDirection
    {
        None,
        Climbing,
        Descending
    }
}