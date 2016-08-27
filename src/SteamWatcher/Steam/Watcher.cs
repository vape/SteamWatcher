using NLog;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamWatcher.Steam
{
    public class Watcher
    {
        public bool Running
        { get; set; }

        private Logger logger = LogManager.GetLogger("watcher");

        private readonly SteamClient client;
        private readonly SteamUser user;
        private readonly SteamApps apps;
        private readonly CallbackManager callbackManager;

        private bool loggedOn;
        private uint lastChangeNumber;

        public Watcher()
        {
            client = new SteamClient();
            user = client.GetHandler<SteamUser>();
            apps = client.GetHandler<SteamApps>();

            callbackManager = new CallbackManager(client);
            callbackManager.Subscribe<SteamClient.ConnectedCallback>(ConnectedHandler);
            callbackManager.Subscribe<SteamClient.DisconnectedCallback>(DisconnectedHandler);
            callbackManager.Subscribe<SteamUser.LoggedOnCallback>(LoggedOnHandler);
            callbackManager.Subscribe<SteamUser.LoggedOffCallback>(LoggedOffHandler);
            callbackManager.Subscribe<SteamApps.PICSChangesCallback>(PICSChangesHandler);
        }

        public void Tick()
        {
            var loadServersTask = SteamDirectory.Initialize(0);
            loadServersTask.Wait();

            if (loadServersTask.IsFaulted)
            {
                logger.Error($"Error loading server list from directory: {loadServersTask.Exception.Message}");
                return;
            }

            client.Connect();

            Running = true;
            while (Running)
            {
                callbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(5));

                if (loggedOn)
                {
                    apps.PICSGetChangesSince(lastChangeNumber, true, true);
                }
            }

            logger.Info("Stopping watcher...");
            client.Disconnect();
        }

        #region Events Routine

        private void ConnectedHandler(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                logger.Error($"Could not connect to Steam: {callback.Result}");
                Running = false;

                return;
            }

            logger.Info("Connected to Steam, logging in...");
            user.LogOnAnonymous();
        }

        private void DisconnectedHandler(SteamClient.DisconnectedCallback callback)
        {
            logger.Info("Disconnected");

            if (!Running)
            {
                logger.Info("Shutting down...");
                return;
            }

            loggedOn = false;

            logger.Info("Trying to reconnect...");
            Thread.Sleep(TimeSpan.FromSeconds(15));

            client.Connect();
        }

        private void LoggedOnHandler(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                logger.Error($"Failed to login: {callback.Result}");
                Thread.Sleep(TimeSpan.FromSeconds(2));

                return;
            }

            loggedOn = true;
            logger.Info("Logged in");
        }

        private void LoggedOffHandler(SteamUser.LoggedOffCallback callback)
        {
            loggedOn = false;
            logger.Info("Logged off");
        }

        private void PICSChangesHandler(SteamApps.PICSChangesCallback callback)
        {
            var previous = lastChangeNumber;
            if (previous == callback.CurrentChangeNumber)
            { return; }

            lastChangeNumber = callback.CurrentChangeNumber;

            logger.Info($"Changenumber changed: {previous} -> {lastChangeNumber}");

            var appChanges = callback.AppChanges.Select(x => x.Value.ID);
            var packageChanges = callback.PackageChanges.Select(x => x.Value.ID);

            var changelist = new Changelist(lastChangeNumber, appChanges, packageChanges);
            ChangelistResolver(changelist);
        }

        #endregion

        private void ChangelistResolver(Changelist changelist)
        {
            if (changelist.Apps.Count > 0 || changelist.Packages.Count > 0)
            {
                logger.Info($"{changelist.Apps.Count} apps changed / {changelist.Packages.Count} packages changed");
            }
            else
            {
                logger.Info("No apps/packages changed");
            }

            var db = new Database();
            foreach (var app in changelist.Apps.Select(id => db.SelectAppInfo((int)id)).Where(a => a != null))
            {
                logger.Info($"Known app changed: {app.Value.Name}");
            }
        }
    }
}
