using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.Cineblog01
{
    public static class Constants
    {
        public static string Cineblog01ListaFilmUrl { get { return "http://www.cb01.me/lista-film-completa-download/"; } }
        public static string Cineblog01ListaSerieUrl { get { return "http://www.cb01.me/serietv/lista-alfabetica-completa-serietv/"; } }
        public static string Cineblog01SearchUrl { get { return "http://www.cb01.co/?s=$searchterm$"; } }
        public static string Cineblog01CartoonSearchUrl { get { return "http://www.cineblog01.co/anime/?s=$searchterm$"; } }
        public static string Cineblog01SeriesSearchUrl { get { return "http://www.cb01.co/serietv/?s=$searchterm$"; } }
        public static string Cineblog01HomepageUrl { get { return "http://www.cb01.me/"; } }        
    }
}
