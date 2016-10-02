using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ZombieStandardTime.Clock;
using ZombieStandardTime.Clock.Steam;
using ZombieStandardTime.DataModel;
using ZombieStandardTime.Properties;

namespace ZombieStandardTime
{
    public class Model
    {
        private static Model _instance;
        
        public static Model Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Model();
                }

                return _instance;
            }
        }
        
        private readonly ExceptionReportingClient<Model> _reporter = new ExceptionReportingClient<Model>();
        private Dictionary<LaunchResultMessage, string> _launchResultMessages;

        public SteamEnvironmentSettings SteamEnvironmentSettings
        {
            get
            {
                if (Settings.Default.SteamEnvironmentSettings == null)
                {
                    Settings.Default.SteamEnvironmentSettings = new SteamEnvironmentSettings();
                }

                return Settings.Default.SteamEnvironmentSettings;
            }
        }
        
        public string CurrentProfileName
        {
            get
            {
                return SteamEnvironmentSettings.CurrentProfileName;
            }
            set
            {
                SteamEnvironmentSettings.CurrentProfileName = value;
            }
        }

        public string PathToGame
        {
            get
            {
                return SteamEnvironmentSettings.PathToGame;
            }
            set
            {
                SteamEnvironmentSettings.PathToGame = value;
            }
        }

        public bool OverridePathToGame
        {
            get
            {
                return SteamEnvironmentSettings.OverridePathToGame;
            }
            set
            {
                SteamEnvironmentSettings.OverridePathToGame = value;
            }
        }

        public string LaunchOptions
        {
            get
            {
                return SteamEnvironmentSettings.LaunchOptions;
            }
            set
            {
                SteamEnvironmentSettings.LaunchOptions = value;
            }
        }

        public List<Profile> Profiles
        {
            get
            {
                if (Settings.Default.Profiles == null)
                {
                    Settings.Default.Profiles = new Profiles();
                }
                
                return Settings.Default.Profiles;
            }
        }

        public Profile GetProfileFor(string name)
        {
            Profile profile = Profiles.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (profile == null)
            {
                profile = new Profile { Name = name };

                Profiles.Add(profile);
            }

            return profile;
        }

        public void Save()
        {
            try
            {
                Settings.Default.Save();
            }
            catch (Exception error)
            {
                _reporter.ReportMajorException(error, "Save");
            }
        }

        public List<String> ProfileNames
        {
            get
            {
                return SteamEnvironment.Instance.Accounts.Keys.ToList();
            }
        }

        public string SteamPath
        {
            get
            {
                return SteamEnvironment.Instance.SteamInstallPath;
            }
        }

        public Dictionary<LaunchResultMessage, string> LaunchResultMessages
        {
            get
            {
                if (_launchResultMessages == null)
                {
                    _launchResultMessages = new Dictionary<LaunchResultMessage, string>();

                    Dictionary<string, string> resources = Resources.ResourceManager
                        .GetResourceSet(CultureInfo.CurrentCulture, true, true)
                        .Cast<DictionaryEntry>()
                        .Where(r => r.Key is string && r.Value is string)
                        .ToDictionary(r => r.Key as string, r => r.Value as string);

                    foreach (LaunchResultMessage resultMessage in Enum.GetValues(typeof(LaunchResultMessage)).Cast<LaunchResultMessage>())
                    {
                        if (resources.ContainsKey(resultMessage.ToString()))
                        {
                            _launchResultMessages.Add(resultMessage, resources[resultMessage.ToString()]);
                        }
                    }
                }

                return _launchResultMessages;
            }
        }
    }
}
