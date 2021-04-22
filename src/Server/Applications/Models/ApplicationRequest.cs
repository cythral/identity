namespace Brighid.Identity.Applications
{
    public class ApplicationRequest : Application
    {
        public new string[] Roles { get; set; }

        private new string EncryptedSecret { get; set; }

        private new string Secret { get; set; }
    }
}
