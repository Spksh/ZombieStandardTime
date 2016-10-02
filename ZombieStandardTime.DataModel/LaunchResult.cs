using System;

namespace ZombieStandardTime.DataModel
{
    public class LaunchResult
    {
        public bool Success { get; set; }

        public DateTime LaunchTimeUtc { get; set; }

        public LaunchResultMessage Message { get; set; }

        public string MessageExtended { get; set; }
        
        public LaunchResult()
        {
            
        }

        public LaunchResult(bool success, LaunchResultMessage message)
        {
            Success = success;
            Message = message;
        }

        public LaunchResult(bool success, LaunchResultMessage message, DateTime launchTimeUtc) : this(success, message)
        {
            LaunchTimeUtc = launchTimeUtc;
        }

        public LaunchResult(bool success, LaunchResultMessage message, string messageExtended) : this(success, message)
        {
            MessageExtended = messageExtended;
        }
    }
}
