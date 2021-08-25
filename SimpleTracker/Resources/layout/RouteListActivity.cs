using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using V7 = Android.Support.V7.Widget;

namespace SimpleTracker.Resources.layout
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class RouteListActivity : AppCompatActivity
    {
        private Database.SimpleGpsDatabase database;
        private Adapters.RoutesAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.route_list);

            V7.Toolbar toolbar = FindViewById<V7.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            this.database = Database.SimpleGpsDatabase.Instance;
            List<Database.SimpleGpsRoute> routes = this.database.GetAllRoutes();
            this.adapter = new Adapters.RoutesAdapter(routes);

            adapter.NotifyItemRangeChanged(0, routes.Count);

            V7.RecyclerView routesList = FindViewById<V7.RecyclerView>(Resource.Id.routesListView);

            adapter.ItemClick += Adapter_ItemClick;
            V7.LinearLayoutManager layoutManager = new V7.LinearLayoutManager(this);
            routesList.SetLayoutManager(layoutManager);
            routesList.AddItemDecoration(new V7.DividerItemDecoration(routesList.Context, layoutManager.Orientation));
            routesList.SetAdapter(adapter);

            Button clearAllRoutes = FindViewById<Button>(Resource.Id.clearAllRoutesButton);
            clearAllRoutes.Click += ClearAllRoutes_Click;
        }

        private void Adapter_ItemClick(object sender, int e)
        {
            Toast.MakeText(this, $"RouteId {e}", ToastLength.Short).Show();

            var activity = new Intent(this, typeof(RouteDetailsActivity));
            StartActivity(activity);
        }

        private void ClearAllRoutes_Click(object sender, EventArgs e)
        {
            int removedRoutesCount = adapter.ItemCount;

            this.database.ClearAllRoutes();

            adapter.Routes = new List<Database.SimpleGpsRoute>();
            adapter.NotifyItemRangeRemoved(0, removedRoutesCount);
        }
    }
}