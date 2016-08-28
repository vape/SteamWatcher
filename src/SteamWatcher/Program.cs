using CommandLine;
using CommandLine.Text;
using NLog;
using SteamWatcher.Steam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher
{
    public interface IConfig
    {
        string Host
        { get; }

        string DatabaseFile
        { get; }

        string ApplicationDirectory
        { get; }
    }

    public static class Program
    {
        private class Options : IConfig
        {
            private const string DefaultHost = "localhost:8080";
            private const bool DefaultFetchApps = false;
            private static readonly string DefaultAppDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SteamWatcher");

            [Option('h', "host", DefaultValue = DefaultHost, HelpText = "API server host address", Required = false)]
            public string Host
            { get; set; }

            [Option('d', "db", HelpText = "Custom database file path", Required = false)]
            public string DatabaseFile
            { get; set; }

            [Option('a', "appdir", HelpText = "Custom application directory", Required = false)]
            public string ApplicationDirectory
            { get; set; }

            [Option('f', "fetch-apps", DefaultValue = DefaultFetchApps, HelpText = "Fetch apps list on startup", Required = false)]
            public bool FetchApps
            { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }

            public void SetDefaults()
            {
                if (ApplicationDirectory == null)
                {
                    ApplicationDirectory = DefaultAppDirectory;
                }

                if (!Directory.Exists(ApplicationDirectory))
                {
                    Directory.CreateDirectory(ApplicationDirectory);
                }

                if (DatabaseFile == null)
                {
                    DatabaseFile = Path.Combine(ApplicationDirectory, "database.db");
                }

                LogManager.Configuration.Variables["logpath"] = Path.Combine(ApplicationDirectory, "Logs");
            }
        }

        public static IConfig Config;

        private static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                options.SetDefaults();
                if (!File.Exists(options.DatabaseFile))
                {
                    Database.Create(options.DatabaseFile);
                    options.FetchApps = true;
                }

                if (options.FetchApps)
                {
                    var appsList = SteamHelper.FetchAppsList();
                    var db = new Database(options.DatabaseFile) ;

                    db.DeleteAppsInfo();
                    db.InsertAppsInfo(appsList.ToArray());
                }

                Config = options;
                Server.Run(Config.Host);
            }
        }
    }
}
