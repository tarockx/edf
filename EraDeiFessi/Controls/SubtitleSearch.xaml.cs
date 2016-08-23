using EraDeiFessi.Helpers;
//using SubtitleDownloader.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

namespace EraDeiFessi.Controls
{
    /// <summary>
    /// Interaction logic for SubtitleSearch.xaml
    /// </summary>
    public partial class SubtitleSearch : UserControl
    {
    //    public ObservableCollection<ISubtitleDownloaderWrapper> Downloaders { get; set; }
    //    private int _RunningSearches;
    //    public int RunningSearches
    //    {
    //        get
    //        {
    //            return _RunningSearches;
    //        }
    //        set
    //        {
    //            _RunningSearches = value;
    //            stackSearchBar.IsEnabled = (_RunningSearches == 0);
    //        }
    //    }


    //    public SubtitleSearch()
    //    {
    //        Downloaders = new ObservableCollection<ISubtitleDownloaderWrapper>();

    //        Downloaders.Add(new ISubtitleDownloaderWrapper(new SubtitleDownloader.Implementations.Subscene.SubsceneDownloader(), this));
    //        Downloaders.Add(new ISubtitleDownloaderWrapper(new SubtitleDownloader.Implementations.OpenSubtitles.OpenSubtitlesDownloader(), this));

    //        InitializeComponent();
    //    }


    //    public void SearchSubtitles(string title)
    //    {
    //        List<string> langs = new List<string>();
    //        if (chkEnglish.IsChecked.Value)
    //            langs.Add(Languages.GetLanguageCode("English"));
    //        if (chkItalian.IsChecked.Value)
    //            langs.Add(Languages.GetLanguageCode("Italian"));

    //        foreach (var item in Downloaders)
    //        {
    //            item.SearchSubtitlesAsync(title, langs.ToArray());
    //        }
    //    }

    //    private void PerformSearch()
    //    {
    //        string q = actb.SelectedItem as string;
    //        if (!string.IsNullOrWhiteSpace(q))
    //            SearchSubtitles(q);
    //        else if(!string.IsNullOrWhiteSpace(actb.Text))
    //            SearchSubtitles(actb.Text);
    //    }

    //    private void actb_KeyDown(object sender, KeyEventArgs e)
    //    {
    //        if ((e.Key == Key.Enter || e.Key == Key.Return) && actb.SelectedItem != null)
    //        {
    //            PerformSearch();
    //        }
    //    }

    //    private void actb_TextChanged_2(string newText)
    //    {
    //        if (string.IsNullOrWhiteSpace(newText))
    //            buttonSearch.IsEnabled = false;
    //        else
    //            buttonSearch.IsEnabled = true;
    //    }

    //    private void buttonSearch_Click(object sender, RoutedEventArgs e)
    //    {
    //        PerformSearch();
    //    }

    //    private void btnDownload_Click(object sender, RoutedEventArgs e)
    //    {
    //        ((sender as Button).DataContext as SubtitleWrapper).DownloadSubtitleAsync();
    //    }

    //    private void StackPanel_MouseMove(object sender, MouseEventArgs e)
    //    {
    //        StackPanel s = sender as StackPanel;
    //        SubtitleFileWrapper sw = s.DataContext as SubtitleFileWrapper;

    //        if (s != null && e.LeftButton == MouseButtonState.Pressed)
    //        {
    //            var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EraDeiFessi\\subtitles\\";
    //            if (!Directory.Exists(dir))
    //                Directory.CreateDirectory(dir);

    //            File.Copy(sw.file.FullName, dir + sw.file.Name, true);

    //            var dataObject = new DataObject(DataFormats.FileDrop, new string[] { dir + sw.file.Name });
    //            DragDrop.DoDragDrop(s, dataObject, DragDropEffects.Copy);
    //        }
    //    }

    //    private void LoadingPanel_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        (sender as LoadingPanel).Size = 20;
    //    }
    //}

