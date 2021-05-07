using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTracker.Adapters
{
    public class RoutesAdapter : RecyclerView.Adapter
    {
        public RoutesAdapter(IList<Database.SimpleGpsRoute> routes)
        {
            this.Routes = routes;
        }

        public IList<Database.SimpleGpsRoute> Routes { get; set; }

        public event EventHandler<int> ItemClick;

        public override int ItemCount => this.Routes.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is RoutesViewHolder routesViewHolder
                && this.Routes.Count > position)
            {
                routesViewHolder.SetData(this.Routes[position]);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.route_partial, parent, false);

            return new RoutesViewHolder(view, ItemClick);
        }
    }
}
