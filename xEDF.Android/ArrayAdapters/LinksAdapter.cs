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
using System.Collections;
using libEraDeiFessi;

namespace xEDF.Droid.ArrayAdapters
{
    public class LinksAdapter : BaseAdapter
    {
        private Context context;
        private List<Link> links;

        public LinksAdapter(Context context, List<Link> links) : base()
        {
            this.context = context;
            this.links = links;
        }

        public override int Count
        {
            get
            {
                return links.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView; // re-use an existing view, if one is supplied
            if (view == null) // otherwise create a new one
                view = ((LayoutInflater)context.GetSystemService(Context.LayoutInflaterService)).Inflate(Android.Resource.Layout.SimpleListItem2, null);

            Link link = links[position];

            TextView txt1 = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            txt1.Text = DomainExtractor.GetDomainFromUrl(link.Url);
            txt1.SetMaxLines(1);

            TextView txt2 = view.FindViewById<TextView>(Android.Resource.Id.Text2);
            txt2.Text = link.Url;
            txt2.SetMaxLines(1);

            return view;
        }

    }

}