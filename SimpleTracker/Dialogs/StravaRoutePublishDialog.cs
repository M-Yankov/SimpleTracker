using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using V4 = Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using StravaIntegrator.Models;

namespace SimpleTracker.Dialogs
{
    public class StravaRoutePublishDialog : V4.DialogFragment
    {
        private readonly EventHandler<PublishActivity> onPublishClick;

        public StravaRoutePublishDialog(EventHandler<PublishActivity> onPublishClick = null)
        {
            this.onPublishClick = onPublishClick;
        }
        //public override Dialog OnCreateDialog(Bundle savedInstanceState)
        //{
        //    return new AlertDialog.Builder(Activity)
        //        .SetMessage("HelloWorld")
        //        .SetPositiveButton("Publish", (object sender, DialogClickEventArgs args) =>
        //        {

        //        })
        //        .Create();

        //}

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // will I have ID? 

            View view = inflater.Inflate(Resource.Layout.strava_route_publish_dialog, container, false);
            RadioGroup radioGroup = view.FindViewById<RadioGroup>(Resource.Id.strava_publish_route_type);

            string[] activityTypes = Resources
                .GetStringArray(Resource.Array.activity_types);

            for (int i = 0; i < activityTypes.Length; i++)
            {
                string activityType = activityTypes[i];
                RadioButton radioButton = new RadioButton(this.Context) { Id = 100 + (i + 1), Text = activityType };

                radioGroup.AddView(radioButton);
            }

            view.FindViewById<Button>(Resource.Id.strava_publish_route_cancel).Click += OnCancelClick;
            view.FindViewById<Button>(Resource.Id.strava_publish_route_confirm).Click += OnPublishClick;

            return view;
        }

        private void OnPublishClick(object sender, EventArgs e)
        {
            RadioGroup radioGroup = View.FindViewById<RadioGroup>(Resource.Id.strava_publish_route_type);

            string activityType = string.Empty;
            if (radioGroup.Selected)
            {
                activityType = ((RadioButton)radioGroup.GetChildAt(radioGroup.CheckedRadioButtonId)).Text;
            }

            string activityName = View.FindViewById<EditText>(Resource.Id.strava_publish_route_name).Text;
            string activityDescription = View.FindViewById<EditText>(Resource.Id.strava_publish_route_description).Text;
            bool activityMuted = View.FindViewById<CheckBox>(Resource.Id.strava_publish_route_mute).Checked;

            if (onPublishClick != null)
            {
                PublishActivity activity = new PublishActivity()
                {
                    Type = activityType,
                    Description = activityDescription,
                    Muted = activityMuted,
                    Name = activityName
                };

                onPublishClick(this, activity);
            }
        }

        private void OnCancelClick(object sender, EventArgs e) =>
            this.Dismiss();
    }
}