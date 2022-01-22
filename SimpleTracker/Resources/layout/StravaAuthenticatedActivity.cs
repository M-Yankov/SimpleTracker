using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

using SimpleDatabase;

using SimpleTracker.Common;

using StravaIntegrator;
using StravaIntegrator.Models;

namespace SimpleTracker.Resources.layout
{
    [Activity(Label = "@string/app_name")]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataSchemes = new[] { "https", "http" },
        DataHosts = new[] { "simpletracker.com", "simpletracker.mihyan.com" },
        DataPathPrefixes = new[] { "/authorize-result", })]
    public class StravaAuthenticatedActivity : Activity
    {
        private SimpleGpsDatabase database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.strava_authenticated);

            string link = Intent.DataString;

            this.database = SimpleGpsDatabase.Instance;

            StravaAuthentication stravaAuthorization = new StravaAuthentication();
            TextView messageField = FindViewById<TextView>(Resource.Id.strava_authenticated_text);
            OneTimeUsableCode oneTimeCode = stravaAuthorization.TryGetAuthorizationCode(link, ApplicationSecrets.Strava.Scopes);
            if (!string.IsNullOrWhiteSpace(oneTimeCode.Error))
            {
                messageField.Text = oneTimeCode.Error;
                return;
            }

            AuthorizationTokens tokensInformaton = stravaAuthorization.GetAuthotizationsTokens(
                oneTimeCode.Value,
                ApplicationSecrets.Strava.ClientId,
                ApplicationSecrets.Strava.ClientSecret,
                StravaAuthentication.GrantTypeAuthorizationCode);

            if (!string.IsNullOrWhiteSpace(tokensInformaton.ErrorResponse))
            {
                messageField.Text = $"An error occurred:{System.Environment.NewLine}{tokensInformaton.ErrorResponse}";
                return;
            }

            SimpleGpsSettings settings = this.database.GetSettings();
            settings.StravaAccessToken =  Utilities.EncryptValue(tokensInformaton.AccessToken);
            settings.StravaAccessTokenExpirationDate = tokensInformaton.AccessTokenExpireDate;
            settings.StravaRefreshToken = Utilities.EncryptValue(tokensInformaton.RefreshToken);

            this.database.UpdateSettings(settings);
        }
    }
}