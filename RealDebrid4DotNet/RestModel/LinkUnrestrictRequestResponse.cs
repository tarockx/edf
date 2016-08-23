using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RealDebrid4DotNet.RestModel
{
    public class LinkUnrestrictRequestResponse
    {
        public class Alternative
        {
            public string id { get; set; }
            public string filename { get; set; }
            public string download { get; set; }
            public string quality { get; set; }
        }


        public string id { get; set; }
        public string filename { get; set; }
        public int filesize { get; set; }
        public string link { get; set; }
        public string host { get; set; }
        public int chunks { get; set; }
        public int crc { get; set; }
        public string download { get; set; }
        public int streamable { get; set; }
        public List<Alternative> alternatives { get; set; } = new List<Alternative>();

        //ERROR
        public bool has_error { get { return error_code != -999; } }
        public int error_code { get; set; } = -999;
        public string error_message { get; set; }

        //Helpers
        public string FormattedFilesize { get { return Helpers.FormatFilesize(filesize); } }

        public LinkUnrestrictRequestResponse(int errorCode, string errorMessage)
        {
            error_code = errorCode;
            error_message = errorMessage;
        }

        public LinkUnrestrictRequestResponse(string jsonResponse)
        {
            JsonSerializer serializer = new JsonSerializer();
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

            if (dictionary.ContainsKey("error"))
            {
                error_code = int.Parse(dictionary["error_code"].ToString());
                error_message = ErrorResponse.getErrorMessage(error_code);
            }
            else
            {
                id = dictionary["id"].ToString();
                filename = dictionary["filename"].ToString();
                filesize = int.Parse(dictionary["filesize"].ToString());
                link = dictionary["link"].ToString();
                host = dictionary["host"].ToString();
                chunks = int.Parse(dictionary["chunks"].ToString());
                crc = int.Parse(dictionary["crc"].ToString());
                download = dictionary["download"].ToString();
                streamable = int.Parse(dictionary["streamable"].ToString());

                if (dictionary.ContainsKey("alternative"))
                {
                    try
                    {
                        alternatives = JsonConvert.DeserializeObject<List<Alternative>>(dictionary["alternative"].ToString());
                    } catch(Exception e)
                    {
                        string msg = e.Message;
                    }
                }
            }
        }
    }
}