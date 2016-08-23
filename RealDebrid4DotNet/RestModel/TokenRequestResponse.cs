using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealDebrid4DotNet.RestModel
{
    [Serializable()]
    public class TokenRequestResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }

        //Non-response fields (added manually)
        public string client_id { get; set; }
        public string client_secret { get; set; }

        //Helpers
        public DateTime expirationDate { get; set; }
        public bool Valid { get {
                return access_token != null && refresh_token != null && client_id != null && client_secret != null;
            } }

        public TokenRequestResponse() { /* For serialization purposes */}

        public TokenRequestResponse(string jsonResponse)
        {
            JsonSerializer serializer = new JsonSerializer();
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

            access_token = dictionary["access_token"].ToString();
            expires_in = int.Parse(dictionary["expires_in"].ToString());
            expirationDate = DateTime.Now.AddSeconds(expires_in);
            refresh_token = dictionary["refresh_token"].ToString();
            token_type = dictionary["token_type"].ToString();
        }
    }
}
