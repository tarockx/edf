using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libEraDeiFessi.Plugins;

namespace libEraDeiFessi.Plugins
{
    public static class PluginsRepo
    {
        public static Dictionary<string, IEDFPlugin> Plugins { get; set; }

        static PluginsRepo() { Plugins = new Dictionary<string, IEDFPlugin>(); }
    }
}
