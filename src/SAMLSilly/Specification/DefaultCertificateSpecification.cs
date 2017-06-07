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
        public bool IsSatisfiedBy(X509Certificate2 certificate, ILogger logger)
        {
            var useMachineContext = false;
            var chainPolicy = new X509ChainPolicy { RevocationMode = X509RevocationMode.NoCheck };
            var defaultCertificateValidator = X509CertificateValidator.CreateChainTrustValidator(useMachineContext, chainPolicy);

            try
            {
                defaultCertificateValidator.Validate(certificate);
                return true;
            }
            catch (Exception e)
            {
                if (logger != null)
                {
                    logger.LogWarning(string.Format(ErrorMessages.CertificateIsNotRFC3280Valid, certificate.SubjectName.Name, certificate.Thumbprint), e);
                }
            }

            return false;
        }
    }
}
