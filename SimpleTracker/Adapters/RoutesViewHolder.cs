using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using SimpleDatabase;

using System;

namespace SimpleTracker.Adapters
{
    public class RoutesViewHolder : RecyclerView.ViewHolder
    {
        private readonly TextView routeIdElement;
        private readonly TextView routeNameElement;

        public RoutesViewHolder(View listView, EventHandler<int> handler)
            : base(listView)
        {
            this.routeIdElement = listView.FindViewById<TextView>(Resource.Id.routeIdTextView);
            this.routeNameElement = listView.FindViewById<TextView>(Resource.Id.routeNameTextView);

            listView.Click += (sender, e) =>
            {
                int routeId = int.Parse(this.routeIdElement.Text);
                handler.Invoke(listView, routeId);
            };
        }

        public void SetData(SimpleGpsRoute route)
        {
            this.routeIdElement.Text = $"{route.Id}";
            this.routeNameElement.Text = route.Name;
        }
    }
}
