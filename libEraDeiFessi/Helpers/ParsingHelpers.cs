using System.Threading.Tasks;
using libEraDeiFessi.Plugins;
using HtmlAgilityPack;
using System.IO;

namespace libEraDeiFessi
{
    public class ParsingHelpers
    {

        public static Task<ParseResult> ParsePageAsync(IEDFContentProvider obj, Bookmark bm)
        {
            var task = Task.Factory.StartNew(() => obj.ParsePage(bm));
            return task;
        }

        public static Task<SearchResult> GetBookmarkListAsync(IEDFList obj, string list, string nextPage = "")
        {
            var task = Task.Factory.StartNew(() => obj.GetList(list, nextPage));
            return task;
        }

        public static Task<SearchResult> PerformSearchAsync(IEDFSearch obj, string searchterm)
        {
            var task = Task.Factory.StartNew(() => obj.PerformSearch(searchterm));
            return task;
        }

        public static Task<SearchResult> GetResultPageAsync(IEDFSearch obj, string uri)
        {
            var task = Task.Factory.StartNew(() => obj.GetResultPage(uri));
            return task;
        }

        public static string ConvertHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }

        private static void ConvertTo(HtmlNode node, TextWriter outText)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }
                    break;
            }
        }
    }
}
