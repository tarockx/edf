using libEraDeiFessi.Plugins;
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.Specialized;

namespace EDFPlugin.WarezBB
{
    [Serializable]
    [XmlRoot(Namespace = "EDFPlugin.WarezBB")]
    [XmlInclude(typeof(Options))]
    public class Options
    {
        private string _username;
        private string _password;


        [Description("Username")]
        public string Username {
            get { return _username; }
            set { _username = value;}
        }

        [Description("Password")]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

    }
}
