using log4net;
using System;
using System.IO;
using System.Reflection;

namespace libEraDeiFessi.Plugins
{
    public static class PluginLoader
    {
        private static ILog Logger { get; set; } = LogManager.GetLogger(typeof(PluginLoader));

        public static void LoadPlugins(string path)
        {
            try
            {
                foreach (string file in Directory.GetFiles(path, "*.dll"))
                {
                    if (!file.Contains("EDFPlugin"))
                        continue;

                    Logger.Debug("Trying to load plugin assembly from file: " + file);
                    Assembly a = Assembly.LoadFrom(file);
                    foreach (Type t in a.GetTypes())
                    {
                        if (t.GetInterface("IEDFPlugin") != null)
                        {
                            try
                            {
                                Logger.Debug("EDF Plugin Interface found in type " + t.Name + ", trying to instantiate...");
                                IEDFPlugin pluginclass = Activator.CreateInstance(t) as IEDFPlugin;
                                PluginsRepo.Plugins.Add(pluginclass.pluginID, pluginclass);
                                Logger.Debug("Plugin " + t.Name + " instantiated succesfully!");
                            }
                            catch (Exception)
                            {
                                Logger.Debug("Plugin " + t.Name + " failed to instantiate!");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Exception in LoadPlugins(): " + ex.Message, ex);
                throw ex;
            }
        }
    }
}
