using Foundation;
using libEraDeiFessi;
using System;
using System.Collections.Generic;
using UIKit;

namespace xEDF.iOS
{
    class TableSource_NestBlocks : UITableViewSource
    {
        private UITableView myTable;

        List<NestBlock> blocks;
        string CellIdentifier = "TableCell";

        public TableSource_NestBlocks(UITableView table, List<NestBlock> blocks)
        {
            this.blocks = blocks;
            myTable = table;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return blocks.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);

            NestBlock block = blocks[indexPath.Row];
            string title = block.Title;

            string subtitle = "";
            if (block is ContentNestBlock)
                subtitle = string.Format("{0} link disponibili", (block as ContentNestBlock).Links.Count);
            else
                subtitle = string.Format("{0} episodi/file", block.Children.Count.ToString());
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
            var block = blocks[indexPath.Row];
            if(block is ContentNestBlock)
                ContentNestBlockSelected(this, new ContentNestBlockSelectedEventArgs(block as ContentNestBlock));
            else
                NestBlockSelected(this, new NestBlockSelectedEventArgs(block));
        }

        public event NestBlockSelectedEventHandler NestBlockSelected;
        public delegate void NestBlockSelectedEventHandler(object sender, NestBlockSelectedEventArgs e);
        public class NestBlockSelectedEventArgs : EventArgs
        {
            public NestBlock NestBlock { get; set; }

            public NestBlockSelectedEventArgs(NestBlock block) { NestBlock = block; }
        }

        public event ContentNestBlockSelectedEventHandler ContentNestBlockSelected;
        public delegate void ContentNestBlockSelectedEventHandler(object sender, ContentNestBlockSelectedEventArgs e);
        public class ContentNestBlockSelectedEventArgs : EventArgs
        {
            public ContentNestBlock ContentNestBlock { get; set; }

            public ContentNestBlockSelectedEventArgs(ContentNestBlock block) { ContentNestBlock = block; }
        }
    }
}
