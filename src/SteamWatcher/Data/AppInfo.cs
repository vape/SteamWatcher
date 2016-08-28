using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher.Data
{
    public struct AppInfo
    {
        [JsonProperty("appid")]
        public readonly int AppID;
        [JsonProperty("name")]
        public readonly string Name;

        public AppInfo(int appId, string name)
        {
            AppID = appId;
            Name = name ?? "Unknown Application";
        }

        public override string ToString()
        {
            return $"AppID: {AppID}; Name: {Name}";
        }
    }
}
