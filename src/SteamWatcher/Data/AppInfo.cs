using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher.Data
{
    public struct AppInfo
    {
        public readonly int AppID;
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
