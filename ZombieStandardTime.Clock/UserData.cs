using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZombieStandardTime.Clock
{
    public class UserData
    {
        private static readonly object _lock = new object();
        private static UserData _instance;

        public static UserData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserData();
                }

                return _instance;
            }
        }

        private readonly ExceptionReportingClient<UserData> _reporter = new ExceptionReportingClient<UserData>();
        
        public string BackupUserDataFor(string pathToUserData)
        {
            lock (_lock)
            {
                DirectoryInfo userDataFolder = new DirectoryInfo(pathToUserData);

                if (!userDataFolder.Exists)
                {
                    return null;
                }

                if (userDataFolder.Parent == null)
                {
                    return null;
                }

                if (!userDataFolder.Parent.Exists)
                {
                    return null;
                }

                try
                {
                    DirectoryInfo backup = userDataFolder.Parent.CreateSubdirectory(userDataFolder.Name + "." + DateTime.Now.ToFileTimeUtc());

                    // Sanity check
                    if (!backup.Exists)
                    {
                        return null;
                    }

                    foreach (FileInfo file in userDataFolder.GetFiles())
                    {
                        FileInfo backupFile = file.CopyTo(Path.Combine(Path.GetFullPath(backup.FullName), file.Name), true);

                        backupFile.CreationTime = file.CreationTime;
                        backupFile.LastAccessTime = file.LastAccessTime;
                        backupFile.LastWriteTime = file.LastWriteTime;
                    }

                    try
                    {

                        backup.CreationTime = userDataFolder.CreationTime;
                        backup.LastAccessTime = userDataFolder.LastAccessTime;
                        backup.LastWriteTime = userDataFolder.LastWriteTime;
                    }
                    catch (IOException error)
                    {
                        // We get "IOException: The process cannot access the file 'xxxx' because it is being used by another process" if Windows Explorer has a handle
                        // It's a little bit hacky to try inside a try, but I'd rather catch these minor IOExceptions
                        // Based on quick testing, State of Decay doesn't really care about the timestamps on the folder anyway
                        _reporter.ReportMinorExceptionInBackground(error, "BackupUserDataFor", "SetDirectoryTimestamps");
                    }

                    return Path.GetFullPath(backup.FullName);
                }
                catch (Exception error)
                {
                    _reporter.ReportMajorExceptionInBackground(error, "BackupUserDataFor");
                    throw;
                }
            }
        }

        public bool RestoreLastBackupFor(string pathToUserData)
        {
            lock (_lock)
            {
                DirectoryInfo userDataFolder = new DirectoryInfo(pathToUserData);

                if (!userDataFolder.Exists)
                {
                    return false;
                }

                if (userDataFolder.Parent == null)
                {
                    return false;
                }

                if (!userDataFolder.Parent.Exists)
                {
                    return false;
                }

                try
                {
                    List<DirectoryInfo> backups = userDataFolder
                        .Parent
                        .GetDirectories("local.*", SearchOption.TopDirectoryOnly)
                        .Where(backup => Regex.IsMatch(backup.Name, @"[0-9]+\z"))
                        .OrderByDescending(backup => backup.Name)
                        .ToList();

                    if (backups.Any())
                    {
                        DirectoryInfo backup = backups.First();

                        // Sanity check
                        if (!backup.Exists)
                        {
                            return false;
                        }

                        foreach (FileInfo file in backup.GetFiles())
                        {
                            FileInfo restoredFile = file.CopyTo(Path.Combine(Path.GetFullPath(userDataFolder.FullName), file.Name), true);

                            restoredFile.CreationTime = file.CreationTime;
                            restoredFile.LastAccessTime = file.LastAccessTime;
                            restoredFile.LastWriteTime = file.LastWriteTime;
                        }

                        try
                        {
                            userDataFolder.CreationTime = backup.CreationTime;
                            userDataFolder.LastAccessTime = backup.LastAccessTime;
                            userDataFolder.LastWriteTime = backup.LastWriteTime;
                        }
                        catch (IOException error)
                        {
                            // We get "IOException: The process cannot access the file 'xxxx' because it is being used by another process" if Windows Explorer has a handle
                            // It's a little bit hacky to try inside a try, but I'd rather catch these minor IOExceptions
                            // Based on quick testing, State of Decay doesn't really care about the timestamps on the folder anyway
                            _reporter.ReportMinorExceptionInBackground(error, "RestoreLastBackupFor", "SetDirectoryTimestamps");
                        }

                        return true;
                    }
                }
                catch (Exception error)
                {
                    _reporter.ReportMajorExceptionInBackground(error, "RestoreLastBackupFor");
                    throw;
                }
            }

            return false;
        }
    }
}
