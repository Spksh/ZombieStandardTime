using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using EasyHook;
using ZombieStandardTime.Clock;
using ZombieStandardTime.Clock.Steam;

namespace ZombieStandardTime
{
    class Program
    {
        private static string _channelName = null; // null to trigger auto-generation of random channel name, filled in my IpcCreateServer

        private static void Main(string[] args)
        {
            try
            {
                RemoteHooking.IpcCreateServer<ZombieClockIpcChannel>(ref _channelName, WellKnownObjectMode.SingleCall); // This is null, but ref so we pass something to be filled in

                Process gameLauncher = Process.Start(StateOfDecayEnvironment.Instance.ExecutablePath);
                gameLauncher.WaitForExit();

                while (!Process.GetProcessesByName(StateOfDecayEnvironment.ProcessName).Any())
                {    
                    Thread.Sleep(10);
                }

                Process game = Process.GetProcessesByName(StateOfDecayEnvironment.ProcessName).First();

                RemoteHooking.Inject(
                    game.Id,
                    InjectionOptions.DoNotRequireStrongName, 
                    "ZombieStandardTime.Clock.dll",
                    null, // No 64-bit assembly
                    _channelName, 
                    "23/09/2013 9:30:00 p.m.");

                game.WaitForExit();
            }
            catch (Exception error)
            {
                Console.WriteLine("There was an error while connecting to target:\r\n{0}", error.ToString());
                Console.ReadLine();
            }

            Console.Write("Exited...");
            Console.ReadLine();
        }
    }
}
