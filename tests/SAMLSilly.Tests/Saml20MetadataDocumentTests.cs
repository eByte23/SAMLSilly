using System;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Xunit;
using SAMLSilly.Utils;
using SAMLSilly.Schema.Metadata;

namespace SAMLSilly.Tests
{
    /// <summary>
    /// <see cref="Saml20MetadataDocument"/> tests.
    /// </summary>

    public class Saml20MetadataDocumentTests
    {
        /// <summary>
        /// Constructor tests.
        /// </summary>

        public class ConstructorMethod
        {
            /// <summary>
            /// Verify that certificates can be extracted.
            /// </summary>
            [Fact]
            public void CanExtractCertificates()
            {
                // Arrange
                var doc = new XmlDocument { PreserveWhitespace = true };
                doc.Load(@"Protocol\MetadataDocs\metadata-ADLER.xml");

                // Act
                var metadata = new Saml20MetadataDocument(doc);
                var certificateCheckResult = XmlSignatureUtils.CheckSignature(doc, (KeyInfo)metadata.Keys[0].KeyInfo);

                // Assert
                Assert.True(metadata.GetKeys(KeyTypes.Signing).Count == 1);
                Assert.True(metadata.GetKeys(KeyTypes.Encryption).Count == 1);
                Assert.True(metadata.Keys[0].Use == KeyTypes.Signing);
                Assert.True(metadata.Keys[1].Use == KeyTypes.Encryption);

                // The two certs in the metadata document happen to be identical, and are also
                // used for signing the entire document.
                // Extract the certificate and verify the document.
                Assert.True(certificateCheckResult);
                Assert.Equal("ADLER_SAML20_ID", metadata.EntityId);
            }

            /// <summary>
            /// Verify that certificates can be extracted.
            /// </summary>
            [Fact]
            public void CanExtractCertificatesOnStream()
            {
                Saml20MetadataDocument metadata;
                // Arrange
                using (var ms = new MemoryStream())
                {
                    using (var reader = File.OpenText(@"Protocol\MetadataDocs\metadata-ADLER.xml"))
                    {
                        reader.BaseStream.CopyTo(ms);
                        reader.Close();
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    metadata = new Saml20MetadataDocument(ms, null);
                }

                // Assert
                Assert.True(metadata.GetKeys(KeyTypes.Signing).Count == 1);
                Assert.True(metadata.GetKeys(KeyTypes.Encryption).Count == 1);
                Assert.True(metadata.Keys[0].Use == KeyTypes.Signing);
                Assert.True(metadata.Keys[1].Use == KeyTypes.Encryption);

                Assert.Equal("ADLER_SAML20_ID", metadata.EntityId);
            }

            /// <summary>
            /// Verify that IDP endpoints can be extracted.
            /// </summary>
            [Fact]
            public void CanExtractEndpoints()
            {
                // Arrange
                var doc = new XmlDocument { PreserveWhitespace = true };
                doc.Load(@"Protocol\MetadataDocs\metadata-ADLER.xml");

                // Act
                var metadata = new Saml20MetadataDocument(doc);

                // Assert
                Assert.Equal(2, metadata.IDPSLOEndpoints.Count);
                Assert.Equal(2, metadata.SSOEndpoints.Count);
            }
        }

        /// <summary>
        /// ToXml method tests.
        /// </summary>

        public class ToXmlMethod
        {
            /// <summary>
            /// Sign an &lt;EntityDescriptor&gt; metadata element.
            /// </summary>
            /// <remakrs>
            /// This requires that the configured signing certificate for tests is in the local store!
            /// </remakrs>
            [Fact]
            public void SignsXml()
            {
                // Arrange
                var doc = new Saml20MetadataDocument(true);
                var entity = doc.CreateDefaultEntity();
                entity.ValidUntil = DateTime.Now.AddDays(14);

                var certificate = new X509Certificate2(FileEmbeddedResource("SAMLSilly.Tests.Certificates.sts_dev_certificate.pfx"), "test1234");
                // Act
                var metadata = doc.ToXml(null, certificate);
                var document = new XmlDocument { PreserveWhitespace = true };
                document.LoadXml(metadata);
                var result = XmlSignatureUtils.CheckSignature(document);

                // Assert
                Assert.True(result);
            }

            private byte[] FileEmbeddedResource(string path)
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = path;

                byte[] result = null;
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    result = memoryStream.ToArray();
                }
                return result;
            }
        }


        public class LoadingMetadata
        {
            [Fact]
            public void LoadMetadataFromUrl()
            {
                var doc = new XmlDocument { PreserveWhitespace = true };
                doc.Load(@"https://sts.newcrest.com.au/federationmetadata/2007-06/federationmetadata.xml");

                // Act
                var metadata = new Saml20MetadataDocument(doc);

                // Assert
                Assert.Equal(2, metadata.IDPSLOEndpoints.Count);
                Assert.Equal(2, metadata.SSOEndpoints.Count);
            }

            [Fact(Skip ="TODO: @ebyte23 come back and make a valid test")]
            public void TestMetadataloadWithNoUseTypeSigning()
            {
                var doc = new XmlDocument { PreserveWhitespace = true };
                doc.Load(@"Protocol\MetadataDocs\rmit-metadata.xml");

                // Act
                var metadata = new Saml20MetadataDocument(doc);
                //var l  = XmlSignatureUtils.CheckSignature(doc);
                //var a = "";
            }

        }
    }
}
