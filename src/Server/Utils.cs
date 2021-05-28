using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Abstractions;

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

        public static SecurityKey GenerateDevelopmentSecurityKey()
        {
            var subject = new X500DistinguishedName("CN=OpenIddict Server Signing Certificate");
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            // Try to retrieve the existing development certificates from the specified store.
            // If no valid existing certificate was found, create a new signing certificate.
            var certificate = (from cert in store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, subject.Name, validOnly: false).OfType<X509Certificate2>()
                               where cert.NotBefore < DateTime.Now && cert.NotAfter > DateTime.Now
                               select cert).FirstOrDefault();

            if (certificate == null)
            {
                using var algorithm = RSA.Create(keySizeInBits: 2048);

                var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

                certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(2));

                // Note: CertificateRequest.CreateSelfSigned() doesn't mark the key set associated with the certificate
                // as "persisted", which eventually prevents X509Store.Add() from correctly storing the private key.
                // To work around this issue, the certificate payload is manually exported and imported back
                // into a new X509Certificate2 instance specifying the X509KeyStorageFlags.PersistKeySet flag.
                var data = certificate.Export(X509ContentType.Pfx, string.Empty);

                try
                {
                    var flags = X509KeyStorageFlags.PersistKeySet;
                    certificate = new X509Certificate2(data, string.Empty, flags);
                }
                finally
                {
                    Array.Clear(data, 0, data.Length);
                }

                store.Add(certificate);
            }

            return new X509SecurityKey(certificate);
        }
    }
}

public delegate string GenerateRandomString(int length);

public delegate OpenIddictRequest GetOpenIdConnectRequest(Controller controller);
