using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using SimpleTracker.Activities;
using SimpleTracker.Common;

namespace SimpleTracker.Resources.layout
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class SettingsActivity : BaseApplicationActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.settings);

            bool isStravaInstalled = Utilities.IsPackageInstalled(this, "com.strava");

            Button stravaButton = FindViewById<Button>(Resource.Id.strava_authorize_button);
            stravaButton.Enabled = isStravaInstalled;

            if (isStravaInstalled)
            {
                stravaButton.Click += AuthorizeStrava_Click;
            }
            else
            {
                FindViewById<TextView>(Resource.Id.strava_text_na).Visibility = ViewStates.Visible;
                stravaButton.SetBackgroundColor(Android.Graphics.Color.Gray);
            }
        }

        private void AuthorizeStrava_Click(object sender, EventArgs e)
        {
            var builder = new UriBuilder("https://www.strava.com/oauth/mobile/authorize");
            var queryParams = new Dictionary<string, string>()
            {
                { "client_id", ApplicationSecrets.Strava.ClientId },
                { "redirect_uri", "https://simpletracker.mihyan.com/authorize-result" },
                { "response_type", "code" },
                { "approval_prompt", "auto" },
                { "scope", ApplicationSecrets.Strava.Scopes },
            };

            builder.Query = string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"));

            var uri = Android.Net.Uri.Parse(builder.ToString());
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        }
    }
}