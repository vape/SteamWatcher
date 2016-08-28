using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher.Data
{
    public struct PriceChange
    {
        public readonly int AppID;
        public readonly PriceInfo Previous;
        public readonly PriceInfo New;
        public readonly DateTime Updated;

        public PriceChange(int appId, PriceInfo previous, PriceInfo newInfo)
        {
            AppID = appId;
            Previous = previous;
            New = newInfo;
            Updated = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"{{{Previous.ToString()}}} -> {{{New.ToString()}}}";
        }
    }
}
