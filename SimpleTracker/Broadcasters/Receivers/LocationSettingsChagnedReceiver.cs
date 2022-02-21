using Android.Content;

namespace SimpleTracker.Broadcasters.Receivers
{
    public class LocationSettingsChagnedReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (context is MainActivity mainActivity)
            {
                mainActivity.CheckLocationProviderStatus();
            }
        }
    }
}