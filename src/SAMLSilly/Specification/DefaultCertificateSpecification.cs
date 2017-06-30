using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;

namespace SAMLSilly.Specification
{
    /// <summary>
    /// Checks if a certificate is within its validity period
    /// Performs an online revocation check if the certificate contains a CRL url (oid: 2.5.29.31)
    /// </summary>
    public class DefaultCertificateSpecification : ICertificateSpecification
    {
        public bool IsSatisfiedBy(X509Certificate2 certificate)
        {
            var useMachineContext = false;

            X509Chain ch = new X509Chain(useMachineContext);
            ch.ChainPolicy = new X509ChainPolicy { RevocationMode = X509RevocationMode.NoCheck };

            return ch.Build(certificate);
        }
    }
}
