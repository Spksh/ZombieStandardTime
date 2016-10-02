using System;
using System.Runtime.InteropServices;
using System.Threading;
using EasyHook;

namespace ZombieStandardTime.Clock
{
    public class ZombieClock : IEntryPoint
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern void GetSystemTimeAsFileTime(out long lpSystemTimeAsFileTime);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
        public delegate void GetSystemTimeAsFileTimeDelegate(out long lpSystemTimeAsFileTime);

        private static TimeSpan _fileTimeUtcOffset;
        
        public static void GetSystemTimeAsFileTimeHooked(out long time)
        {
            time = DateTime.Now.Subtract(_fileTimeUtcOffset).ToFileTimeUtc();
        }

        private readonly ExceptionReportingClient<UserData> _reporter = new ExceptionReportingClient<UserData>();
        private readonly ZombieClockIpcChannel _ipcChannel;
        private LocalHook _getSystemTimeAsFileTime;

        public ZombieClock(RemoteHooking.IContext context, String channel, TimeSpan fileTimeUtcOffset)
        {
            try
            {
                _fileTimeUtcOffset = fileTimeUtcOffset;

                // connect to host...
                _ipcChannel = RemoteHooking.IpcConnectClient<ZombieClockIpcChannel>(channel);

                // Validate
                _ipcChannel.Ping();
            }
            catch (Exception error)
            {
                _reporter.ReportFatalException(error, "ZombieClock");
                throw;
            }
        }

        public void Run(RemoteHooking.IContext context, String channel, TimeSpan fileTimeUtcOffset)
        {
            try
            {
                _getSystemTimeAsFileTime = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "GetSystemTimeAsFileTime"),
                    new GetSystemTimeAsFileTimeDelegate(GetSystemTimeAsFileTimeHooked), 
                    null);

                _getSystemTimeAsFileTime.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            }
            catch (Exception error)
            {
                _reporter.ReportFatalException(error);
                return;
            }

            // Wait for host process termination...
            try
            {
                while (true)
                {
                    Thread.Sleep(500);
                    
                    // Keep alive
                    _ipcChannel.Ping();
                }
            }
            catch(Exception error)
            {
                // .NET Remoting will raise an exception if host is unreachable
                _reporter.ReportMajorException(error, "Run");
            }
        }
    }
}
