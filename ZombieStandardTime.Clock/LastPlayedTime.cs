using System;
using System.IO;
using System.Text;

namespace ZombieStandardTime.Clock
{
    public class LastPlayedTime
    {
        private static readonly object _lock = new object();
        private static readonly DateTime _fileTimeStartFromLocal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();

        private static LastPlayedTime _instance;

        public static LastPlayedTime Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LastPlayedTime();
                }

                return _instance;
            }
        }

        private readonly ExceptionReportingClient<LastPlayedTime> _reporter = new ExceptionReportingClient<LastPlayedTime>();

        public void SetLastPlayedTimeFor(string pathToProfileXml, DateTime lastPlayedLocal)
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(pathToProfileXml))
                    {
                        File.WriteAllText(
                            pathToProfileXml,
                            string.Format("<Profile Name=\"default\" LastPlayed=\"{0}\"/>", (int) (lastPlayedLocal - _fileTimeStartFromLocal).TotalSeconds), 
                            Encoding.ASCII);
                    }
                }
                catch (Exception error)
                {
                    _reporter.ReportMajorExceptionInBackground(error, "SetLastPlayedTimeFor");
                    throw;
                }
            }
        }
    }
}
