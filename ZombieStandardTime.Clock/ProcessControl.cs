using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using EasyHook;

namespace ZombieStandardTime.Clock
{
    public class ProcessControl
    {
        private static readonly object _lock = new object();
        private static ProcessControl _instance;

        public static ProcessControl Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProcessControl();
                }

                return _instance;
            }
        }

        private readonly string _channelName; // null to trigger auto-generation of random channel name, filled in by IpcCreateServer
        private readonly IpcServerChannel _channel;

        public ProcessControl()
        {
            _channel = RemoteHooking.IpcCreateServer<ZombieClockIpcChannel>(ref _channelName, WellKnownObjectMode.SingleCall);
        }

        public Process LaunchProcess(string processName, string path, string options, TimeSpan timeout)
        {
            lock (_lock)
            {
                if (Process.GetProcessesByName(processName).Any())
                {
                    Process game = Process.GetProcessesByName(processName).FirstOrDefault();

                    if (game != null)
                    {
                        return game;
                    }
                }

                Process launcher = Process.Start(path, options);
                launcher.WaitForExit((int)timeout.TotalMilliseconds);

                DateTime launcherExitTime = DateTime.Now;

                while (DateTime.Now.Subtract(launcherExitTime) < timeout)
                {
                    if (launcher.HasExited)
                    {
                        Process game = Process.GetProcessesByName(processName).FirstOrDefault();

                        if (game != null)
                        {
                            return game;
                        }
                    }

                    Thread.Sleep(10);
                }

                throw new TimeoutException("Timeout expired for LaunchProcess");
            }
        }

        public void InjectClockIntoProcess(Process process, TimeSpan fileTimeUtcOffset)
        {
            lock (_lock)
            {
                RemoteHooking.Inject(
                    process.Id,
                    InjectionOptions.DoNotRequireStrongName,
                    "ZombieStandardTime.Clock.dll",
                    null, // No 64-bit assembly
                    _channelName,
                    fileTimeUtcOffset);
            }
        }
    }
}
