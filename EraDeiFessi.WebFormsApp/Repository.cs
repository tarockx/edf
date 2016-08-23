using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libEraDeiFessi.Plugins;
using System.IO;
using System.Xml.Serialization;

namespace EraDeiFessi.WebFormsApp
{
    static class Repository
    {
        public static bool WriteSharedHistory(SerializableDictionary<string, libEraDeiFessi.HistoryEntry> h)
        {
            try
            {
                XmlSerializer s = new XmlSerializer(typeof(SerializableDictionary<string, libEraDeiFessi.HistoryEntry>));
                FileStream fs = new FileStream(Constants.SharedHistoryPath, FileMode.Create);
                s.Serialize(fs, h);
                fs.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
             
        }

        public static SerializableDictionary<string, libEraDeiFessi.HistoryEntry> ReadSharedHistory()
        {
            try
            {
                XmlSerializer s = new XmlSerializer(typeof(SerializableDictionary<string, libEraDeiFessi.HistoryEntry>));
                FileStream fs = new FileStream(Constants.SharedHistoryPath, FileMode.Open);
                var r = s.Deserialize(fs) as SerializableDictionary<string, libEraDeiFessi.HistoryEntry>;
                fs.Close();
                return r;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void Startup()
        {
            PluginLoader.LoadPlugins(Constants.PluginsPath);
        }
    }
}
