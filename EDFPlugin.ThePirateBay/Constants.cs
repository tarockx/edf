using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.ThePirateBay
{
    public static class Constants
    {
        public static string TPBSearchUrl { get { return "$proxy$/search/$searchterm$"; } }
        public static string ProxyBayUrl { get { return "https://proxybay.la/"; } }
        public static string TPBBaseUrl { get; set; }
    }
}
