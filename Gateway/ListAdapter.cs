using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Gateway
{
    public class ListAdapter : BaseAdapter<Message>
    {
        Activity context;
        List<Message> list;

        public ListAdapter(Activity context, List<Message> list)
            : base()
        {
            this.context = context;
            this.list = list;
        }

        public override int Count
        {
            get { return list.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Message this[int index]
        {
            get { return list[index]; }
        }

        public void AddTop(Message m)
        {
            list.Insert(0, m);
            NotifyDataSetChanged();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;

            // re-use an existing view, if one is available
            // otherwise create a new one
            if (view == null)
                view = context.LayoutInflater.Inflate(Resource.Layout.ListItemRow, parent, false);

            Message item = this[position];

            var titleView = view.FindViewById<TextView>(Resource.Id.Title);
            var descView = view.FindViewById<TextView>(Resource.Id.Description);
            var statusView = view.FindViewById<TextView>(Resource.Id.Status);

            titleView.Text = item.Text;
            descView.Text = item.Sender + " (" + item.Date.ToString() + ")";
            statusView.Text = item.RequestStatusCode ?? "NULL";
            
            if (item.RequestStatusCode == "OK")
            {
                statusView.SetTextColor(new Android.Graphics.Color(38, 183, 33));
            }
            else {
                statusView.SetTextColor(new Android.Graphics.Color(170, 23, 20));
            }

            return view;
        }
    }
}