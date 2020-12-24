#pragma warning disable CA1034
namespace Brighid.Identity
{
    public static class Constants
    {
        public const string RequestSource = "identity:request:source";

        public static class ClaimTypes
        {
            public const string UserId = "userid";
            public const string Role = "role";
        }
    }

    public enum IdentityRequestSource
    {
        Sns,
        Direct,
    }
}
