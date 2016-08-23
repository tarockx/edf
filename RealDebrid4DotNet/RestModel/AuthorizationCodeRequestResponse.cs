using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealDebrid4DotNet.RestModel
{
    public class AuthorizationCodeRequestResponse
    {
        public string device_code { get; set; }
        public string user_code { get; set; }
        public int interval { get; set; }
        public int expires_in { get; set; }
        public string verification_url { get; set; }

        //Helpers
        public DateTime AuthorizationWindow { get; set; }

        public AuthorizationCodeRequestResponse(string jsonResponse)
        {
            JsonSerializer serializer = new JsonSerializer();
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

            device_code = dictionary["device_code"].ToString();
            user_code = dictionary["user_code"].ToString();
            interval = int.Parse(dictionary["interval"].ToString());
            expires_in = int.Parse(dictionary["expires_in"].ToString());
            AuthorizationWindow = DateTime.Now.AddSeconds(expires_in);
            verification_url = dictionary["verification_url"].ToString();
        }
    }
}
