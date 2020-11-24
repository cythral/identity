namespace Brighid.Identity.Client
{
    public class TokenCache
    {
        private Token? token;

        public virtual Token? Token
        {
            get => token?.HasExpired == true
                    ? token = null
                    : token;
            set => token = value;
        }
    }
}
