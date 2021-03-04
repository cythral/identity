using System;

namespace Brighid.Identity.LoginProviders
{
    [Flags]
    public enum AuthType
    {
        OAuth,
        OpenID
    }
}
