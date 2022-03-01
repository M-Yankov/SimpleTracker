using System;
using System.Collections.Generic;
using SimpleDatabase;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using V7 = Android.Support.V7.Widget;
using Android.Views;
using SimpleTracker.Activities;
using SimpleTracker.Common;

namespace SimpleTracker.Resources.layout
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class RouteListActivity : BaseApplicationActivity
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

            // Same as savedInstanceState?.GetBoolean
            bool isRecording = Intent.Extras.GetBoolean(SimpleConstants.ExtraNames.IsRecording);

            if (isRecording)
            {
                DisableButton(Resource.Id.clearAllRoutesButton);
            }
            else
            {
                Button clearAllRoutes = FindViewById<Button>(Resource.Id.clearAllRoutesButton);
                clearAllRoutes.Click += ClearAllRoutes_Click;
            }
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
            bool isRecording = Intent.Extras.GetBoolean(SimpleConstants.ExtraNames.IsRecording);

            var activity = new Intent(this, typeof(RouteDetailsActivity));
            Bundle bundle = new Bundle();
            bundle.PutInt("id", id);
            bundle.PutBoolean(SimpleConstants.ExtraNames.IsRecording, isRecording);
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