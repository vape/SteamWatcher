using Nancy;
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
            Get["/"] = param =>
            {
                return "Nothing to do here yet.";
            };
        }
    }
}
