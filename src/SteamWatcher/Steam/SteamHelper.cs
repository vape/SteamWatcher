using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using SteamKit2;
using SteamWatcher.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher.Steam
{
    public static class SteamHelper
    {
        private static Logger logger = LogManager.GetLogger("steam-helper");

        public static PriceInfo? GetAppPrice(int appId)
        {
            logger.Info($"Trying to get price for {appId}...");

            var json = Request($"http://store.steampowered.com/api/appdetails?appids={appId}");
            if (json[appId.ToString()]["success"].ToObject<bool>())
            {
                var priceOverview = json[appId.ToString()]["data"]["price_overview"];
                if (priceOverview == null)
                {
                    logger.Info("Price overview is null");
                    return null;
                }

                var price = priceOverview["initial"].ToObject<int>();
                var discount = priceOverview["discount_percent"].ToObject<int>();

                logger.Info($"Got price for {appId}");
                return new PriceInfo(appId, price, discount);
            }
            else
            {
                logger.Info("Request not success");
                return null;
            }
        }

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

        private static JToken Request(string url)
        {
            using (var response = Get(new Uri(url)))
            {
                JToken json;
                using (var respStream = new JsonTextReader(new StreamReader(response.GetResponseStream())))
                {
                    json = JToken.ReadFrom(respStream);
                }

                return json;
            }
        }

        private static WebResponse Get(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var request = WebRequest.Create(uri);
            request.Method = "GET";

            return request.GetResponse();
        }
    }
}
