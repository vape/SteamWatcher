using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher.Utilities
{
    public static class Extensions
    {
        private static readonly DateTime Epoch =
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTime(this int time)
        {
            return Epoch.AddSeconds(time);
        }

        public static double ToUnixTime(this DateTime time)
        {
            return Math.Round((time - Epoch).TotalSeconds, 2);
        }
    }
}
