using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace libEraDeiFessi.Plugins
{
    public class PluginOptionsRepo : IXmlSerializable
    {
        private List<IEDFPluginOptions> pluginsWithOptions;

        public PluginOptionsRepo()
        {
            ReloadPlugins();
        }

        public void ReloadPlugins()
        {
            pluginsWithOptions = (from p in PluginsRepo.Plugins.Values where p is IEDFPluginOptions select p as IEDFPluginOptions).ToList();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                string pluginID = reader.GetAttribute("pluginID");
                reader.ReadStartElement("PluginOption");                

                try
                {
                    IEDFPluginOptions plugin = (from p in pluginsWithOptions where (p as IEDFPlugin).pluginID == pluginID select p).ElementAt(0);
                    XmlSerializer ser = new XmlSerializer(plugin.Options.GetType());
                    plugin.Options = ser.Deserialize(reader);
                }
                catch { }

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var item in pluginsWithOptions)
            {
                writer.WriteStartElement("PluginOption");
                writer.WriteAttributeString("pluginID", (item as IEDFPlugin).pluginID);

                XmlSerializer ser = new XmlSerializer(item.Options.GetType());
                ser.Serialize(writer, item.Options);

                writer.WriteEndElement();
            }

        }
    }
}
