namespace ZombieStandardTime.DataModel
{
    public enum LaunchResultMessage
    {
        Unknown = 0,
        Success = 1,

        PathToGameInvalid = 10,
        GameLaunchFailed = 11,
        PostLaunchFileUpdateFailed = 12,
        
        ProfileIsNull = 20,
        PathToUserDataInvalid = 21,
        PathToUserDataBackupInvalid = 22,
        UserDataRestoreFailed = 23
    }
}
