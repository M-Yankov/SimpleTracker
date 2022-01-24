using System;

using Android.App;
using Android.Content;
using Android.OS;

using V4 = Android.Support.V4.App;


namespace SimpleTracker.Dialogs
{
    public class ConfirmExitDialog : V4.DialogFragment 
    {
        private readonly Action exitApplicationCallback;

        public ConfirmExitDialog(Action exitApplicationCallback)
        {
            this.exitApplicationCallback = exitApplicationCallback;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState) =>
            new AlertDialog.Builder(this.Activity)
                   .SetMessage("Closing application will stop tracking.\nExit?")
                   .SetPositiveButton("Yes", this.OnButtonClick)
                   .SetNegativeButton("No", this.OnButtonClick)
                   .Create();

        private void OnButtonClick(object sender, DialogClickEventArgs args)
        {
            DialogButtonType buttonType = (DialogButtonType)args.Which;
            switch (buttonType)
            {
                case DialogButtonType.Positive:
                    exitApplicationCallback();
                    break;
                case DialogButtonType.Neutral:
                case DialogButtonType.Negative:
                default:
                    if (sender is AlertDialog dialog)
                    {
                        dialog.Dismiss();
                    }
                    break;
            }
        }
    }
}