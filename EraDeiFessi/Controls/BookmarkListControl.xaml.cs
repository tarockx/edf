using EraDeiFessi.Helpers;
using EraDeiFessi.Repository;
using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for MovieListDialog.xaml
    /// </summary>
    public partial class BookmarkListControl : UserControl
    {
        ObservableCollection<Bookmark> Bookmarks = new ObservableCollection<Bookmark>();
        public ObservableCollection<IEDFList> Plugins { get; set; }
        public ICollectionView BookmarksView { get; set; }


        public Bookmark SelectedMovie
        {
            get
            {
                return (Bookmark)listShows.SelectedItem;
            }
        }


        public BookmarkListControl(IEnumerable<IEDFList> plugins)
        {
            Plugins = new ObservableCollection<IEDFList>();
            foreach (var plugin in plugins)
            {
                Plugins.Add(plugin);
            }

            BookmarksView = CollectionViewSource.GetDefaultView(Bookmarks);
            InitializeComponent();

            BookmarksView.Filter = CustomerFilter;
        }

        async private void LoadList(IEDFList plugin, string list, string nextPage)
        {
            btnBack.IsEnabled = false;
            btnMore.IsEnabled = false;

            lp1.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(nextPage))
                Bookmarks.Clear();

            var liveitems = await ParsingHelpers.GetBookmarkListAsync(plugin, list, nextPage);
            if (liveitems != null)
            {
                foreach (var item in liveitems.Result)
                    Bookmarks.Add(item);

                curNextPage = liveitems.NextPageUrl;
                curPlugin = plugin;
                curList = list;
            }
            else
            {
                curNextPage = string.Empty;
                curPlugin = null;
                curList = string.Empty;
            }

            btnMore.IsEnabled = !string.IsNullOrEmpty(curNextPage);

            lp1.Visibility = System.Windows.Visibility.Hidden;
            btnBack.IsEnabled = true;
        }

        private void btnPlugin_Click(object sender, RoutedEventArgs e)
        {
            listPlugins.Visibility = System.Windows.Visibility.Hidden;
            gridBookmarks.Visibility = System.Windows.Visibility.Visible;

            Button b = sender as Button;
            ItemsControl bparent = VisualTreeHelpers.FindAncestor<ItemsControl>(b); 

            LoadList(bparent.DataContext as IEDFList, b.Tag as string, string.Empty);
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            BookmarksView.Refresh();
        }

        private bool CustomerFilter(object item)
        {
            Bookmark show = item as Bookmark;
            string filter = txtFilter.Text;
            return string.IsNullOrWhiteSpace(filter) || filter.Length <= 1 || show.Name.ToLower().Contains(filter.Trim().ToLower());
        }

        private void movie_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                OnBookmarkSelected(new BookmarkSelectedEventArgs(listShows.SelectedItem as Bookmark));
            }
        }

        private void listShows_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnGo.IsEnabled = (listShows.SelectedItem != null);
        }

        protected virtual void OnBookmarkSelected(BookmarkSelectedEventArgs e)
        {
            if (BookmarkSelected != null)
                BookmarkSelected(this, e);
        }
        public static event BookmarkSelectedEventHandler BookmarkSelected;


        public class BookmarkSelectedEventArgs : EventArgs
        {
            public Bookmark Bookmark { get; set; }

            public BookmarkSelectedEventArgs(Bookmark bm) { Bookmark = bm; }
        }
        public delegate void BookmarkSelectedEventHandler(object sender, BookmarkSelectedEventArgs e);

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            OnBookmarkSelected(new BookmarkSelectedEventArgs(listShows.SelectedItem as Bookmark));
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            listPlugins.Visibility = System.Windows.Visibility.Visible;
            gridBookmarks.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnMore_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(curNextPage)  && !string.IsNullOrEmpty(curList) && curPlugin != null)
                LoadList(curPlugin, curList, curNextPage);
        }
        private string curNextPage = string.Empty;
        private string curList = string.Empty;
        private IEDFList curPlugin = null;

    }
}
