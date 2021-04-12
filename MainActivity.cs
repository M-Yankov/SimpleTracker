using System;
using System.Threading.Tasks;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

// using Xamarin.Essentials;

using Android.Locations;
using Android.Content;
using Android.Support.V4.App;
using Android;

namespace SimpleTracker
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private LocationManager gpsManager;
        private SimpleGpsLocationListener gpsListener;

        private const int GpsRequestCode = 100;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            gpsManager = Application.Context.GetSystemService(Context.LocationService) as LocationManager;
            gpsListener = new SimpleGpsLocationListener();
            gpsListener.ProviderDisabled += GpsListener_ProviderDisabled;

            Button trackButton = FindViewById<Button>(Resource.Id.trackButton);
            trackButton.Click += TrackButton_Click;

            Button stopButton = FindViewById<Button>(Resource.Id.stopTrackButton);
            stopButton.Enabled = false;
            stopButton.Click += StopButton_Click;
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

        private void StopButton_Click(object sender, EventArgs e)
        {
            gpsManager.RemoveUpdates(gpsListener);
            FindViewById<Button>(Resource.Id.trackButton).Enabled = true;
            FindViewById<Button>(Resource.Id.stopTrackButton).Enabled = false;

            FindViewById<TextView>(Resource.Id.textView1).Text += "\nStopped";

            var intent = new Intent(this, typeof(Services.GpsTrackerService));
            intent.SetAction("Stop");
            StopService(intent);
        }

        private void TrackButton_Click(object sender, EventArgs e)
        {
            FindViewById<Button>(Resource.Id.trackButton).Enabled = false;
            FindViewById<Button>(Resource.Id.stopTrackButton).Enabled = true;

            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessFineLocation }, GpsRequestCode);
        }

        private void Current_PositionChanged(object sender, PositionEventArgs e)
        {
            TextView label = FindViewById<TextView>(Resource.Id.textView1);
            
            label.Text += $"\n{nameof(e.Location.Longitude)}:{e.Location.Longitude} {nameof(e.Location.Latitude)}{e.Location.Latitude}";
            label.SetTextColor(Android.Graphics.Color.Black);
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

        public override void OnRequestPermissionsResult(
            int requestCode, 
            string[] permissions, 
            [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == GpsRequestCode)
            {
                bool arePermissionsForLocationGranted = 
                    permissions?.Contains(Manifest.Permission.AccessFineLocation) == true
                    && grantResults?.Contains(Permission.Granted) == true;
                
                if (arePermissionsForLocationGranted)
                {
                    gpsListener.PositionChanged += Current_PositionChanged;
                    gpsManager.RequestLocationUpdates(LocationManager.GpsProvider, 1000, 5, gpsListener);

                    var intent = new Intent(this, typeof(Services.GpsTrackerService));
                    intent.SetAction("Start");
                    StartService(intent);
                }
                else
                {
                    TextView text = FindViewById<TextView>(Resource.Id.textView1);
                    
                    text.Text = "Please provide GPS permissions.";
                    text.SetTextColor(Android.Graphics.Color.Red);

                    FindViewById<Button>(Resource.Id.trackButton).Enabled = true;
                    FindViewById<Button>(Resource.Id.stopTrackButton).Enabled = false;
                }

                //Permission res = CheckCallingOrSelfPermission(Manifest.Permission.AccessFineLocation);
                //if (res == Permission.Granted)
                //{
                //}
            }

            // Xamarin.Essentials
            // Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
