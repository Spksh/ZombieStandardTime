using System;

namespace ZombieStandardTime.DataModel
{
    [Serializable]
    public class SteamEnvironmentSettings
    {
        public string CurrentProfileName { get; set; }

        public string PathToGame { get; set; }

        public bool OverridePathToGame { get; set; }

        public string LaunchOptions { get; set; }
    }
}
