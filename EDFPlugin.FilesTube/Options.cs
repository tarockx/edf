using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlugin.FilesTube
{
    public enum FileExtension { 
        [Description("Tutti")]
        All,
        [Description("AVI")]
        AVI,
        [Description("Mp4")]
        MP4,
        [Description("MKV")]
        MKV}

    public class Options
    {
        private FileExtension _Extension;
        [Description("Filtra per estensione file")]
        public FileExtension Extension {
            get { return _Extension; }
            set { _Extension = value;}
        }
    }
}
