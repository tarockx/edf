﻿using System;

using EDFPlugin.AllUC;
//using EDFPlugin.Cineblog01;
using EDFPlugin.Eurostreaming;
using EDFPlugin.Filespr;
using EDFPlugin.IlCorsaroNero;
using EDFPlugin.ItaliaSerie;
using EDFPlugin.PirateStreaming;
using EDFPlugin.ShareDir;
//using EDFPlugin.WarezBB;

using System.Collections.Generic;
using libEraDeiFessi.Plugins;

namespace xEDF
{
    public class PluginRepo
    {
        public static Dictionary<string, IEDFPlugin> Plugins;
        static PluginRepo()
        {
            Plugins = new Dictionary<string, IEDFPlugin>();

            AllUCDownloadPlugin allucDownloadPlugin = new AllUCDownloadPlugin();
            Plugins.Add(allucDownloadPlugin.pluginID, allucDownloadPlugin);
            AllUCStreamPlugin allucStreamPlugin = new AllUCStreamPlugin();
            Plugins.Add(allucStreamPlugin.pluginID, allucStreamPlugin);

            //CBMoviePlugin cbMoviePlugin = new CBMoviePlugin();
            //Plugins.Add(cbMoviePlugin.pluginID, cbMoviePlugin);
            //CBSeriesPlugin cBSeriesPlugin = new CBSeriesPlugin();
            //Plugins.Add(cBSeriesPlugin.pluginID, cBSeriesPlugin);
            //CBCartoonPlugin cBCartoonPlugin = new CBCartoonPlugin();
            //Plugins.Add(cBCartoonPlugin.pluginID, cBCartoonPlugin);

            EurostreamingPlugin eurostreamingPlugin = new EurostreamingPlugin();
            Plugins.Add(eurostreamingPlugin.pluginID, eurostreamingPlugin);

            FsprPlugin fsprPlugin = new FsprPlugin();
            Plugins.Add(fsprPlugin.pluginID, fsprPlugin);

            ICNPlugin icnPlugin = new ICNPlugin();
            Plugins.Add(icnPlugin.pluginID, icnPlugin);

            ItaliSeriePlugin italiaSeriePlugin = new ItaliSeriePlugin();
            Plugins.Add(italiaSeriePlugin.pluginID, italiaSeriePlugin);

            PirateStreamingMoviesPlugin pirateStreamingMoviesPlugin = new PirateStreamingMoviesPlugin();
            Plugins.Add(pirateStreamingMoviesPlugin.pluginID, pirateStreamingMoviesPlugin);

            SDPlugin sdPlugin = new SDPlugin();
            Plugins.Add(sdPlugin.pluginID, sdPlugin);

        }

        public static List<IEDFPlugin> getPluginsSorted()
        {
            List<IEDFPlugin> plugins = new List<IEDFPlugin>();
            plugins.AddRange(Plugins.Values);

            plugins.Sort((x, y) => x.pluginName.CompareTo(y.pluginName));

            return plugins;
        }
    }
}

