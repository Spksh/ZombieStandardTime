using System;
using System.Windows;
using ZombieStandardTime.Clock;

namespace ZombieStandardTime
{
    public partial class App : Application
    {
        private readonly ExceptionReportingClient<App> _reporter = new ExceptionReportingClient<App>();
        
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += _reporter.ReportUnhandledException;
        }
    }
}
