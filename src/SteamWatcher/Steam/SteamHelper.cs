using NLog;
using SteamKit2;
using SteamWatcher.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher.Steam
{
    public static class SteamHelper
    {
        private static Logger logger = LogManager.GetLogger("steam-helper");

        public static List<AppInfo> FetchAppsList()
        {
            using (dynamic apps = WebAPI.GetInterface("ISteamApps"))
            {
                logger.Info("Fetching apps list...");

                var appsList = new List<AppInfo>();
                KeyValue response = apps.GetAppList2();

                foreach (var app in response["apps"].Children)
                {
                    var id = app["appid"].AsInteger();
                    var name = app["name"].AsString();

                    appsList.Add(new AppInfo(id, name));
                }

                logger.Info($"Apps list fetched: {appsList.Count} records");
                return appsList;
            }
        }
    }
}
