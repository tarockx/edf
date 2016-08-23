using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace xEDF.iOS
{
    public class TableSource_Suggestions : UITableViewSource, IUIGestureRecognizerDelegate
    {
        private UITableView myTable;

        List<string> suggestions = new List<string>();
        string CellIdentifier = "TableCell";

        public TableSource_Suggestions(UITableView table)
        {
            myTable = table;

            UILongPressGestureRecognizer lpgr = new UILongPressGestureRecognizer();
            lpgr.MinimumPressDuration = 1.3;
            lpgr.AddTarget((p) => { RowLongClicked(p as UILongPressGestureRecognizer); });
            myTable.AddGestureRecognizer(lpgr);
        }

        public void UpdateSuggestions(List<string> items)
        {
            suggestions.Clear();
            if (items != null)
                suggestions.AddRange(items);

            InvokeOnMainThread(delegate {
                myTable.ReloadData();
            });
        }
        
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return suggestions.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
            string item = suggestions[indexPath.Row];

            //---- if there are no cells to reuse, create a new one
            if (cell == null)
            { cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier); }

            cell.TextLabel.Text = item;

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            //base.RowSelected(tableView, indexPath);

            string item = suggestions[indexPath.Row].Trim();
            SuggestionSelected(this, new SuggestionSelectedEventArgs(item, false));
        }

        public void RowLongClicked(UILongPressGestureRecognizer gestureRecognizer)
        {
            CGPoint p = gestureRecognizer.LocationInView(myTable);

            NSIndexPath indexPath = myTable.IndexPathForRowAtPoint(p);
            if (indexPath != null && gestureRecognizer.State == UIGestureRecognizerState.Began)
            {
                SuggestionSelected(this, new SuggestionSelectedEventArgs(suggestions[indexPath.Row], true));
            }
        }


        public event SuggestionSelectedEventHandler SuggestionSelected;
        public delegate void SuggestionSelectedEventHandler(object sender, SuggestionSelectedEventArgs e);
        public class SuggestionSelectedEventArgs : EventArgs
        {
            public string Suggestion { get; set; }
            public bool LongClick { get; set; }

            public SuggestionSelectedEventArgs(string suggestion, bool longClick) { Suggestion = suggestion; LongClick = longClick; }
        }
    }
}
