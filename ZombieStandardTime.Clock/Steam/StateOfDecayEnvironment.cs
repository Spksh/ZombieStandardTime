using System.IO;

namespace ZombieStandardTime.Clock.Steam
{
    public class StateOfDecayEnvironment
    {
        private static StateOfDecayEnvironment _instance;

        public static StateOfDecayEnvironment Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StateOfDecayEnvironment();
                }

                return _instance;
            }
        }

        public const string AppId = "241540";
        public const string ProcessName = "StateOfDecay";

        private string _installPath;
        private string _executablePath;
        private string _profilePath;
        private string _profileXmlPath;

        public string InstallPath
        {
            get
            {
                if (string.IsNullOrEmpty(_installPath))
                {
                    string path = SteamEnvironment.Instance.GetAppInstallDirFor(AppId);

                    if (!string.IsNullOrEmpty(path))
                    {
                        _installPath = Path.GetFullPath(path);
                    }
                }

                // If installPath is still null, we probably haven't been able to read config.vdf
                // We probably already guessed SteamPath, so guessing InstallPath is just as valid
                if (string.IsNullOrEmpty(_installPath))
                {
                    _installPath = Path.GetFullPath(SteamEnvironment.Instance.SteamInstallPath + @"\steamapps\common\State of Decay");
                }

                return _installPath;
            }
        }

        public string ExecutablePath
        {
            get
            {
                if (string.IsNullOrEmpty(_executablePath))
                {
                    if (!string.IsNullOrEmpty(InstallPath))
                    {
                        _executablePath = Path.GetFullPath(InstallPath + @"\StateOfDecay.exe");
                    }
                }

                return _executablePath;
            }
        }

        public string ProfilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_profilePath))
                {
                    if (!string.IsNullOrEmpty(InstallPath))
                    {
                        _profilePath = Path.GetFullPath(InstallPath + @"\USER\Profiles\default");
                    }
                }

                return _profilePath;
            }
        }

        public string ProfileXmlPath
        {
            get
            {
                if (string.IsNullOrEmpty(_profileXmlPath))
                {
                    _profileXmlPath = Path.GetFullPath(ProfilePath + @"\profile.xml");
                }

                return _profileXmlPath;
            }
        }

        public string GetUserDataPathFor(string account)
        {
            string path = SteamEnvironment.Instance.GetUserDataPathFor(AppId, account);

            if (!string.IsNullOrEmpty(path))
            {
                return Path.GetFullPath(SteamEnvironment.Instance.GetUserDataPathFor(AppId, account) + @"\local");
            }

            return null;
        }
    }
}
