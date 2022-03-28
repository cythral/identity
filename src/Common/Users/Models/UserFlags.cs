using System;

namespace Brighid.Identity.Users
{
    [Flags]
    public enum UserFlags : long
    {
        None = 0,
        Debug = 1,
    }
}
