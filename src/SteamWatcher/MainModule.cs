using Nancy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get["/api/changes/{since}"] = param =>
            {
                int sinceTime = param.since;
                var db = new Database();

                return JsonConvert.SerializeObject(db.SelectPriceChanges(sinceTime));
            };
        }
    }
}
