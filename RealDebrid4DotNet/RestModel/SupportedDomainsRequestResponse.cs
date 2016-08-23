using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealDebrid4DotNet.RestModel
{
    public class SupportedDomainsRequestResponse
    {
        public SupportedDomainsRequestResponse(string jsonResponse)
        {
            JsonSerializer serializer = new JsonSerializer();
            List<string> list = JsonConvert.DeserializeObject<List<string>>(jsonResponse);

            SupportedHosters = list;
        }

        public List<string> SupportedHosters { get; private set; }
    }
}
