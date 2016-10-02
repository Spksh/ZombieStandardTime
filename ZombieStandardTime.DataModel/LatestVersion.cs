using System;

namespace ZombieStandardTime.DataModel
{
    public class LatestVersion
    {
        public Version Version { get; set; }

        public LatestVersion()
        {
            
        }

        public LatestVersion(string version)
        {
            Version = new Version(version);
        }
    }
}
