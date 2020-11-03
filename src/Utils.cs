using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public delegate string GenerateRandomString(int length);
public delegate Task<T> DeserializeAsync<T>(Stream stream);

namespace Brighid.Identity
{
    public static class Utils
    {
        public static string GenerateRandomString(int length)
        {
            using var cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            var buffer = new byte[length];
            cryptoRandomDataGenerator.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }
    }
}
