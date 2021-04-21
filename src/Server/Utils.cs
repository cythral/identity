using System;
using System.Security.Cryptography;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;

public delegate string GenerateRandomString(int length);
public delegate OpenIddictRequest GetOpenIdConnectRequest(Controller controller);

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

        public static OpenIddictRequest GetOpenIdConnectRequest(Controller controller)
        {
            return controller.HttpContext.GetOpenIddictServerRequest()!;
        }
    }
}
