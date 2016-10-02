using System;

namespace ZombieStandardTime.DataModel
{
    [Serializable]
    public class Profile
    {
        public string Name { get; set; }

        public string PathToUserData { get; set; }

        public bool OverridePathToUserData { get; set; }

        public bool BackupOnGameStart { get; set; }

        public bool DisableSimulatedTime { get; set; }

        public bool LimitSimulatedTime { get; set; }

        public long SimulatedTimeLimit { get; set; }

        public bool ForceSimulatedTime { get; set; }

        public long ForcedSimulatedTime { get; set; }
        
        public bool EnableSimulatedTime { get; set; }

        public Profile()
        {
            BackupOnGameStart = true;
            DisableSimulatedTime = true;
            SimulatedTimeLimit = new TimeSpan(0, 1, 0, 0).Ticks;
            ForcedSimulatedTime = new TimeSpan(0, 1, 0, 0).Ticks;
        }
    }
}
