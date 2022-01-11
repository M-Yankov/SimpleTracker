namespace SimpleTracker.Common
{
    public class ApplicationSecrets
    {
        public class Strava
        {
            public const string ClientId = "";
            public const string ClientSecret = "";
            // As a scope I could add ",profile:read_all" - it will be used to get athlete's gear (shoes, bikes) and associate them
            // when publish an activity. But this causes more questions to the users.
            // Why SimpleTracker requires my private profile information ?!?
            // the activity could be associated with the default gear (shoes, bike), but the user could using other not default gear
            // that requires to edit it later the activity in Strava.
            // Almost in all cases the user will edit the activity in the Strava, title, description, stats, photos etc. It will be upon
            // the user to decide which gear was used.
            public const string Scopes = "activity:write";
        }

        public const string SecurityEncriptionValue = "";
    }
}