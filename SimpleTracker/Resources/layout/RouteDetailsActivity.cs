using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Widget;

using SimpleDatabase;

using SimpleTracker.Activities;
using SimpleTracker.Common;
using SimpleTracker.Dialogs;

using StravaIntegrator;
using StravaIntegrator.Models;

using V7 = Android.Support.V7.Widget;

namespace SimpleTracker.Resources.layout
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class RouteDetailsActivity : BaseApplicationActivity
    {
        private SimpleGpsDatabase database;
        private StravaRoutePublishDialog publishRouteDialog;
        private long? stravaActivityid = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.route_details);

            this.database = SimpleGpsDatabase.Instance;

            V7.Toolbar toolbar = FindViewById<V7.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            int id = Intent.Extras.GetInt("id");

            List<SimpleGpsLocation> gpsLocations = this.database.GetRouteLocations(id);
            SimpleGpsRoute route = this.database.GetRoute(id);
            stravaActivityid = route.StravaActivityId;

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
                durationText += $"{(int)Math.Floor(duration.TotalHours):D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
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
            FindViewById<Button>(Resource.Id.strava_view_activity_button).Click += ViewRouteDetailsClick;
        }

        protected override void OnResume()
        {
            base.OnResume();

            int id = Intent.Extras.GetInt("id");

            SimpleGpsRoute route = this.database.GetRoute(id);

            SimpleGpsSettings settings = this.database.GetSettings();
            bool isStravaAuthenticated = !string.IsNullOrWhiteSpace(settings.StravaRefreshToken);
            if (route.StravaActivityId.HasValue)
            {
                FindViewById<Button>(Resource.Id.strava_view_activity_button).Visibility = Android.Views.ViewStates.Visible;
                FindViewById<Button>(Resource.Id.strava_publish_route_button).Visibility = Android.Views.ViewStates.Gone;
            }
            else
            {
                if (isStravaAuthenticated)
                {
                    FindViewById<Button>(Resource.Id.strava_publish_route_button).Visibility = Android.Views.ViewStates.Visible;
                    FindViewById<Button>(Resource.Id.strava_publish_route_button).Click += PublishToStrava_Click;
                }
                else
                {
                    // Make button gray
                    FindViewById<Button>(Resource.Id.strava_publish_route_button).Visibility = Android.Views.ViewStates.Invisible;
                    // show message Strava is not authenticated
                }

                FindViewById<Button>(Resource.Id.strava_view_activity_button).Visibility = Android.Views.ViewStates.Gone;
            }
        }

        private void DeleteRoute_Click(object sender, EventArgs e)
        {
            int routeId = int.Parse(FindViewById<TextView>(Resource.Id.routeDetailsId).Text);
            database.DeleteRouteWithPath(routeId);

            Toast.MakeText(this, $"Route deleted!", ToastLength.Short).Show();

            Finish();
        }

        private void PublishToStrava_Click(object sender, EventArgs e)
        {
            publishRouteDialog = new StravaRoutePublishDialog(this.ConfirmPublishToStrava_Click);
            publishRouteDialog.Show(SupportFragmentManager, publishRouteDialog.GetType().Name);
        }

        private void ConfirmPublishToStrava_Click(object sender, PublishActivity activity)
        {
            SimpleGpsSettings settings = this.database.GetSettings();
            bool shouldRefreshToken = Utilities.ShouldRefreshAccessToken(settings.StravaAccessTokenExpirationDate.Value);

            string accessToken = Utilities.DecryptValue(settings.StravaAccessToken);

            // This should be in a separate logic
            if (shouldRefreshToken)
            {
                AuthorizationTokens newTokenInformation = new StravaAuthentication().GetAuthotizationsTokens(
                    Utilities.DecryptValue(settings.StravaRefreshToken),
                    ApplicationSecrets.Strava.ClientId,
                    ApplicationSecrets.Strava.ClientSecret,
                    StravaAuthentication.GrantTypeRefreshToken);

                accessToken = newTokenInformation.AccessToken;

                settings.StravaAccessToken = Utilities.EncryptValue(newTokenInformation.AccessToken);
                settings.StravaAccessTokenExpirationDate = newTokenInformation.AccessTokenExpireDate;
                this.database.UpdateSettings(settings);
            }

            int id = Intent.Extras.GetInt("id");
            List<SimpleGpsLocation> gpsLocations = this.database.GetRouteLocations(id);

            PackageInfo packageInfo = PackageManager.GetPackageInfo(PackageName, 0);
            UploadActivityModel result = StravaPublisher
                .Publish(gpsLocations, accessToken, activity, packageInfo);

            publishRouteDialog.Dismiss();

            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                Snackbar.Make(FindViewById<TextView>(Resource.Id.routeDetailsId), result.Error, Snackbar.LengthLong).Show();
            }
            else
            {
                this.database.UpdateRouteStravaActivityId(id, result.Id);

                Snackbar.Make(FindViewById<TextView>(Resource.Id.routeDetailsId), "Published!", Snackbar.LengthLong).Show();

                OnResume();
            }
        }

        private void ViewRouteDetailsClick(object sender, EventArgs e)
        {
            if (stravaActivityid.HasValue)
            {
                var builder = new UriBuilder($"https://www.strava.com/activities/{stravaActivityid.Value}");

                var uri = Android.Net.Uri.Parse(builder.ToString());
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            }
        }
    }

    public enum ElevationDirection
    {
        None,
        Climbing,
        Descending
    }
}