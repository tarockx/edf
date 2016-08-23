namespace libEraDeiFessi
{
    public class DomainExtractor
    {
        static public string GetDomainFromUrl(string Url)
        {
            if (Url.StartsWith("http://"))
                Url = Url.Replace("http://", "");

            if (Url.StartsWith("https://"))
                Url = Url.Replace("https://", "");

            if (Url.StartsWith("www."))
                Url = Url.Replace("www.", "");

            if (Url.Contains("/"))
                return Url.Substring(0, Url.IndexOf("/"));

            return "N/A";
        }

        static public string GetLastPathPieceFromUrl(string Url)
        {
            try
            {
                return Url.Substring(Url.LastIndexOf("/") + 1);
            }
            catch
            {
                return "N/A";
            }
        }

    }
}