using Foundation;
using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace xEDF.iOS
{
    class TableSource_SearchResults : UITableViewSource
    {
        private UITableView myTable;

        Dictionary<IEDFPlugin, SearchResult> results;
        List<IEDFPlugin> sortedPlugins;
        string CellIdentifier = "TableCell";

        public TableSource_SearchResults(UITableView table, Dictionary<IEDFPlugin, SearchResult> results)
        {
            this.results = results;
            sortedPlugins = new List<IEDFPlugin>(results.Keys);
            sortedPlugins.Sort((x, y) => x.pluginName.CompareTo(y.pluginName));

            myTable = table;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return sortedPlugins.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
            string item = sortedPlugins[indexPath.Row].pluginName;
            SearchResult res = results[sortedPlugins[indexPath.Row]];

            string subtitle = "";
            UIColor subtitleColor = UIColor.Gray;

            if (res == null || res.HasError || res.Result == null || res.Result.Count == 0)
            {
                subtitle = "Nessun risultato trovato";
                subtitleColor = UIColor.Red;
            }
            else
            {
                subtitle = "Risultati trovati: " + res.Result.Count.ToString();
                subtitleColor = UIColor.Green;
            }

            //---- if there are no cells to reuse, create a new one
            if (cell == null)
            { cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CellIdentifier); }

            cell.TextLabel.Text = item;
            cell.DetailTextLabel.Text = subtitle;
            cell.DetailTextLabel.TextColor = subtitleColor;

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var plugin = sortedPlugins[indexPath.Row];
            SearchResultSelected(this, new SearchResultSelectedEventArgs(results[plugin], plugin));
        }

        public event SearchResultSelectedEventHandler SearchResultSelected;
        public delegate void SearchResultSelectedEventHandler(object sender, SearchResultSelectedEventArgs e);
        public class SearchResultSelectedEventArgs : EventArgs
        {
            public SearchResult SearchResult { get; set; }
            public IEDFPlugin Plugin { get; set; }

            public SearchResultSelectedEventArgs(SearchResult result, IEDFPlugin plugin) { SearchResult = result; Plugin = plugin;  }
        }
    }
}
