using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher.Data
{
    public struct PriceInfo
    {
        [JsonProperty("normalized_price")]
        public double NormalPrice
        {
            get
            {
                return (double)Price / 100d;
            }
        }
        [JsonProperty("currency")]
        public string Currency => "RUB";

        [JsonProperty("appid")]
        public readonly int AppID;
        [JsonProperty("price")]
        public readonly int Price;
        [JsonProperty("discount")]
        public readonly int Discount;
        [JsonProperty("updated_time")]
        public readonly DateTime Updated;

        public PriceInfo(int appId, int price, int discount, DateTime updated)
        {
            AppID = appId;
            Price = price;
            Discount = discount;
            Updated = updated;
        }

        public PriceInfo(int appId, int price, int discount)
            : this(appId, price, discount, DateTime.UtcNow)
        { }

        public override string ToString()
        {
            return $"Price: {NormalPrice}; Discount: {Discount}%";
        }
    }
}
