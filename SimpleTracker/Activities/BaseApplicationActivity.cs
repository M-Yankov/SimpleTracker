using Android.Content;
using Android.Graphics;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

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

        protected void EnableButton(int buttonResourceId)
        {
            SetButtonState(buttonResourceId, true);
        }

        protected void DisableButton(int buttonResourceId)
        {
            SetButtonState(buttonResourceId, false);
        }

        /// <summary>
        /// Special styles applied for Strava buttons.
        /// </summary>
        /// <param name="buttonResourceId"></param>
        protected void DisableStravaButton(int buttonResourceId)
        {
            DisableButton(buttonResourceId);

            // For newer androids
            // FindViewById<Button>(buttonResourceId).Background
            // .SetColorFilter(new BlendModeColorFilter(new Color(255, 180, 120), BlendMode.Multiply));
            
            FindViewById<Button>(buttonResourceId).Background.SetColorFilter(new Color(255, 180, 120), PorterDuff.Mode.Src);
            FindViewById<Button>(buttonResourceId).SetTextColor(Color.Gray);
        }

        private void SetButtonState(int buttonResourceId, bool enabled)
        {
            Button button = FindViewById<Button>(buttonResourceId);
            button.Enabled = enabled;
            button.Clickable = enabled;
        }
    }
}