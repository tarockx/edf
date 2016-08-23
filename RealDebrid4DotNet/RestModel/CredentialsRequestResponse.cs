using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealDebrid4DotNet.RestModel
{
    public class CredentialsRequestResponse
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }


        public CredentialsRequestResponse(string jsonResponse)
        {
            JsonSerializer serializer = new JsonSerializer();
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

            client_id = dictionary["client_id"].ToString();
            client_secret = dictionary["client_secret"].ToString();
        }
    }
}
