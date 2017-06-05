using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;

namespace SAMLSilly.Specification
{
    /// <summary>
    /// Validates a self-signed certificate
    /// </summary>
    public class SelfIssuedCertificateSpecification : ICertificateSpecification
    {
        public bool IsSatisfiedBy(X509Certificate2 certificate, ILogger logger)
        {
            var chainPolicy = new X509ChainPolicy
            {
                RevocationMode = X509RevocationMode.NoCheck,
                VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
            };
            var defaultCertificateValidator = X509CertificateValidator.CreateChainTrustValidator(false, chainPolicy);

            try
            {
                defaultCertificateValidator.Validate(certificate);
                return true;
            }
            catch (Exception e)
            {
                logger.LogWarning(string.Format(ErrorMessages.CertificateIsNotRFC3280Valid, certificate.SubjectName.Name, certificate.Thumbprint), e);
            }

            return false;
        }
    }
}
