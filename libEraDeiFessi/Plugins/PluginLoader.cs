using System;
using System.IO;
using System.Reflection;

namespace libEraDeiFessi.Plugins
{
    public static class PluginLoader
    {

        public static void LoadPlugins(string path)
        {
            try
            {
                foreach (string file in Directory.GetFiles(path, "*.dll"))
                {
                    if (!file.Contains("EDFPlugin"))
                        continue;

                    Assembly a = Assembly.LoadFrom(file);
                    foreach (Type t in a.GetTypes())
                    {
                        if (t.GetInterface("IEDFPlugin") != null)
                        {
                            try
                            {
                                IEDFPlugin pluginclass = Activator.CreateInstance(t) as IEDFPlugin;
                                PluginsRepo.Plugins.Add(pluginclass.pluginID, pluginclass);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
