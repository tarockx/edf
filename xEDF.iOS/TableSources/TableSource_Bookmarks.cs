using Foundation;
using libEraDeiFessi;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace xEDF.iOS
{
    class TableSource_Bookmarks : UITableViewSource
    {
        private UITableView myTable;

        List<Bookmark> bookmarks;
        string CellIdentifier = "TableCell";

        public TableSource_Bookmarks(UITableView table)
        {
            this.bookmarks = new List<Bookmark>();
            myTable = table;
        }

        public void AddBookmarks(IEnumerable<Bookmark> newBookmarks)
        {
            bookmarks.AddRange(newBookmarks);
            InvokeOnMainThread(delegate {
                myTable.ReloadData();
            });
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return bookmarks.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);

            Bookmark bookmark = bookmarks[indexPath.Row];
            string title = bookmark.Name;

            string subtitle = bookmark.Subtitle;
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
            BookmarkSelected(this, new BookmarkSelectedEventArgs(bookmarks[indexPath.Row]));
        }

        public event BookmarkSelectedEventHandler BookmarkSelected;
        public delegate void BookmarkSelectedEventHandler(object sender, BookmarkSelectedEventArgs e);
        public class BookmarkSelectedEventArgs : EventArgs
        {
            public Bookmark Bookmark { get; set; }

            public BookmarkSelectedEventArgs(Bookmark bookmark) { Bookmark = bookmark; }
        }
    }
}
