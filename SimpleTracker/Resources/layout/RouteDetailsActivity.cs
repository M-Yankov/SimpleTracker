using Android.App;
using Android.OS;
using Android.Support.V7.App;

using V7 = Android.Support.V7.Widget;

namespace SimpleTracker.Resources.layout
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class RouteDetailsActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.route_details);

            V7.Toolbar toolbar = FindViewById<V7.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
        }
    }
}