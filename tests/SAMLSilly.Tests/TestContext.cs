using System.IO;
using System.Security.Cryptography.X509Certificates;
using SAMLSilly.Config;
using Xunit.Abstractions;

namespace SAMLSilly.Tests
{
    public class TestContext
    {
        private X509Certificate2 _devCertSign;
        private X509Certificate2 _safewhereTest_SFS;

        private Saml2Configuration _config;

        public TestContext()
        {
                _devCertSign = Certificates.InMemoryResourceUtility.GetInMemoryCertificate("sts_dev_certificate.pfx", "test1234");
            _safewhereTest_SFS = Certificates.InMemoryResourceUtility.GetInMemoryCertificate("SafewhereTest_SFS.pfx", "test1234");

            _config = TestConfiguration.Configuration;


            _config.ServiceProvider = new ServiceProvider
            {
                Id = "secure.inlogik.com",
                Server = "https://secure.inlogik.com",
                SigningCertificate = _devCertSign
            };

            _config.IdentityProviders = new IdentityProviders();

            _config.SigningAlgorithm = AlgorithmType.SHA256;
            _config.ServiceProvider.IncludeArtifactResolutionEndpoints = false;
            _config.ServiceProvider.AuthNAllowCreate = true;
            _config.ServiceProvider.UseValidUntil = false;
            _config.ServiceProvider.RequiredNameIdFormat = new NameIdFormat { Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent" };

            _config.ServiceProvider.NameIdFormats = new NameIdFormats()
            {
                new NameIdFormat { Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent" },
                new NameIdFormat { Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient" }
            };

            _config.ServiceProvider.Endpoints.Add(new ServiceProviderEndpoint(EndpointType.SignOn, "/auth/saml2", "", BindingType.Redirect, true));
            _config.ServiceProvider.Endpoints.Add(new ServiceProviderEndpoint(EndpointType.SignOn, "/auth/saml2", "", BindingType.Post, false));


            _config.IdentityProviders.AddByMetadataDirectory(Path.Combine("Protocol","MetadataDocs","FOBS")); // Set it manually.



        }

        public X509Certificate2 Sts_Dev_cetificate
        {
            get
            {
                return _devCertSign;
            }
        }

        public X509Certificate2 SafewhereTest_SFS
        {
            get
            {
                return _safewhereTest_SFS;
            }
        }

        public Saml2Configuration Config
        {
            get
            {
                return _config;
            }
        }
    }
}