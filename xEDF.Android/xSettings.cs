using Android.Content;
using System.Collections.Generic;
using xEDFlib;
using RealDebrid4DotNet.RestModel;
using System;
using System.Xml.Serialization;
using System.IO;

namespace xEDF.Droid
{
    public class xSettings : IxEDFSettings
    {
        private Context Context;
        private XmlSerializer ser = new XmlSerializer(typeof(TokenRequestResponse));

        public xSettings(Context context)
        {
            Context = context;
        }

        internal void EnableAllPlugins()
        {
            List<string> plugins = new List<string>();
            DisabledPlugins = plugins;
        }

        public bool CheckForUpdates
        {
            get
            {
                ISharedPreferences sharedPreferences = Context.GetSharedPreferences("xEDF", FileCreationMode.MultiProcess);
                return sharedPreferences.GetBoolean("CheckForUpdates", true);
            }
            set
            {
                ISharedPreferences sharedPreferences = Context.GetSharedPreferences("xEDF", FileCreationMode.MultiProcess);
                sharedPreferences.Edit().PutBoolean("CheckForUpdates", value).Commit();
            }
        }

        

        public List<string> DisabledPlugins
        {
            get
            {
                List<string> plugins = new List<string>();
                ISharedPreferences sharedPreferences = Context.GetSharedPreferences("xEDF", FileCreationMode.MultiProcess);
                
                var dp = sharedPreferences.GetStringSet("disabled_plugins", null);
                if (dp != null)
                    plugins.AddRange(dp);
                return plugins;
            }
            set
            {
                ISharedPreferences sharedPreferences = Context.GetSharedPreferences("xEDF", FileCreationMode.MultiProcess);
                sharedPreferences.Edit().PutStringSet("disabled_plugins", value).Commit();
            }
        }

        public TokenRequestResponse RDToken
        {
            get
            {
                ISharedPreferences sharedPreferences = Context.GetSharedPreferences("xEDF", FileCreationMode.MultiProcess);
                string token = sharedPreferences.GetString("RDToken", "");
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
                ISharedPreferences sharedPreferences = Context.GetSharedPreferences("xEDF", FileCreationMode.MultiProcess);
                if (value != null)
                {
                    using (StringWriter textWriter = new StringWriter())
                    {
                        ser.Serialize(textWriter, value);
                        string serializedToken = textWriter.ToString();
                        
                        sharedPreferences.Edit().PutString("RDToken", serializedToken).Commit();
                    }                    
                }
                else
                {
                    sharedPreferences.Edit().PutString("RDToken", "").Commit();
                }
            }
        }
    }
}
