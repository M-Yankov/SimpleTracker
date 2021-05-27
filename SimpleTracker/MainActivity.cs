using System;
using System.Collections.Generic;
using System.Linq;

using Android;
using Android.App;

using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using SimpleTracker.Resources.layout;

using V7 = Android.Support.V7.Widget;

namespace SimpleTracker
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const int GpsRequestCode = 100;
        private Connections.GpsTrackerServiceConnection connection;
        private Database.SimpleGpsDatabase database;
        private Adapters.RoutesAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            this.database = Database.SimpleGpsDatabase.Instance;

            this.adapter = new Adapters.RoutesAdapter(new List<Database.SimpleGpsRoute>());

            V7.RecyclerView routesList = FindViewById<V7.RecyclerView>(Resource.Id.routesListView);

            adapter.ItemClick += Adapter_ItemClick;
            routesList.SetLayoutManager(new V7.LinearLayoutManager(this));
            routesList.SetAdapter(adapter);

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

            Button clearAllRoutes = FindViewById<Button>(Resource.Id.clearAllRoutesButton);
            clearAllRoutes.Click += ClearAllRoutes_Click;

            var b = routesButton.Background;
            // new Android.Graphics.Drawables /
            // It's still null if application is closed from the system, but the service is running
            // Maybe I need to destroy the service in Ondestroy.
            if (this.connection == null)
            {
                this.connection = new Connections.GpsTrackerServiceConnection(this);
            }
        }

        private void ClearAllRoutes_Click(object sender, EventArgs e)
        {
            int removedRoutesCount = adapter.ItemCount;

            this.database.ClearAllRoutes();

            adapter.Routes = new List<Database.SimpleGpsRoute>();
            adapter.NotifyItemRangeRemoved(0, removedRoutesCount);
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (this.IsServiceConnected)
            {
                // Revise the code below: What information to show when user returns to main screen?
                // Connect to database 
                FindViewById<TextView>(Resource.Id.textView1).Text = $"Recording...";
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DisconnectService();
        }

        internal void ShowUpdates()
        {
            this.OnResume();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            FindViewById<Button>(Resource.Id.trackButton).Enabled = true;
            FindViewById<Button>(Resource.Id.stopTrackButton).Enabled = false;

            FindViewById<TextView>(Resource.Id.textView1).Text += "\nStopped";

            DisconnectService();
        }

        private void TrackButton_Click(object sender, EventArgs e)
        {
            FindViewById<Button>(Resource.Id.trackButton).Enabled = false;

            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessFineLocation }, GpsRequestCode);
        }

        private void ShowRoutes_Click(object sender, EventArgs e)
        {
            var routes = this.database.GetAllRoutes();
            adapter.Routes = routes;

            adapter.NotifyItemRangeChanged(0, routes.Count);
        }

        private void Adapter_ItemClick(object sender, int e)
        {
            Toast.MakeText(this, $"RouteId {e}", ToastLength.Short).Show();

            var activity = new Intent(this, typeof(RouteDetailsActivity));
            StartActivity(activity);
        }

        //public override bool OnCreateOptionsMenu(IMenu menu)
        //{
        //    MenuInflater.Inflate(Resource.Menu.menu_main, menu);
        //    return true;
        //}

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        //private void FabOnClick(object sender, EventArgs eventArgs)
        //{
        //    View view = (View)sender;
        //    Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
        //        .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        //}

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
                    FindViewById<Button>(Resource.Id.stopTrackButton).Enabled = true;

                    var intent = new Intent(this, typeof(Services.GpsTrackerService));
                    intent.SetAction("Start");
                    
                    BindService(intent, this.connection, Bind.AutoCreate);
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
