using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace EraDeiFessi.WebFormsApp.Helpers
{
    public static class StreamExtensions
    {
        /*Returns a string representing the content of the body 
of the HTTP-request.*/
        public static string GetFromBodyString(this HttpRequest request)
        {
            string result = string.Empty;

            if (request == null || request.InputStream == null)
                return result;

            request.InputStream.Position = 0;

            /*create a new thread in the memory to save the original 
            source form as may be required to read many of the 
            body of the current HTTP- request*/
            using (MemoryStream memoryStream = new MemoryStream())
            {
                request.InputStream.CopyToMemoryStream(memoryStream);
                using (StreamReader streamReader = new StreamReader(memoryStream))
                {
                    result = streamReader.ReadToEnd();
                }
            }
            return result;
        }

        /*Copies bytes from the given stream MemoryStream and writes 
        them to another stream.*/
        public static void CopyToMemoryStream(this Stream source, MemoryStream destination)
        {
            if (source.CanSeek)
            {
                int pos = (int)destination.Position;
                int length = (int)(source.Length - source.Position) + pos;
                destination.SetLength(length);

                while (pos < length)
                    pos += source.Read(destination.GetBuffer(), pos, length - pos);
            }
            else
                source.CopyTo((Stream)destination);
        }
    }
}