using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;

namespace SAMLSilly.Specification
{
    /// <summary>
    /// Validates a self-signed certificate
    /// </summary>
    public class SelfIssuedCertificateSpecification : ICertificateSpecification
    {
        public bool IsSatisfiedBy(X509Certificate2 certificate)
        {
            var useMachineContext = false;
            var chainPolicy = new X509ChainPolicy
            {
                RevocationMode = X509RevocationMode.NoCheck,
                VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
            };

            X509Chain ch = new X509Chain(useMachineContext);
            ch.ChainPolicy = chainPolicy;

            return ch.Build(certificate);
        }
    }
}
