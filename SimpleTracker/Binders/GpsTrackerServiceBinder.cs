using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SimpleTracker.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTracker.Binders
{
    public class GpsTrackerServiceBinder : Binder
    {
        public GpsTrackerServiceBinder(GpsTrackerService service)
        {
            this.Service = service;
        }

        public GpsTrackerService Service { get; set; }
    }
}