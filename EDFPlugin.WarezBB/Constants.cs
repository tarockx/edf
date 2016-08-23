using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.WarezBB
{
    public static class Constants
    {
        public static string WBB_BaseAddress { get { return "https://www.warez-bb.org"; } }
        public static string WBB_LoginUrl { get { return WBB_BaseAddress + "/login.php"; } }
        public static string WBB_SearchUrl { get { return WBB_BaseAddress + "/search.php"; } }
    }
}
