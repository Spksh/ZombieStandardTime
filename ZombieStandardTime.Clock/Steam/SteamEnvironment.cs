using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using SteamKit2;

namespace ZombieStandardTime.Clock.Steam
{
    public class SteamEnvironment
    {
        private static SteamEnvironment _instance;

        public static SteamEnvironment Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SteamEnvironment();
                }

                return _instance;
            }
        }

        private readonly ExceptionReportingClient<SteamEnvironment> _reporter = new ExceptionReportingClient<SteamEnvironment>();
        private string _steamInstallPath;
        private string _userDataPath;
        private Dictionary<string, ulong> _users;
        private Dictionary<string, string> _userDataPaths;
        private KeyValue _installConfigStore;
        private KeyValue _steam;
        private KeyValue _apps;

        public string SteamInstallPath
        {
            get
            {
                try
                {
                    // Check registry first
                    if (string.IsNullOrEmpty(_steamInstallPath))
                    {
                        RegistryKey steamKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

                        if (steamKey != null)
                        {
                            object steamPath = steamKey.GetValue("SteamPath");

                            if (steamPath != null)
                            {
                                _steamInstallPath = Path.GetFullPath(steamPath.ToString());        
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    _reporter.ReportMajorExceptionInBackground(error, "SteamInstallPath");
                }

                // Still empty? Guess!
                if (string.IsNullOrEmpty(_steamInstallPath))
                {
                    if (Environment.Is64BitOperatingSystem)
                    {
                        _steamInstallPath = @"C:\Program Files (x86)\Steam";
                    }
                    else
                    {
                        _steamInstallPath = @"C:\Program Files\Steam";
                    }
                }

                return _steamInstallPath;
            }
        }

        public string UserDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_userDataPath))
                {
                    _userDataPath = Path.GetFullPath(SteamInstallPath + @"\userdata");
                }

                return _userDataPath;
            }
        }

        public Dictionary<string, ulong> Accounts
        {
            get
            {
                if (_users == null)
                {
                    _users = new Dictionary<string, ulong>();

                    if (Steam.IsValid())
                    {
                        KeyValue accounts = Steam["accounts"];

                        if (accounts.IsValid())
                        {
                            foreach (KeyValue account in accounts.Children)
                            {
                                if (account.IsValid())
                                {
                                    KeyValue steamId = account["SteamID"];

                                    if (steamId.IsValid())
                                    {
                                        _users.Add(account.Name, ulong.Parse(account["SteamID"].Value));
                                    }
                                }
                            }
                        }
                    }

                    // We haven't been able to parse config.vdf, or we found no steam accounts
                    // We'll add a dummy account so things don't just crash
                    if (!_users.Any())
                    {
                        _users.Add("Unknown", 0);
                    }
                }

                return _users;
            }
        }

        public Dictionary<string, string> UserDataPaths
        {
            get
            {
                if (_userDataPaths == null)
                {
                    _userDataPaths = new Dictionary<string, string>();

                    foreach (var account in Accounts)
                    {
                        string profileName = account.Key;
                        SteamID steamId = new SteamID(account.Value);

                        _userDataPaths.Add(profileName, Path.GetFullPath(UserDataPath + @"\" +  steamId.AccountID));
                    }
                }

                return _userDataPaths;
            }
        }

        public string GetUserDataPathFor(string appId, string account)
        {
            if (UserDataPaths.ContainsKey(account))
            {
                return Path.GetFullPath(UserDataPaths[account] + @"\" + appId);
            }

            return null;
        }

        public KeyValue InstallConfigStore
        {
            get
            {
                try
                {
                    if (_installConfigStore == null)
                    {
                        string installConfigStorePath = Path.GetFullPath(SteamInstallPath + @"\config\config.vdf");

                        if (File.Exists(installConfigStorePath))
                        {
                            _installConfigStore = KeyValue.LoadAsText(installConfigStorePath);
                        }
                    }
                }
                catch (Exception error)
                {
                    _reporter.ReportMajorExceptionInBackground(error, "InstallConfigStore");
                }

                return _installConfigStore;
            }
        }

        public KeyValue Steam
        {
            get
            {
                if (_steam == null)
                {
                    if (!InstallConfigStore.IsValid())
                    {
                        return null;
                    }

                    KeyValue software = InstallConfigStore["Software"];

                    if (!software.IsValid())
                    {
                        return null;
                    }

                    KeyValue valve = software["Valve"];

                    if (!valve.IsValid())
                    {
                        return null;
                    }

                    KeyValue steam = valve["steam"];

                    if (!steam.IsValid())
                    {
                        return null;
                    }

                    _steam = steam;
                }

                return _steam;
            }
        }

        public KeyValue Apps
        {
            get
            {
                if (_apps == null)
                {
                    if (Steam.IsValid())
                    {
                        _apps = Steam["apps"];
                    }
                }

                return _apps;
            }
        }

        public string GetAppInstallDirFor(string appId)
        {
            if (Apps.IsValid())
            {
                KeyValue app = Apps[appId];

                if (app.IsValid())
                {
                    KeyValue installDir = app["InstallDir"];

                    if (installDir.IsValid())
                    {
                        return installDir.Value;
                    }
                }
            }

            return null;
        }
    }

    public static class KeyValueExtentions
    {
        public static bool IsValid(this KeyValue keyValue)
        {
            return keyValue != null && keyValue != KeyValue.Invalid;
        }
    }
}
