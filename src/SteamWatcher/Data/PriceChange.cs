using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher.Data
{
    public struct PriceChange
    {
        [JsonProperty("appid")]
        public readonly int AppID;
        [JsonProperty("prev_price")]
        public readonly PriceInfo Previous;
        [JsonProperty("new_price")]
        public readonly PriceInfo New;
        [JsonProperty("updated_time")]
        public readonly DateTime Updated;

        public PriceChange(int appId, PriceInfo previous, PriceInfo newInfo)
            : this(appId, previous, newInfo, DateTime.UtcNow)
        { }

        public PriceChange(int appId, PriceInfo previous, PriceInfo newInfo, DateTime updated)
        {
            AppID = appId;
            Previous = previous;
            New = newInfo;
            Updated = updated;
        }

        public override string ToString()
        {
            return $"{{{Previous.ToString()}}} -> {{{New.ToString()}}}";
        }
    }
}
