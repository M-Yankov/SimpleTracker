using System;
using System.Linq;

using Android;
using Android.App;

using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Widget;

using SimpleTracker.Activities;
using SimpleTracker.Broadcasters.Receivers;
using SimpleTracker.Common;
using SimpleTracker.Dialogs;
using SimpleTracker.Resources.layout;

using V7 = Android.Support.V7.Widget;

namespace SimpleTracker
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : BaseApplicationActivity
    {
        private const int GpsRequestCode = 100;
        private Connections.GpsTrackerServiceConnection connection;
        private LocationSettingsChagnedReceiver receiver;

        /// <summary>
        /// A temporary flag indicating whether gps service is starting.
        /// It requires more time and for that some UI changes may depend on that status.
        /// It is returned to false when the gps service is started.
        /// </summary>
        private bool isServiceConnecting = false;

        public override void OnBackPressed()
        {
            if (this.IsServiceConnected)
            {
                // base.OnBackPressed will call onDestroy() later.
                new ConfirmExitDialog(base.OnBackPressed)
                    .Show(SupportFragmentManager, typeof(ConfirmExitDialog).Name);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            // How to access the service here ?
            // I need to know this information in order to show some info on the screen and disable buttons.

            V7.Toolbar toolbar = FindViewById<V7.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            Button trackButton = FindViewById<Button>(Resource.Id.trackButton);
            trackButton.Click += TrackButton_Click;

            Button stopButton = FindViewById<Button>(Resource.Id.stopTrackButton);
            stopButton.Enabled = false;
            stopButton.Click += StopButton_Click;

            Button routesButton = FindViewById<Button>(Resource.Id.routesButton);
            routesButton.Click += ShowRoutes_Click;

            // It's still null if application is closed from the system, but the service is running
            // Maybe I need to destroy the service in Ondestroy.
            if (this.connection == null)
            {
                this.connection = new Connections.GpsTrackerServiceConnection(this);
            }

            this.receiver = new LocationSettingsChagnedReceiver();
        }

        protected override void OnResume()
        {
            base.OnResume();

            RegisterReceiver(receiver, new IntentFilter(LocationManager.ProvidersChangedAction));

            this.CheckLocationProviderStatus();
        }

        protected override void OnPause()
        {
            UnregisterReceiver(receiver);

            base.OnPause();
        }

        /// <summary>
        /// If the method is not implemented, the service cannot be disconnected anymore ...
        /// Need to check possibilities, where it goes and how it can be accessed if OnDestroy
        /// method is not implemented.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            DisconnectService();
        }

        internal void TrackingServiceConnected()
        {
            this.isServiceConnecting = false;
            this.CheckLocationProviderStatus();
        }

        /// <summary>
        /// Invoked when:
        /// 1. OnResume of main activity
        /// 2. GPS Status changed from settings while on main activity
        /// 3. When tracking is started
        /// </summary>
        internal void CheckLocationProviderStatus()
        {
            var gpsManager = (LocationManager)GetSystemService(LocationService);
            bool gpsEnabled = gpsManager.IsProviderEnabled(LocationManager.GpsProvider);

            if (this.IsServiceConnected)
            {
                // Revise the code below: What information to show when user returns to main screen?
                // Connect to database
                TextView textView = FindViewById<TextView>(Resource.Id.textView1);
                textView.SetTextColor(Color.DarkGray);

                textView.Text = gpsEnabled
                    ? "Recording..."
                    : "Enable GPS to continue recording!";
            }
            else if (!isServiceConnecting)
            {
                if (gpsEnabled)
                {
                    EnableButton(Resource.Id.trackButton);
                    string startText = FindViewById<TextView>(Resource.Id.trackButton).Text;
                    FindViewById<TextView>(Resource.Id.textView1).Text = $"Press \"{startText}\" to start GPS recording";
                }
                else
                {
                    FindViewById<TextView>(Resource.Id.textView1).Text = "Turn on GPS, to start tracking";
                    DisableButton(Resource.Id.trackButton);
                }
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            EnableButton(Resource.Id.trackButton);
            DisableButton(Resource.Id.stopTrackButton);

            FindViewById<TextView>(Resource.Id.textView1).Text += "\nStopped";

            DisconnectService();
        }

        private void TrackButton_Click(object sender, EventArgs e)
        {
            this.isServiceConnecting = true;

            DisableButton(Resource.Id.trackButton);

            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessFineLocation }, GpsRequestCode);
        }

        private void ShowRoutes_Click(object sender, EventArgs e)
        {
            var activity = new Intent(this, typeof(RouteListActivity));
            activity.PutExtra(SimpleConstants.ExtraNames.IsRecording, this.IsServiceConnected);
            StartActivity(activity);
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
                    EnableButton(Resource.Id.stopTrackButton);

                    var intent = new Intent(this, typeof(Services.GpsTrackerService));
                    intent.SetAction("Start");

                    BindService(intent, this.connection, Bind.AutoCreate);
                    StartService(intent);
                }
                else
                {
                    TextView text = FindViewById<TextView>(Resource.Id.textView1);

                    text.Text = "Please provide GPS permissions.";
                    text.SetTextColor(Color.Red);

                    EnableButton(Resource.Id.trackButton);
                    DisableButton(Resource.Id.stopTrackButton);
                }
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void DisconnectService()
        {
            var intent = new Intent(this, typeof(Services.GpsTrackerService));
            intent.SetAction("Stop");
            StopService(intent);

            if (this.IsServiceConnected)
            {
                UnbindService(this.connection);
            }
        }

        private bool IsServiceConnected => this.connection?.Binder?.Service?.IsStarted == true;
    }
}
