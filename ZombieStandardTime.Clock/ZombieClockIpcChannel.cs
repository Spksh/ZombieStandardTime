using System;

namespace ZombieStandardTime.Clock
{
    public class ZombieClockIpcChannel : MarshalByRefObject
    {
        public void Ping()
        {
            // Empty, just to keep channel open
        }
    }
}
