using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SAMLSilly.Config;
using SAMLSilly.Utils;
using Xunit;

namespace SAMLSilly.Tests
{
    public class Saml20MetadataDocumentTests : IClassFixture<TestContext>
    {
        private readonly TestContext _context;

        public Saml20MetadataDocumentTests(TestContext context)
        {
            _context = context;
        }
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
            var metadata = new Saml20MetadataDocument().Load(_context.Config);

            Assert.NotEmpty(metadata.ToXml());
        }

        [Fact]
        public void Serialize_metadata_from_configuration_signed()
        {

            var cert = _context.Sts_Dev_cetificate;

            var metadata = new Saml20MetadataDocument().Load(_context.Config);


            Assert.NotEmpty(metadata.ToXml(null, _context.Config.ServiceProvider.SigningCertificate, _context.Config.SigningAlgorithm));
        }

        [Fact]

        public void Parse_metadata_from_xml()
        {
            var metadata = new Saml20MetadataDocument().Load(FileLoadUtils.GetStream(Path.Combine("Protocol","MetadataDocs","metadata-HAIKU.xml")));

        }

        [Fact]
        public void Parse_metadata_from_xml_test_idp()
        {
            var metadata = new Saml20MetadataDocument().Load(FileLoadUtils.GetStream(Path.Combine("Protocol","MetadataDocs","inlogik-test-adfs-metadata.xml")));
            var a = metadata.EntityId;
        }
    }
}