using RealDebrid4DotNet.RestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xEDFlib
{
    public interface IxEDFSettings
    {
        TokenRequestResponse RDToken { get; set; }
        List<string> DisabledPlugins { get; set; }
        bool CheckForUpdates { get; set; }
    }
}
