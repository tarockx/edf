using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libRealDebrid4DotNet
{
    public class RDResponse
    {
        public string FileLink { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }

        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public bool HasError { get; set; }

        public RDResponse(string link, string name, string size) { FileLink = link; FileName = name; FileSize = size; HasError = false; }
        public RDResponse(string errMsg) { ErrorMessage = errMsg; HasError = true; }
        public RDResponse(string errMsg, string stackTrace) { ErrorMessage = errMsg; StackTrace = stackTrace; HasError = true; }

    }
}
