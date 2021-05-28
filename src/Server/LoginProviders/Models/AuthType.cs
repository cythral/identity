using System;

namespace Brighid.Identity.LoginProviders
{
    [Flags]
    public enum AuthType
    {
        /// <summary>
        /// OAuth Login Provider Type.
        /// </summary>
        OAuth,

        /// <summary>
        /// OpenID Login Provider Type.
        /// </summary>
        OpenID,
    }
}
