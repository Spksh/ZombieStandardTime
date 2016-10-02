using System;

namespace ZombieStandardTime.DataModel
{
    public class LatestVersionResult
    {
        public bool Success { get; set; }
        public Version CurrentVersion { get; set; }
        public Version LatestVersion { get; set; }

        public LatestVersionResult()
        {
            
        }

        public LatestVersionResult(bool success, Version current, LatestVersion latest)
        {
            Success = success;
            CurrentVersion = current;
            LatestVersion = latest.Version;
        }
    }
}
