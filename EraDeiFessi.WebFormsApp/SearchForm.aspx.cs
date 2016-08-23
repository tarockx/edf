using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using libEraDeiFessi.Plugins;
using libEraDeiFessi;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using EraDeiFessi.WebFormsApp.Helpers;

namespace EraDeiFessi.WebFormsApp
{
    public partial class SearchForm : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && !System.Web.UI.ScriptManager.GetCurrent(this.Page).IsInAsyncPostBack)
            {
                Repository.Startup();
                
            }

        }

        protected void btnPerformSearch_Click(object sender, EventArgs e)
        {
            repeaterPlugins.DataSource = null;
            repeaterPlugins.DataBind();
            repeaterPlugins.DataSource = PluginsRepo.Plugins.Values;
            repeaterPlugins.DataBind();
            

            //Search(txtSearchBox.Text.Trim());
            Search(txtAutoSearchBox.Value.Trim());

            System.Web.UI.ScriptManager.GetCurrent(this.Page).AddHistoryPoint("do", "search");
            ViewState["lastActionDone"] = "search"; //hack
        }

        protected void btnBookmark_Click(object sender, EventArgs e)
        {
            var lb = sender as LinkButton;
            var bminfo = lb.Parent.FindControl("spanBookmarkInfo") as HtmlGenericControl;
            string name = bminfo.InnerText.Split(";;;".ToArray(), StringSplitOptions.RemoveEmptyEntries).ElementAt(0);
            string url = bminfo.InnerText.Split(";;;".ToArray(), StringSplitOptions.RemoveEmptyEntries).ElementAt(1);
            string pluginID = bminfo.InnerText.Split(";;;".ToArray(), StringSplitOptions.RemoveEmptyEntries).ElementAt(2);

            Bookmark bm = new Bookmark(pluginID, name, url); //Hack orribile
            LoadContent(PluginsRepo.Plugins[pluginID] as IEDFContentProvider, bm);

            System.Web.UI.ScriptManager.GetCurrent(this.Page).AddHistoryPoint("lastActionDone", "loadContent");
            ViewState["lastActionDone"] = "loadContent"; //hack
        }

        protected void btnBackToSearchResults_Click(object sender, EventArgs e)
        {
            BackFromContentToSearchResults();
        }

        private void BackFromContentToSearchResults()
        {
            mainMultiView.SetActiveView(viewSearch);
            ViewState["lastActionDone"] = "search"; //hack
            ViewState["secondNavigation"] = true;
        }

        private void ClearSearchResults()
        {
            repeaterPlugins.DataSource = null;
            repeaterPlugins.DataBind();
            ViewState["lastActionDone"] = null;
            ViewState["secondNavigation"] = true;
        }

        private void Search(string query)
        {
            int count = 0;
            foreach (var plugin in PluginsRepo.Plugins.Values)
            {
                IEDFSearch searchplugin = plugin as IEDFSearch;
                if (plugin is IEDFSearch)
                {
                    PerformSearch(searchplugin, query);
                    count++;
                }
                
            }

        }

        private async void PerformSearch(IEDFSearch searchplugin, string searchterm)
        {
            //return;
            //var searchres = await ParsingHelpers.PerformSearchAsync(searchplugin, searchterm);
            var searchres = await ParsingHelpers.PerformSearchAsync(searchplugin, searchterm);
            string nextpage = null;
            IEnumerable<Bookmark> res = null;

            if (searchres != null)
            {
                res = searchres.Result;
                nextpage = searchres.NextPageUrl;
            }

            var resultsDiv = this.FindControlRecursive("pluginDiv" + (searchplugin as IEDFPlugin).pluginID.Replace(".", "")) as Control;
            var resultsRepeater = resultsDiv.FindControlsOfType<Repeater>().First();
            var resultsLabel = resultsDiv.FindControlRecursive("labelNoResults") as Label;
            var spanCounterBadge = resultsDiv.FindControlRecursive("spanCounterBadge") as HtmlGenericControl;


            if (res == null || res.Count() == 0)
            {
                resultsLabel.Visible = true;
                resultsRepeater.DataSource = null;
                resultsRepeater.DataBind();
                resultsRepeater.Visible = false;
                spanCounterBadge.InnerText = "0";
                spanCounterBadge.Attributes["class"] = spanCounterBadge.Attributes["class"] + " label-danger";
            }
            else
            {
                resultsLabel.Visible = false;
                resultsRepeater.DataSource = res;
                resultsRepeater.DataBind();
                resultsRepeater.Visible = true;
                spanCounterBadge.InnerText = res.Count().ToString();
                spanCounterBadge.Attributes["class"] = spanCounterBadge.Attributes["class"] + " label-success";
            }
        }

        private void LoadContent(IEDFContentProvider plugin, Bookmark bm)
        {
            var res = plugin.ParsePage(bm);
            if (!res.HasError)
            {
                switch (res.ResultType)
                {
                    case ParseResult.ParseResultType.HtmlContent:
                        LoadContent(res.Result as HtmlContent, bm.Name, bm.Url);
                        mainMultiView.SetActiveView(viewHtmlResult);
                        break;
                    case ParseResult.ParseResultType.NestedContent:
                        LoadContent(res.Result as NestedContent);
                        lblShowTitle.Text = bm.Name;
                        mainMultiView.SetActiveView(viewNestedResult);
                        break;
                }
            }
        }

        private void LoadContent(HtmlContent hc, string title, string referrer)
        {
            lblHtmlContentTitle.Text = title;
            //imgCover.Attributes["src"] = hc.CoverImageUrl;
            string content = hc.Content.Replace("target=\"_self\"", "target=\"_blank\"");
            Regex reg = new Regex("href=([\"'])(?:(?=(\\\\?))\\2.)*?\\1");
            var matches = reg.Matches(content);
            foreach (Match item in matches)
            {
                string link = item.Value.Replace("href=\"", "").TrimEnd("\"".ToArray());
                string bypassedlink = link;
                if (WebBypasser.IsLinkSupported(link))
                {
                    bypassedlink = WebBypasser.Bypass(link, referrer);
                }
                content = content.Replace(link, string.IsNullOrEmpty(bypassedlink) ? link : bypassedlink);
            }
            divHtmlContentLinks.InnerHtml = content;

            content = hc.Content;
            reg = new Regex( "href=([\"'])(?:(?=(\\\\?))\\2.)*?\\1");
            matches = reg.Matches(content);
            foreach (Match item in matches)
            {
                string link = item.Value.Replace("href=\"", "").TrimEnd("\"".ToArray());
                string encodedLink = Uri.EscapeDataString(link);
                content = content.Replace(link, encodedLink);
            }
            content = content.Replace("href=\"", "href=\"http://real-debrid.com/downloader?links=").Replace("target=\"_self\"", "target=\"_blank\"");
            divHtmlContentLinksRD.InnerHtml = content;
        }

        private void LoadContent(NestedContent nc)
        {
            var rep = viewNestedResult.FindControlRecursive("repeaterSeasons") as Repeater;
            rep.DataSource = nc.Children;
            rep.DataBind();
        }


        protected void pluginDiv_Load(object sender, EventArgs e)
        {
            var parent = (sender as Control).Parent as Control;
            parent.ID = "pluginDiv" + (sender as Label).Text.Replace(".", "");
        }

        protected void repeaterPlugins_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var div = e.Item.FindControlRecursive("pluginDiv");
            div.ID = "pluginDiv" + (e.Item.DataItem as IEDFPlugin).pluginID.Replace(".", "");
        }

        protected void repeaterSeasons_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var div = e.Item.FindControlRecursive("seasonDiv");
            div.ID = "seasonDiv" + (e.Item.ItemIndex +1).ToString();

            var nb = e.Item.DataItem as NestBlock;
            var spanCounterBadge = e.Item.FindControlRecursive("spanCounterBadge") as HtmlGenericControl;
        
            var lbl = e.Item.FindControlRecursive("labelNoResults") as Label;
            var rep = e.Item.FindControlRecursive("repeaterEpisodes") as Repeater;

            if (nb.Children == null || nb.Children.Count == 0)
            {
                rep.Visible = false;
                lbl.Visible = true;
                spanCounterBadge.InnerText = "0";
                spanCounterBadge.Attributes["class"] = spanCounterBadge.Attributes["class"] + " label-danger";
            }
            else
            {
                rep.DataSource = nb.Children;
                rep.DataBind();
                rep.Visible = true;
                lbl.Visible = false;
                spanCounterBadge.InnerText = nb.Children.Count.ToString();
                spanCounterBadge.Attributes["class"] = spanCounterBadge.Attributes["class"] + " label-success";
            }
            
        }

        protected void repeaterEpisodes_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem != null)
            {
                var ls = e.Item.FindControlRecursive("linksStore") as HtmlGenericControl;
                var cnb = e.Item.DataItem as ContentNestBlock;

                var rep = e.Item.FindControlRecursive("repeaterLinks") as Repeater;
                rep.DataSource = cnb.Links;
                rep.DataBind();
            }
        }

        protected void searchScriptManager_Navigate(object sender, HistoryEventArgs e)
        {
            bool? secondNavigation = ViewState["secondNavigation"] as bool?;
            if (secondNavigation.HasValue && secondNavigation.Value)
            {
                ViewState["secondNavigation"] = false;
                return;
            }


            //string d = e.State["do"];
            string d = ViewState["lastActionDone"] as string;
            if (d == "search")
            {
                ClearSearchResults();
            }
            else if (d == "loadContent")
            {
                BackFromContentToSearchResults(); //back to search
            }
        }

       
    }
}