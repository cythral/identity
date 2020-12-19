namespace Brighid.Identity
{
    public static class Constants
    {
        public const string RequestSource = "identity:request:source";
    }

    public enum IdentityRequestSource
    {
        Sns,
        Direct,
    }
}
