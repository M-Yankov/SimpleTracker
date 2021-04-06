using System;
using System.Threading.Tasks;

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Plugin.CurrentActivity;

using Xamarin.Essentials;

using Plugin.Geolocator;
using Android.Locations;
using Android.Content;
using System.Linq.Expressions;

namespace SimpleTracker
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private LocationManager gpsManager;
        private GpsLocationListener gpsListener;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            gpsManager = Application.Context.GetSystemService(Context.LocationService) as LocationManager;
            gpsListener = new GpsLocationListener();

            Button trackButton = FindViewById<Button>(Resource.Id.trackButton);
            trackButton.Click += TrackButton_Click;

            Button stopButton = FindViewById<Button>(Resource.Id.stopTrackButton);
            stopButton.Click += StopButton_Click;
            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            gpsManager.RemoveUpdates(gpsListener);
        }

        private void TrackButton_Click(object sender, EventArgs e)
        {
            //Location location = await Geolocation.GetLastKnownLocationAsync();
            //if (location == null)
            //{
            /*
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            // cts = new CancellationTokenSource();
            Location location = await Geolocation.GetLocationAsync(request, default);

                if (location != null)
                {
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    TextView label = FindViewById<TextView>(Resource.Id.textView1);
                    label.Text = $"{nameof(location.Longitude)}:{location.Longitude} {nameof(location.Latitude)}{location.Latitude}";
                }
            //}
            */


            var locationCriteria = new Criteria();

            locationCriteria.Accuracy = Accuracy.Coarse;
            locationCriteria.PowerRequirement = Power.Medium;

            string provider = gpsManager.GetBestProvider(locationCriteria, true);
           
            gpsManager.RequestLocationUpdates(provider, 5000, 10, gpsListener);
            /*
            
            var allProvider = gpsManager.GetProviders(enabledOnly: false);
            
            foreach (var p in allProvider)
            {
                var isE =
                    gpsManager.IsProviderEnabled(p);
            }
            return;
            if (CrossGeolocator.Current.IsListening)
            {
                return;
            }

            CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 10);
            CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
            CrossGeolocator.Current.PositionError += Current_PositionError;*/
        }

        private void Current_PositionError(object sender, Plugin.Geolocator.Abstractions.PositionErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Error);
        }

        private void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            TextView label = FindViewById<TextView>(Resource.Id.textView1);
            
            label.Text = $"{nameof(e.Position.Longitude)}:{e.Position.Longitude} {nameof(e.Position.Latitude)}{e.Position.Latitude}";
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            // Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
