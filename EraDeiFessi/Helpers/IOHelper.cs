using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EraDeiFessi.Helpers
{
    public class IOHelper
    {
        public static bool ExtractResource(string resname, string outpath)
        {
            try
            {
                var restnames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                string res = string.Empty;
                foreach (var item in restnames)
                {
                    if (item.Contains(resname))
                        res = item;
                }
                using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream(res))
                {
                    using (Stream output = File.Create(outpath))
                    {
                        CopyStream(input, output);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return File.Exists(outpath);
        }

        private static void CopyStream(Stream input, Stream output)
        {
            // Insert null checking here for production
            byte[] buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}
