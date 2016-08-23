using Foundation;
using System.Collections.Generic;
using System;
using xEDFlib;
using RealDebrid4DotNet.RestModel;
using System.Xml.Serialization;
using System.IO;

namespace xEDF.iOS
{
    public class xSettings : IxEDFSettings
    {
        private XmlSerializer ser = new XmlSerializer(typeof(TokenRequestResponse));

        internal void EnableAllPlugins()
        {
            List<string> plugins = new List<string>();
            DisabledPlugins = plugins;
        }


        public TokenRequestResponse RDToken
        {
            get
            {
                string token = NSUserDefaults.StandardUserDefaults.StringForKey("RDToken");
                try
                {
                    return ser.Deserialize(new System.IO.StringReader(token)) as TokenRequestResponse;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    using (StringWriter textWriter = new StringWriter())
                    {
                        ser.Serialize(textWriter, value);
                        string serializedToken = textWriter.ToString();
                        NSUserDefaults.StandardUserDefaults.SetString(serializedToken, "RDToken");
                    }
                }
                else
                {
                    NSUserDefaults.StandardUserDefaults.SetString("", "RDToken");
                }

                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public List<string> DisabledPlugins
        {
            get
            {
                List<string> plugins = new List<string>();
                string[] ep = NSUserDefaults.StandardUserDefaults.StringArrayForKey("disabled_plugins");
                if (ep != null)
                    plugins.AddRange(ep);
                return plugins;
            }
            set
            {
                NSMutableArray arr = new NSMutableArray();
                foreach (var item in value)
                {
                    arr.Add(new NSString(item));
                }
                NSUserDefaults.StandardUserDefaults.SetValueForKey(arr, new NSString("disabled_plugins"));

                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public bool CheckForUpdates
        {
            get
            {
                if (!NSUserDefaults.StandardUserDefaults.ToDictionary().ContainsKey(new NSString("CheckForUpdates")))
                    return true;
                else
                    return NSUserDefaults.StandardUserDefaults.BoolForKey("CheckForUpdates");
            }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, "CheckForUpdates");
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }


    }
}
