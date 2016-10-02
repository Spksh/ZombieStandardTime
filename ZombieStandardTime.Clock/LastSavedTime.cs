using System;
using System.IO;
using SteamKit2;

namespace ZombieStandardTime.Clock
{
    public class LastSavedTime
    {
        private static readonly object _lock = new object();
        private static LastSavedTime _instance;

        public static LastSavedTime Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LastSavedTime();
                }

                return _instance;
            }
        }
        
        private const string LastSavedTimeVdf = "LastSavedTime.vdf";
        private const string Class30Ulsave = "Class3.0.ulsave";

        private readonly ExceptionReportingClient<LastSavedTime> _reporter = new ExceptionReportingClient<LastSavedTime>();

        public DateTime GetLastSavedTimeUtcFor(string pathToUserData)
        {
            lock (_lock)
            {
                string lastSavedTimePath = Path.GetFullPath(pathToUserData + @"\" + LastSavedTimeVdf);
                string saveGamePath = Path.GetFullPath(pathToUserData + @"\" + Class30Ulsave);

                try
                {
                    if (File.Exists(lastSavedTimePath))
                    {
                        KeyValue lastSavedTime = KeyValue.LoadAsText(lastSavedTimePath);

                        if (lastSavedTime != null)
                        {
                            long fileTimeUtc;

                            if (long.TryParse(lastSavedTime["fileTimeUtc"].Value, out fileTimeUtc))
                            {
                                return DateTime.FromFileTimeUtc(fileTimeUtc);
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(saveGamePath))
                        {
                            DateTime timeUtc = File.GetLastWriteTimeUtc(saveGamePath);

                            WriteLastSavedTimeFor(lastSavedTimePath, timeUtc);

                            return timeUtc;
                        }
                    }
                }
                catch (Exception error)
                {
                    _reporter.ReportFatalException(error, "GetLastSavedTimeUtcFor");
                    throw;
                }
            }

            return DateTime.UtcNow;
        }

        /// <summary>
        /// We can't assume that when the game exits the player hasn't been sitting on the menu screen for ages, so we can't rely on process exit for last saved time
        /// However, writes to savegame files while under ZST control will show normal system time for Last Modified, not ZST time
        /// To get the last save time in ZST, we take the final Last Modified and offset by our starting calculated time offset
        /// </summary>
        /// <param name="pathToUserData"></param>
        /// <param name="fileTimeUtcOffset"></param>
        public void SetLastSavedTimeFor(string pathToUserData, TimeSpan fileTimeUtcOffset)
        {
            lock (_lock)
            {
                string lastSavedTimePath = Path.GetFullPath(pathToUserData + @"\" + LastSavedTimeVdf);
                string saveGamePath = Path.GetFullPath(pathToUserData + @"\" + Class30Ulsave);

                try
                {
                    if (File.Exists(saveGamePath))
                    {
                        WriteLastSavedTimeFor(lastSavedTimePath, File.GetLastWriteTimeUtc(saveGamePath).Subtract(fileTimeUtcOffset));
                    }
                }
                catch (Exception error)
                {
                    _reporter.ReportFatalException(error, "SetLastSavedTimeFor");
                    throw;
                }
            }
        }

        private void WriteLastSavedTimeFor(string lastSavedTimePath, DateTime timeUtc)
        {
            lock (_lock)
            {
                KeyValue lastSavedTime = new KeyValue("LastSavedTime");
                lastSavedTime.Children.Add(new KeyValue("fileTimeUtc", timeUtc.ToFileTimeUtc().ToString()));

                try
                {
                    lastSavedTime.SaveToFile(lastSavedTimePath, false);
                }
                catch (Exception error)
                {
                    _reporter.ReportFatalException(error,"WriteLastSavedTimeFor");
                    throw;
                }
            }
        }
    }
}