    //public class ISubtitleDownloaderWrapper
    //{
    //    private SubtitleSearch Parent;
    //    public ISubtitleDownloader downloader { get; set; }
    //    public SearchTabHeader Header { get; set; }
    //    public ObservableCollection<SubtitleWrapper> Results { get; set; }

    //    public ISubtitleDownloaderWrapper(ISubtitleDownloader d, SubtitleSearch p) { Parent = p; downloader = d; Header = new SearchTabHeader(Name); Results = new ObservableCollection<SubtitleWrapper>(); Header.ShowSearchImage = false; }

    //    public string Name { get { return downloader == null ? "N/A" : downloader.GetName(); } }

    //    public async void SearchSubtitlesAsync(string title, string[] langs)
    //    {
    //        Parent.RunningSearches += 1;
    //        Header.ShowSearchImage = true;
    //        Results.Clear();

    //        var task = Task.Factory.StartNew(() => SearchSubtitles(title, langs));
    //        await task;

    //        if (task.Result != null)
    //        {
    //            foreach (var item in task.Result)
    //            {
    //                Results.Add(new SubtitleWrapper(item, this));
    //            }
    //            Header.SetCounter(task.Result.Count);
    //        }
    //        else
    //        {
    //            Header.SetCounter(0);
    //        }

    //        Header.ShowSearchImage = false;
    //        Parent.RunningSearches -= 1;
    //    }

    //    public List<Subtitle> SearchSubtitles(string title, string[] langs)
    //    {
    //        try
    //        {
    //            SubtitleDownloader.Core.SearchQuery query = new SearchQuery(title.Trim());
    //            query.LanguageCodes = langs;
    //            List<Subtitle> results = downloader.SearchSubtitles(query);
    //            return results;
    //        }
    //        catch (Exception)
    //        {
    //            return null;
    //        }            
    //    }


    //}

    //public class SubtitleWrapper : INotifyPropertyChanged
    //{
    //    private bool _ShowDownloadButton = true, _Downloading = false, _IsExpanded = true;

    //    public bool ShowDownloadButton{
    //        get{
    //            return _ShowDownloadButton;
    //        }
    //        set{
    //            _ShowDownloadButton = value;
    //            PropertyChanged(this, new PropertyChangedEventArgs("ShowDownloadButton"));
    //        }
    //    }
    //    public bool IsExpanded
    //    {
    //        get
    //        {
    //            return _IsExpanded;
    //        }
    //    }
    //    public bool Downloading { get { return _Downloading; } set { _Downloading = value; PropertyChanged(this, new PropertyChangedEventArgs("Downloading")); } }
    //    public ObservableCollection<SubtitleFileWrapper> DownloadedFiles { get; set; }

    //    Subtitle Sub;
    //    ISubtitleDownloaderWrapper Parent;

    //    public string FileName { get { return Sub == null ? "N/A" : Sub.FileName; } }

    //    public SubtitleWrapper(Subtitle s, ISubtitleDownloaderWrapper p) { DownloadedFiles = new ObservableCollection<SubtitleFileWrapper>(); Sub = s; Parent = p; }

    //    public async void DownloadSubtitleAsync()
    //    {
    //        ShowDownloadButton = false;
    //        Downloading = true;

    //        var task = Task.Factory.StartNew(() => DownloadSubtitle());
    //        await task;

    //        var subtitleFiles = task.Result;
    //        if (subtitleFiles != null)
    //        {
    //            foreach (var item in subtitleFiles)
    //            {
    //                DownloadedFiles.Add(new SubtitleFileWrapper(item, this));
    //            }
    //        }

    //        Downloading = false;            
    //    }

    //    public List<FileInfo> DownloadSubtitle()
    //    {
    //        List<FileInfo> subtitleFiles = Parent.downloader.SaveSubtitle(Sub);
    //        return subtitleFiles;
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;
    //}

    //public class SubtitleFileWrapper
    //{
    //    public FileInfo file { get; set; }
    //    SubtitleWrapper Parent;

    //    public SubtitleFileWrapper(FileInfo f, SubtitleWrapper p) { file = f; Parent = p; }

    //    public string FileName { get { return file.Name; } }
    }
}
