using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using libEraDeiFessi;
using System.Collections.ObjectModel;

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for BookmarkManager.xaml
    /// </summary>
    public partial class BookmarkManager : UserControl
    {
        public ObservableCollection<Bookmark> Bookmarks { get; set; }
        public bool BookmarkMode { get; set; }
        public string NextResultPage {get;set;}
        public IEDFSearch Plugin { get; set; }

        public BookmarkManager(IEnumerable<Bookmark> bookmarks, bool bookmarkmode)
        {
            BookmarkMode = bookmarkmode;

            Bookmarks = new ObservableCollection<Bookmark>();
            foreach (var item in bookmarks)
            {
                Bookmarks.Add(item);
            }
            InitializeComponent();
            lp2.Size = 16;
            
        }

        public void RecheckNextPageButtonVisibility() {
            if (string.IsNullOrEmpty(NextResultPage))
                btnLoadNextPage.Visibility = System.Windows.Visibility.Collapsed;
            else
                btnLoadNextPage.Visibility = System.Windows.Visibility.Visible;
        }

        public void ShowLoadingPanel()
        {
            panel1.Visibility = System.Windows.Visibility.Visible;
        }
        public void HideLoadingPanel()
        {
            panel1.Visibility = System.Windows.Visibility.Hidden;
        }

        public void ShowLoadingMorePanel()
        {
            lp2.Visibility = System.Windows.Visibility.Visible;
            btnLoadNextPage.Visibility = System.Windows.Visibility.Collapsed;
        }
        public void HideLoadingMorePanel()
        {
            lp2.Visibility = System.Windows.Visibility.Collapsed;
            btnLoadNextPage.Visibility = string.IsNullOrEmpty(NextResultPage) ?  System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        public void ShowBackgroundMessage(string message)
        {
            txtMessage.Text = message;
            txtMessage.Visibility = System.Windows.Visibility.Visible;
        }
        public void HideBackgroundMessage()
        {
            txtMessage.Text = "";
            txtMessage.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Bookmark b = (Bookmark)((Control)sender).DataContext;
            Repository.Repo.Settings.Bookmarks.Remove(b);
            Bookmarks.Remove(b);
            OnBookmarkDeleted(new BookmarkDeletedEventArgs(b));
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            Bookmark b = (Bookmark)((Control)sender).DataContext;
            OnBookmarkSelected(new BookmarkSelectedEventArgs(b));
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                Bookmark b = (Bookmark)((FrameworkElement)sender).DataContext;
                OnBookmarkSelected(new BookmarkSelectedEventArgs(b));
            }
        }

        private void btnLoadNextPage_Click(object sender, RoutedEventArgs e)
        {
            OnGetMoreResults(new GetMoreResultsEventArgs(this));
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


        protected virtual void OnBookmarkDeleted(BookmarkDeletedEventArgs e)
        {
            if (BookmarkDeleted != null)
                BookmarkDeleted(this, e);
        }
        public static event BookmarkDeletedEventHandler BookmarkDeleted;


        public class BookmarkDeletedEventArgs : EventArgs
        {
            public Bookmark Bookmark { get; set; }

            public BookmarkDeletedEventArgs(Bookmark bm) { Bookmark = bm; }
        }
        public delegate void BookmarkDeletedEventHandler(object sender, BookmarkDeletedEventArgs e);


        protected virtual void OnGetMoreResults(GetMoreResultsEventArgs e)
        {
            if (GetMoreResults != null)
                GetMoreResults(this, e);
        }
        public static event GetMoreResultsEventHandler GetMoreResults;


        public class GetMoreResultsEventArgs : EventArgs
        {
            public BookmarkManager BookmarkManager { get; set; }

            public GetMoreResultsEventArgs(BookmarkManager bm) { BookmarkManager = bm; }
        }
        public delegate void GetMoreResultsEventHandler(object sender, GetMoreResultsEventArgs e);


    }
}
