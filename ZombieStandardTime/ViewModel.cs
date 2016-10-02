using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using ZombieStandardTime.Annotations;
using ZombieStandardTime.Clock;
using ZombieStandardTime.Clock.Steam;
using ZombieStandardTime.DataModel;
using ZombieStandardTime.Properties;

namespace ZombieStandardTime
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly ExceptionReportingClient<ViewModel> _reporter = new ExceptionReportingClient<ViewModel>();
        
        private string _account;
        private string _pathToGame;
        private bool _overridePathToGame;
        private string _launchOptions;
        private string _pathToUserData;
        private bool _overridePathToUserData;
        private bool _backupOnGameStart;
        private bool _disableSimulatedTime;
        private bool _limitSimulatedTime;
        private TimeSpan _simulatedTimeLimit;
        private bool _forceSimulatedTime;
        private TimeSpan _forcedSimulatedTime;
        private bool _enableSimulatedTime;
        private string _launchMessage;
        private bool _launched;
        private bool _launchFailed;
        private bool _newVersionAvailable;
        private Version _newVersion;

        public void Initialize()
        {
            if (!string.IsNullOrEmpty(Model.Instance.PathToGame) && Model.Instance.OverridePathToGame)
            {
                PathToGame = Model.Instance.PathToGame;
            }
            else
            {
                PathToGame = StateOfDecayEnvironment.Instance.ExecutablePath;
            }

            OverridePathToGame = Model.Instance.OverridePathToGame;

            if (!string.IsNullOrEmpty(Model.Instance.LaunchOptions))
            {
                LaunchOptions = Model.Instance.LaunchOptions;
            }

            PropertyChanged += HandlePropertyChanged;

            if (!string.IsNullOrEmpty(Model.Instance.CurrentProfileName))
            {
                Account = Model.Instance.CurrentProfileName;
            }
            else
            {
                Account = Model.Instance.ProfileNames.First();
            }
        }

        public void BrowseForPathToGame()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    FileName = "StateOfDecay.exe",
                    Filter = "Programs (.exe)|*.exe",
                    InitialDirectory = Model.Instance.SteamPath
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string path = openFileDialog.FileName;

                    if (File.Exists(path))
                    {
                        PathToGame = Path.GetFullPath(path);
                    }
                }
            }
            catch (Exception error)
            {
                _reporter.ReportMajorExceptionInBackground(error, "BrowseForPathToGame");
            }
        }

        public void BrowseForPathToUserData()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    FileName = "Class3.0.ulsave",
                    Filter = "User Data (.ulsave)|*.ulsave",
                    InitialDirectory = Model.Instance.SteamPath
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string path = openFileDialog.FileName;

                    if (File.Exists(path))
                    {
                        PathToUserData = Path.GetDirectoryName(path);
                    }
                }
            }
            catch (Exception error)
            {
                _reporter.ReportMajorExceptionInBackground(error, "BrowseForPathToUserData");
            }
        }

        public void MainWindowClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (Process.GetProcessesByName(StateOfDecayEnvironment.ProcessName).Any())
            {
                MessageBoxResult result = MessageBox.Show(Resources.StateOfDecayStillRunningMessage, Resources.StateOfDecayStillRunningCaption, MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                {
                    cancelEventArgs.Cancel = true;
                    return;
                }
            }

            Model.Instance.Save();
        }

        public void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            CheckLatestVersion();
        }

        public void CheckLatestVersion()
        {
            Task.Factory
                .StartNew(() => CheckLatestVersion(Version))
                .ContinueWith(task => CheckLatestVersionComplete(task.Result), TaskScheduler.FromCurrentSynchronizationContext());
        }

        private LatestVersionResult CheckLatestVersion(Version version)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://zombiestandardtime.com/api/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync("latestversion").Result;
                response.EnsureSuccessStatusCode();

                return new LatestVersionResult(true, version, response.Content.ReadAsAsync<LatestVersion>().Result);
            }
            catch (Exception error)
            {
                _reporter.ReportMinorExceptionInBackground(error, "CheckLatestVersion");
            }
            
            return new LatestVersionResult(false, version, null);
        }

        private void CheckLatestVersionComplete(LatestVersionResult result)
        {
            if (result.Success && result.LatestVersion > result.CurrentVersion)
            {
                NewVersionAvailable = true;
                NewVersion = result.LatestVersion;
            }
        }

        public void Launch()
        {
            // Reset messaging
            LaunchFailed = false;
            LaunchMessage = string.Empty;

            // We be launchin'
            Launched = true;
            
            Task.Factory
                .StartNew(() => Launch(Model.Instance.GetProfileFor(Account), PathToGame, LaunchOptions))
                .ContinueWith(task => LaunchComplete(task.Result), TaskScheduler.FromCurrentSynchronizationContext()); 
        }

        public void RestoreAndLaunch()
        {
            // Reset messaging
            LaunchFailed = false;
            LaunchMessage = string.Empty;

            // We be launchin'
            Launched = true;

            Task.Factory
                .StartNew(() => RestoreAndLaunch(Model.Instance.GetProfileFor(Account), PathToGame, LaunchOptions))
                .ContinueWith(task => LaunchComplete(task.Result), TaskScheduler.FromCurrentSynchronizationContext()); 
        }

        private LaunchResult RestoreAndLaunch(Profile profile, string pathToGame, string launchOptions)
        {
            try
            {
                bool restoredBackup = UserData.Instance.RestoreLastBackupFor(profile.PathToUserData);

                if (!restoredBackup)
                {
                    return new LaunchResult(false, LaunchResultMessage.UserDataRestoreFailed);
                }

            }
            catch (Exception error)
            {
                return new LaunchResult(false, LaunchResultMessage.UserDataRestoreFailed, error.Message);
            }

            return Launch(profile, pathToGame, launchOptions);
        }

        private LaunchResult Launch(Profile profile, string pathToGame, string launchOptions)
        {
            if (string.IsNullOrWhiteSpace(pathToGame) || !File.Exists(pathToGame))
            {
                return new LaunchResult(false, LaunchResultMessage.PathToGameInvalid);
            }

            if (profile == null)
            {
                return new LaunchResult(false, LaunchResultMessage.ProfileIsNull);
            }

            if (string.IsNullOrWhiteSpace(profile.PathToUserData) || !Directory.Exists(profile.PathToUserData))
            {
                return new LaunchResult(false, LaunchResultMessage.PathToUserDataInvalid);
            }
            
            try
            {
                if (profile.BackupOnGameStart)
                {
                    string backupPath = UserData.Instance.BackupUserDataFor(profile.PathToUserData);

                    if (string.IsNullOrWhiteSpace(backupPath) || !Directory.Exists(backupPath))
                    {
                        return new LaunchResult(false, LaunchResultMessage.PathToUserDataBackupInvalid);
                    }
                }
            }
            catch (Exception error)
            {
                return new LaunchResult(false, LaunchResultMessage.PathToUserDataBackupInvalid, error.Message);
            }

            DateTime lastSavedUtc = LastSavedTime.Instance.GetLastSavedTimeUtcFor(profile.PathToUserData);
            DateTime nowUtc = DateTime.UtcNow;

            TimeSpan fileTimeUtcOffset = TimeSpan.Zero;

            if (EnableSimulatedTime)
            {
                // Offset is nothing
            }

            if (DisableSimulatedTime)
            {
                fileTimeUtcOffset = nowUtc - lastSavedUtc;
            }

            if (LimitSimulatedTime)
            {
                TimeSpan sinceLastSaved = nowUtc - lastSavedUtc;

                // If we're loading a game with a lastsaved greater than our SimulatedTimeLimit
                if (sinceLastSaved > SimulatedTimeLimit)
                {
                    // Our fileTimeUtcOffset is truncated by the SimulatedTimeLimit
                    fileTimeUtcOffset = sinceLastSaved - SimulatedTimeLimit;
                }
            }

            if (ForceSimulatedTime)
            {
                fileTimeUtcOffset = (nowUtc - lastSavedUtc) - ForcedSimulatedTime;
            }

            try
            {
                Process game = ProcessControl.Instance.LaunchProcess(StateOfDecayEnvironment.ProcessName, pathToGame, launchOptions, new TimeSpan(0, 0, 60));

                if (fileTimeUtcOffset > TimeSpan.Zero)
                {
                    ProcessControl.Instance.InjectClockIntoProcess(game, fileTimeUtcOffset);
                }

                game.WaitForExit();
            }
            catch (Exception error)
            {
                _reporter.ReportFatalExceptionInBackground(error, "Launch");
                
                return new LaunchResult(false, LaunchResultMessage.GameLaunchFailed, error.Message);
            }

            try
            {
                // Write exit time as LastSavedTime.xml
                // - exit time must be adjusted time, DateTime.Now - sim time offset
                LastSavedTime.Instance.SetLastSavedTimeFor(profile.PathToUserData, fileTimeUtcOffset);

                // Rewrite profile.xml with real launch time
                LastPlayedTime.Instance.SetLastPlayedTimeFor(StateOfDecayEnvironment.Instance.ProfileXmlPath, nowUtc.ToLocalTime());
            }
            catch (Exception error)
            {
                return new LaunchResult(false, LaunchResultMessage.PostLaunchFileUpdateFailed, error.Message);
            }

            return new LaunchResult(true, LaunchResultMessage.Success, nowUtc);
        }

        private void LaunchComplete(LaunchResult result)
        {
            Launched = false;
            
            if (result.Success)
            {
                LaunchFailed = false;
                LaunchMessage = string.Empty;

                return;
            }

            LaunchFailed = true;
            LaunchMessage = string.IsNullOrWhiteSpace(result.MessageExtended)
                ? Model.Instance.LaunchResultMessages[result.Message]
                : string.Format("{0} {1}", Model.Instance.LaunchResultMessages[result.Message], result.MessageExtended);
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PathToGame")
            {
                Model.Instance.PathToGame = PathToGame;
                return;
            }

            if (e.PropertyName == "OverridePathToGame")
            {
                Model.Instance.OverridePathToGame = OverridePathToGame;

                if (!OverridePathToGame)
                {
                    PathToGame = StateOfDecayEnvironment.Instance.ExecutablePath;
                }

                return;
            }

            if (e.PropertyName == "LaunchOptions")
            {
                Model.Instance.LaunchOptions = LaunchOptions;
                return;
            }

            if (e.PropertyName == "Account" && !string.IsNullOrEmpty(Account))
            {
                Model.Instance.CurrentProfileName = Account;
                SetProfileView(Account);

                return;
            }

            SetProfileValue(e.PropertyName);
        }

        private void SetProfileView(string account)
        {
            Profile profile = Model.Instance.GetProfileFor(account);

            if (string.IsNullOrEmpty(profile.PathToUserData))
            {
                profile.PathToUserData = StateOfDecayEnvironment.Instance.GetUserDataPathFor(profile.Name);
            }

            if (!string.IsNullOrEmpty(profile.PathToUserData) && profile.OverridePathToUserData)
            {
                PathToUserData = profile.PathToUserData;
            }
            else
            {
                PathToUserData = StateOfDecayEnvironment.Instance.GetUserDataPathFor(profile.Name);
            }

            OverridePathToUserData = profile.OverridePathToUserData;
            BackupOnGameStart = profile.BackupOnGameStart;
            DisableSimulatedTime = profile.DisableSimulatedTime;
            LimitSimulatedTime = profile.LimitSimulatedTime;
            SimulatedTimeLimit = new TimeSpan(profile.SimulatedTimeLimit);
            ForceSimulatedTime = profile.ForceSimulatedTime;
            ForcedSimulatedTime = new TimeSpan(profile.ForcedSimulatedTime);
            EnableSimulatedTime = profile.EnableSimulatedTime;
        }

        private void SetProfileValue(string property)
        {
            Profile profile = Model.Instance.GetProfileFor(Account);

            switch (property)
            {
                case "PathToUserData":
                    profile.PathToUserData = PathToUserData;
                    break;

                case "OverridePathToUserData":
                    profile.OverridePathToUserData = OverridePathToUserData;

                    if (!OverridePathToUserData)
                    {
                        PathToUserData = StateOfDecayEnvironment.Instance.GetUserDataPathFor(profile.Name);
                    }

                    break;

                case "BackupOnGameStart":
                    profile.BackupOnGameStart = BackupOnGameStart;
                    break;

                case "DisableSimulatedTime":
                    profile.DisableSimulatedTime = DisableSimulatedTime;
                    break;

                case "LimitSimulatedTime":
                    profile.LimitSimulatedTime = LimitSimulatedTime;
                    break;

                case "SimulatedTimeLimit":
                    profile.SimulatedTimeLimit = SimulatedTimeLimit.Ticks;
                    break;

                case "ForceSimulatedTime":
                    profile.ForceSimulatedTime = ForceSimulatedTime;
                    break;

                case "ForcedSimulatedTime":
                    profile.ForcedSimulatedTime = ForcedSimulatedTime.Ticks;
                    break;

                case "EnableSimulatedTime":
                    profile.EnableSimulatedTime = EnableSimulatedTime;
                    break;
            }
        }        

        #region Properties

        public List<String> Accounts
        {
            get
            {
                return Model.Instance.ProfileNames;
            }
        }

        public string Account
        {
            get { return _account; }
            set
            {
                if (value == _account) return;
                _account = value;
                OnPropertyChanged("Account");
            }
        }

        public string PathToGame
        {
            get { return _pathToGame; }
            set
            {
                if (value == _pathToGame) return;
                _pathToGame = value;
                OnPropertyChanged("PathToGame");
            }
        }

        public bool OverridePathToGame
        {
            get { return _overridePathToGame; }
            set
            {
                if (value.Equals(_overridePathToGame)) return;
                _overridePathToGame = value;
                OnPropertyChanged("OverridePathToGame");
            }
        }

        public string LaunchOptions
        {
            get { return _launchOptions; }
            set
            {
                if (value == _launchOptions) return;
                _launchOptions = value;
                OnPropertyChanged("LaunchOptions");
            }
        }

        public string LaunchMessage
        {
            get { return _launchMessage; }
            set
            {
                if (value == _launchMessage) return;
                _launchMessage = value;
                OnPropertyChanged("LaunchMessage");
            }
        }

        public bool Launched
        {
            get { return _launched; }
            set
            {
                if (value == _launched) return;
                _launched = value;
                OnPropertyChanged("Launched");
            }
        }

        public bool LaunchFailed
        {
            get { return _launchFailed; }
            set
            {
                if (value == _launchFailed) return;
                _launchFailed = value;
                OnPropertyChanged("LaunchFailed");
            }
        }

        public string PathToUserData
        {
            get { return _pathToUserData; }
            set
            {
                if (value == _pathToUserData) return;
                _pathToUserData = value;
                OnPropertyChanged("PathToUserData");
            }
        }

        public bool OverridePathToUserData
        {
            get { return _overridePathToUserData; }
            set
            {
                if (value.Equals(_overridePathToUserData)) return;
                _overridePathToUserData = value;
                OnPropertyChanged("OverridePathToUserData");
            }
        }

        public bool BackupOnGameStart
        {
            get { return _backupOnGameStart; }
            set
            {
                if (value.Equals(_backupOnGameStart)) return;
                _backupOnGameStart = value;
                OnPropertyChanged("BackupOnGameStart");
            }
        }

        public bool DisableSimulatedTime
        {
            get { return _disableSimulatedTime; }
            set
            {
                if (value.Equals(_disableSimulatedTime)) return;
                _disableSimulatedTime = value;
                OnPropertyChanged("DisableSimulatedTime");
            }
        }

        public bool LimitSimulatedTime
        {
            get { return _limitSimulatedTime; }
            set
            {
                if (value.Equals(_limitSimulatedTime)) return;
                _limitSimulatedTime = value;
                OnPropertyChanged("LimitSimulatedTime");
            }
        }

        public TimeSpan SimulatedTimeLimit
        {
            get { return _simulatedTimeLimit; }
            set
            {
                if (value.Equals(_simulatedTimeLimit)) return;
                _simulatedTimeLimit = value;
                OnPropertyChanged("SimulatedTimeLimit");
            }
        }

        public bool ForceSimulatedTime
        {
            get { return _forceSimulatedTime; }
            set
            {
                if (value.Equals(_forceSimulatedTime)) return;
                _forceSimulatedTime = value;
                OnPropertyChanged("ForceSimulatedTime");
            }
        }

        public TimeSpan ForcedSimulatedTime
        {
            get { return _forcedSimulatedTime; }
            set
            {
                if (value.Equals(_forcedSimulatedTime)) return;
                _forcedSimulatedTime = value;
                OnPropertyChanged("ForcedSimulatedTime");
            }
        }

        public bool EnableSimulatedTime
        {
            get { return _enableSimulatedTime; }
            set
            {
                if (value.Equals(_enableSimulatedTime)) return;
                _enableSimulatedTime = value;
                OnPropertyChanged("EnableSimulatedTime");
            }
        }

        public Version Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public bool NewVersionAvailable
        {
            get { return _newVersionAvailable; }
            set
            {
                if (value.Equals(_newVersionAvailable)) return;
                _newVersionAvailable = value;
                OnPropertyChanged("NewVersionAvailable");
            }
        }

        public Version NewVersion
        {
            get { return _newVersion; }
            set
            {
                if (value.Equals(_newVersion)) return;
                _newVersion = value;
                OnPropertyChanged("NewVersion");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
