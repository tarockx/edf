using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEraDeiFessi
{
    public class LinkMetadata
    {
        public LinkMetadata()
        {
            Extension = Host = Date = "N/A";
            Parts = 1;
        }

        public string Extension { get; set; }
        public string Host { get; set; }
        public string Size { get; set; }
        public int Parts { get; set; }
        public string Date { get; set; }

        public bool IsVideo { get {
                return Extension.ToLower().Contains("avi") ||
                    Extension.ToLower().Contains("mkv") ||
                    Extension.ToLower().Contains("flv") ||
                    Extension.ToLower().Contains("mp4") ||
                    Extension.ToLower().Contains("ts") ||
                    Extension.ToLower().Contains("rm") ||
                    Extension.ToLower().Contains("mpg") ||
                    Extension.ToLower().Contains("mpeg") ||
                    Extension.ToLower().Contains("m4v") ||
                    Extension.ToLower().Contains("m4p") ||
                    Extension.ToLower().Contains("wmv") ||
                    Extension.ToLower().Contains("wmp") ||
                    Extension.ToLower().Contains("webm");
            } }
    }
}
