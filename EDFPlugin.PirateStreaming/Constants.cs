using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.PirateStreaming
{
    public static class Constants
    {
        public static string PirateStreamingHomepageUrl { get { return "http://www.piratestreaming.net/"; } }
        public static string PirateStreamingSearchUrl { get { return "http://www.piratestreaming.net/cerca.php?all=$searchterm$"; } }
        public static string PirateStreamingSearchUrlBase { get { return "http://www.piratestreaming.net/cerca.php"; } }

        private static Dictionary<string, string> _liste;
        public static Dictionary<string, string> Liste { get { return _liste; } }

        static Constants()
        {
            _liste = new Dictionary<string, string>(){
                {"0-9", "http://www.piratestreaming.net/categoria/serietv/0-9.html"},
                {"A-F", "http://www.piratestreaming.net/categoria/serietv/a-f.html"},
                {"G-L", "http://www.piratestreaming.net/categoria/serietv/g-l.html"},
                {"M-R", "http://www.piratestreaming.net/categoria/serietv/m-r.html"},
                {"S-Z", "http://www.piratestreaming.net/categoria/serietv/s-z.html"},
                {"Ultime novità", "http://www.piratestreaming.net/serietv-aggiornamenti.php"}
            };
        }
    }
}
