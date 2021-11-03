using System;
using System.Collections.Generic;
using SimpleDatabase;

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
        private SimpleGpsDatabase database;
        private Adapters.RoutesAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.route_list);

            V7.Toolbar toolbar = FindViewById<V7.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            this.database = SimpleGpsDatabase.Instance;
            this.adapter = new Adapters.RoutesAdapter(new List<SimpleGpsRoute>());


            V7.RecyclerView routesList = FindViewById<V7.RecyclerView>(Resource.Id.routesListView);

            adapter.ItemClick += Adapter_ItemClick;
            V7.LinearLayoutManager layoutManager = new V7.LinearLayoutManager(this);
            routesList.SetLayoutManager(layoutManager);
            routesList.AddItemDecoration(new V7.DividerItemDecoration(routesList.Context, layoutManager.Orientation));
            routesList.SetAdapter(adapter);

            Button clearAllRoutes = FindViewById<Button>(Resource.Id.clearAllRoutesButton);
            clearAllRoutes.Click += ClearAllRoutes_Click;
        }

        protected override void OnResume()
        {
            base.OnResume();

            List<SimpleGpsRoute> routes = this.database.GetAllRoutes();
            this.adapter.Routes = routes;

            adapter.NotifyDataSetChanged();
        }

        private void Adapter_ItemClick(object sender, int id)
        {
            // Toast.MakeText(this, $"RouteId {id}", ToastLength.Short).Show();

            var activity = new Intent(this, typeof(RouteDetailsActivity));
            Bundle bundle = new Bundle();
            bundle.PutInt("id", id);
            activity.PutExtras(bundle);
            StartActivity(activity);
        }

        private void ClearAllRoutes_Click(object sender, EventArgs e)
        {
            int removedRoutesCount = adapter.ItemCount;

            this.database.ClearAllRoutes();

            adapter.Routes = new List<SimpleGpsRoute>();
            adapter.NotifyItemRangeRemoved(0, removedRoutesCount);
        }
    }
}