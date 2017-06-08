using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SAMLSilly.Config;
using Xunit;

namespace SAMLSilly.Tests
{
    public class Saml20MetadataDocumentTests
    {
        //These are to fix our api and make it not shit

        [Fact]
        public void Serialize_metadata_to_xml_empty_constructor_should_throw_exception()
        {

            var metadata = new Saml20MetadataDocument();

            Assert.Throws<System.InvalidOperationException>(() => metadata.ToXml());
        }


        [Fact]
        public void Serialize_metatdata_from_configuration_not_signed()
        {
            var config = new Saml2Configuration()
            {

                AllowedAudienceUris = new System.Collections.Generic.List<Uri>(),
                IdentityProviders = new IdentityProviders(),
                ServiceProvider = new ServiceProvider
                {
                    Id = "secure.inlogik.com",
                    Server = "https://secure.inlogik.com",
                    SigningCertificate = new X509Certificate2(@"Certificates\SafewhereTest_SFS.pfx", "test1234"),

                },

            };
            config.ServiceProvider.IncludeArtifactResolutionEndpoints = false;
            config.ServiceProvider.AuthNAllowCreate = true;
            config.ServiceProvider.UseValidUntil = false;
            config.ServiceProvider.NameIdFormats = new NameIdFormats()
            {

            };

            config.ServiceProvider.NameIdFormats.AddRange(new[]
            {
                new NameIdFormat { Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent" },
                new NameIdFormat { Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient" }
            });


            config.AllowedAudienceUris.Add(new Uri("https://saml.safewhere.net"));
            config.IdentityProviders.AddByMetadataDirectory(@"Protocol\MetadataDocs\FOBS"); // Set it manually.

            var metadata = new Saml20MetadataDocument().Load(config);

            Assert.NotEmpty(metadata.ToXml());
        }

        [Fact]
        public void Serialize_metadata_from_configuration_signed()
        {

            var config = new Saml2Configuration()
            {

                AllowedAudienceUris = new System.Collections.Generic.List<Uri>(),
                IdentityProviders = new IdentityProviders(),
                ServiceProvider = new ServiceProvider
                {
                    Id = "secure.inlogik.com",
                    Server = "https://secure.inlogik.com",
                    SigningCertificate = new X509Certificate2(@"Certificates\SafewhereTest_SFS.pfx", "test1234"),

                },

            };
            config.ServiceProvider.IncludeArtifactResolutionEndpoints = false;
            config.ServiceProvider.AuthNAllowCreate = true;
            config.ServiceProvider.UseValidUntil = false;
            config.ServiceProvider.NameIdFormats = new NameIdFormats()
            {

            };

            config.ServiceProvider.NameIdFormats.AddRange(new[]
            {
                new NameIdFormat { Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent" },
                new NameIdFormat { Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient" }
            });


            config.AllowedAudienceUris.Add(new Uri("https://saml.safewhere.net"));
            config.IdentityProviders.AddByMetadataDirectory(@"Protocol\MetadataDocs\FOBS"); // Set it manually.

            var cert = new X509Certificate2(@"Certificates\SafewhereTest_SFS.pfx", "test1234");
            //var encryptedAssertion = new Saml20EncryptedAssertion((RSA)cert.PrivateKey);

            var metadata = new Saml20MetadataDocument().Load(config);

            Assert.NotEmpty(metadata.ToXml(null, config.ServiceProvider.SigningCertificate));
        }
    }
}