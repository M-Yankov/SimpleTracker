using Android.Content;
using Android.Support.V7.App;
using Android.Views;

using SimpleTracker.Resources.layout;

namespace SimpleTracker.Activities
{
    public class BaseApplicationActivity : AppCompatActivity
    {
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
                var activity = new Intent(this, typeof(SettingsActivity));
                StartActivity(activity);
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}