using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SimpleTracker.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleTracker.Binders;

namespace SimpleTracker.Connections
{
    public class GpsTrackerServiceConnection : Java.Lang.Object, IServiceConnection
    {
        private MainActivity mainActivity;

        public GpsTrackerServiceConnection(MainActivity mainActivity)
        {
            this.mainActivity = mainActivity;
            this.Binder = null;
            this.IsConnected = false;
        }

        public bool IsConnected { get; set; }
        public GpsTrackerServiceBinder Binder { get; set; }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            this.Binder = service as GpsTrackerServiceBinder;
            this.IsConnected = this.Binder != null;

            // if (this.IsConnected)
            // {
            //     this.mainActivity.Update ....();
            // }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            this.mainActivity = null;
            this.IsConnected = false;
            this.Binder = null;
        }
    }
}