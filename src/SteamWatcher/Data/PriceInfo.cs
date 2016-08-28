using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher.Data
{
    public struct PriceInfo
    {
        public double NormalPrice
        {
            get
            {
                return (double)Price / 100d;
            }
        }

        public readonly int AppID;
        public readonly int Price;
        public readonly int Discount;
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
