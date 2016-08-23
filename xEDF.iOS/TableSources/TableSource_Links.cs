using Foundation;
using libEraDeiFessi;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace xEDF.iOS
{
    class TableSource_Links : UITableViewSource
    {

        List<Link> links;
        string CellIdentifier = "TableCell";

        public TableSource_Links(List<Link> links)
        {
            this.links = links;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return links.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);

            Link link = links[indexPath.Row];
            string title = DomainExtractor.GetDomainFromUrl(link.Url);

            string subtitle = link.Url;
            UIColor subtitleColor = UIColor.DarkGray;


            //---- if there are no cells to reuse, create a new one
            if (cell == null)
            { cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CellIdentifier); }

            cell.TextLabel.Text = title;
            cell.DetailTextLabel.Text = subtitle;
            cell.DetailTextLabel.TextColor = subtitleColor;

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            LinkSelected(this, new LinkSelectedEventArgs(links[indexPath.Row]));
        }

        public event LinkSelectedEventHandler LinkSelected;
        public delegate void LinkSelectedEventHandler(object sender, LinkSelectedEventArgs e);
        public class LinkSelectedEventArgs : EventArgs
        {
            public Link Link { get; set; }

            public LinkSelectedEventArgs(Link block) { Link = block; }
        }

    }
}
