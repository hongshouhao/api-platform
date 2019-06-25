using System;

namespace ApiGatewayManager.Exceptions
{
    public class UserFriendlyException : Exception
    {
        public UserFriendlyException(string message)
            : base(message)
        {
        }
    }
}
