using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SAMLSilly20.Tests
{
    public class UnitTest1
    {
        private X509Certificate2 _certificate;

        public UnitTest1()
        {
            _certificate = Certificates.InMemoryResourceUtility.GetInMemoryCertificate("pingcertificate.crt");
        }
        
        public void Default_certificate_verification()
        {            
            X509Chain ch = new X509Chain(false);
            //ch.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            //ch.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;// | X509VerificationFlags.IgnoreNotTimeValid;

            ch.ChainPolicy = new X509ChainPolicy { RevocationMode = X509RevocationMode.NoCheck };

            var isValid = ch.Build(_certificate);

        }

        [Fact]
        public void self_signned_certificate_verification_should_pass()
        {
            X509Chain ch = new X509Chain(false);
            ch.ChainPolicy = new X509ChainPolicy
            {
                RevocationMode = X509RevocationMode.NoCheck,
                VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
            };

            var isValid = ch.Build(_certificate);

            Assert.True(isValid);
        }

        [Fact]
        public void self_signned_certificate_verification_shouldnot_pass()
        {
            var certificate = Certificates.InMemoryResourceUtility.GetInMemoryCertificate("pingcertificate.crt");

            X509Chain ch = new X509Chain(false);
            ch.ChainPolicy = new X509ChainPolicy
            {
                RevocationMode = X509RevocationMode.NoCheck
            };

            var isValid = ch.Build(_certificate);

            Assert.False(isValid);
        }
    }
}
