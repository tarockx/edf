using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace xEDF.iOS
{
    public abstract class ExpandableTableSource<T> : UITableViewSource
    {
        public IReadOnlyList<T> Items;
        protected readonly Action<T> TSelected;
        protected readonly string ParentCellIdentifier = "ParentCell";
        protected readonly string ChildCellIndentifier = "ChildCell";
        protected int currentExpandedIndex = -1;

        public ExpandableTableSource() { }

        public ExpandableTableSource(Action<T> TSelected)
        {
            this.TSelected = TSelected;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Items.Count + ((currentExpandedIndex > -1) ? 1 : 0);
        }


        void collapseSubItemsAtIndex(UITableView tableView, int index)
        {
            tableView.DeleteRows(new[] { NSIndexPath.FromRowSection(index + 1, 0) }, UITableViewRowAnimation.Fade);
        }

        void expandItemAtIndex(UITableView tableView, int index)
        {
            int insertPos = index + 1;
            tableView.InsertRows(new[] { NSIndexPath.FromRowSection(insertPos++, 0) }, UITableViewRowAnimation.Fade);
        }

        protected bool isChild(NSIndexPath indexPath)
        {
            return currentExpandedIndex > -1 &&
                   indexPath.Row > currentExpandedIndex &&
                   indexPath.Row <= currentExpandedIndex + 1;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (isChild(indexPath))
            {
                //Handle selection of child cell
                Console.WriteLine("You touched a child!");
                tableView.DeselectRow(indexPath, true);
                return;
            }
            tableView.BeginUpdates();
            if (currentExpandedIndex == indexPath.Row)
            {
                this.collapseSubItemsAtIndex(tableView, currentExpandedIndex);
                currentExpandedIndex = -1;
            }
            else
            {
                var shouldCollapse = currentExpandedIndex > -1;
                if (shouldCollapse)
                {
                    this.collapseSubItemsAtIndex(tableView, currentExpandedIndex);
                }
                currentExpandedIndex = (shouldCollapse && indexPath.Row > currentExpandedIndex) ? indexPath.Row - 1 : indexPath.Row;
                this.expandItemAtIndex(tableView, currentExpandedIndex);
            }
            tableView.EndUpdates();
            tableView.DeselectRow(indexPath, true);
        }

        //TODO: implement this here?
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            throw new NotImplementedException();
        }
    }
}
