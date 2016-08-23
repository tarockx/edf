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
using Java.Lang;

namespace xEDF.Droid.ArrayAdapters
{
    public class StringListArrayAdapter : BaseAdapter
    {
        private Context context;
        private List<string> items;

        public StringListArrayAdapter(Context context, List<string> values) : base()
        {
            this.context = context;
            this.items = values;
        }

        public override int Count
        {
            get
            {
                return items.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return items[position];
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView; // re-use an existing view, if one is supplied
            if (view == null) // otherwise create a new one
                view = ((LayoutInflater)context.GetSystemService(Context.LayoutInflaterService)).Inflate(Android.Resource.Layout.SimpleListItem1, null);
            
            view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = items[position];
            
            return view;
        }

    }

}