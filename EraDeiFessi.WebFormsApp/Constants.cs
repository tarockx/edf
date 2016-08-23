using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace EraDeiFessi.WebFormsApp
{
    class Constants
    {
        public static string PluginsPath { get { return AppDataPath + "\\plugins\\"; } }
        public static string SharedHistoryPath { get { return AppDataPath + "\\shared_history.xml"; } }
        public static string AppDataPath { get { return Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "App_Data"); } }

    }
}
