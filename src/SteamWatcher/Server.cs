using System;
using Nancy.Hosting.Self;
using SteamWatcher.Steam;
using System.Threading;
using NLog;

namespace SteamWatcher
{
    public static class Server
    {
        public static bool Running;

        private static Logger logger = LogManager.GetLogger("server");

        public static void Run(string address)
        {
            var uri = new Uri($"http://{address}/");
            var watcher = new Watcher();

            using (var host = new NancyHost(uri))
            {
                host.Start();
                logger.Info($"Running API server on {uri.ToString()}");

                Running = true;
                var watcherThread = new Thread(watcher.Tick);
                watcherThread.Name = "Steam Watcher";
                watcherThread.Start();

                while (Running)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        Running = false;
                    }
                }

                logger.Info("Stopping...");
                watcher.Running = false;
            }
        }
    }
}
