using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.Eurostreaming
{
    public static class Constants
    {
        public static string EurostreamingHomepageUrl { get { return "http://eurostreaming.tv/"; } }
        public static string EurostreamingListaSerieUrl { get { return "http://eurostreaming.tv/elenco-serie-tv/"; } }
        public static string EurostreamingListaFilmUrl { get { return "http://eurostreaming.tv/elenco-film-ita/"; } }
        public static string EurostreamingListaAnimeUrl { get { return "http://eurostreaming.tv/elenco-anime-cartoni/"; } }
        public static string EurostreamingSearchUrl { get { return "http://eurostreaming.tv/?s=$searchterm$"; } }

    }
}
