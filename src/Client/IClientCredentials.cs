namespace Brighid.Identity.Client
{
    public interface IClientCredentials
    {
        string ClientId { get; set; }
        string ClientSecret { get; set; }
    }
}
