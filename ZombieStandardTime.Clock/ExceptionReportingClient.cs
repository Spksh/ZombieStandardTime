using System;
using System.Collections.Generic;
using System.Reflection;
using Mindscape.Raygun4Net;

namespace ZombieStandardTime.Clock
{
    public class ExceptionReportingClient<TReportingClass>
    {
        private readonly RaygunClient _reporter;
        private readonly string _reportingClass;
        private readonly string _assemblyVersion;

        public ExceptionReportingClient()
        {
            _reporter = new RaygunClient("=="); // TODO: Use real creds
            _reportingClass = typeof (TReportingClass).FullName;
            _assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public void ReportMinorException(Exception error, params string[] tags)
        {
            ReportException(error, ExceptionTag.Minor, tags);
        }

        public void ReportMinorExceptionInBackground(Exception error, params string[] tags)
        {
            ReportExceptionInBackground(error, ExceptionTag.Minor, tags);
        }

        public void ReportMajorException(Exception error, params string[] tags)
        {
            ReportException(error, ExceptionTag.Major, tags);
        }

        public void ReportMajorExceptionInBackground(Exception error, params string[] tags)
        {
            ReportExceptionInBackground(error, ExceptionTag.Major, tags);
        }

        public void ReportFatalException(Exception error, params string[] tags)
        {
            ReportException(error, ExceptionTag.Fatal, tags);
        }

        public void ReportFatalExceptionInBackground(Exception error, params string[] tags)
        {
            ReportExceptionInBackground(error, ExceptionTag.Fatal, tags);
        }

        public void ReportUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception error = e.ExceptionObject as Exception;

            if (error != null)
            {
                ReportException(error, ExceptionTag.Unhandled);
            }
        }

        public void ReportUnhandledExceptionInBackground(object sender, UnhandledExceptionEventArgs e)
        {
            Exception error = e.ExceptionObject as Exception;

            if (error != null)
            {
                ReportExceptionInBackground(error, ExceptionTag.Unhandled);
            }
        }

        private void ReportException(Exception error, string severity, params string[] tags)
        {
            // TODO: Send Exceptions when we have real creds
            //_reporter.Send(error, new List<string>(tags) { severity, _assemblyVersion, _reportingClass }, _assemblyVersion);
        }

        private void ReportExceptionInBackground(Exception error, string severity, params string[] tags)
        {
            // TODO: Send Exceptions when we have real creds
            //_reporter.SendInBackground(error, new List<string>(tags) { severity, _assemblyVersion, _reportingClass }, _assemblyVersion);
        }
    }

    public static class ExceptionTag
    {
        public const string Minor = "Minor";
        public const string Major = "Major";
        public const string Unhandled = "Unhandled";
        public const string Handled = "Handled";
        public const string Fatal = "Fatal";
    }
}
