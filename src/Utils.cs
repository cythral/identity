using System;
using System.Security.Cryptography;

using AspNet.Security.OpenIdConnect.Primitives;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
public delegate string GenerateRandomString(int length);
public delegate OpenIdConnectRequest GetOpenIdConnectRequest(Controller controller);

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

        public static OpenIdConnectRequest GetOpenIdConnectRequest(Controller controller)
        {
            return controller.HttpContext.GetOpenIdConnectRequest();
        }
    }
}
