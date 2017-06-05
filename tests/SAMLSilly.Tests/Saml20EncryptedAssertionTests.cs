using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using SAMLSilly.Config;
using SAMLSilly.Schema.Core;
using SAMLSilly.Schema.Protocol;
using Xunit;

namespace SAMLSilly.Tests
{
    /// <summary>
    /// <see cref="Saml20EncryptedAssertion"/> tests.
    /// </summary>

    public class Saml20EncryptedAssertionTests : IClassFixture<TestContext>
    {
        private readonly TestContext _context;

        public Saml20EncryptedAssertionTests(TestContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Tests that it is possible to specify the algorithm of the session key.
        /// Steps:
        /// - Create a new encrypted assertion with a specific session key algorithm that is different than the default.
        /// - Decrypt the assertion and verify that it uses the correct algorithm.
        /// - Verify that the SessionKeyAlgorithm property behaves as expected.
        /// </summary>
        //TODO: @ebyte this fails sometimes
        [Fact]
        public void CanEncryptAssertionFull()
        {
            // Arrange
            var encryptedAssertion = new Saml20EncryptedAssertion
            {
                SessionKeyAlgorithm = EncryptedXml.XmlEncAES128Url,
                Assertion = AssertionUtil.GetTestAssertion()
            };

            encryptedAssertion.TransportKey = (RSA)_context.Sts_Dev_cetificate.PublicKey.Key;

            // Act
            encryptedAssertion.Encrypt();
            var encryptedAssertionXml = encryptedAssertion.GetXml();

            // Now decrypt the assertion, and verify that it recognizes the Algorithm used.
            var decrypter = new Saml20EncryptedAssertion((RSA)_context.Sts_Dev_cetificate.PrivateKey);
            decrypter.LoadXml(encryptedAssertionXml.DocumentElement);

            // Set a wrong algorithm and make sure that the class gets it algorithm info from the assertion itself.
            decrypter.SessionKeyAlgorithm = EncryptedXml.XmlEncTripleDESUrl;
            decrypter.Decrypt();

            // Assert
            // Go through the children and look for the EncryptionMethod element, and verify its algorithm attribute.
            var encryptionMethodFound = false;
            foreach (XmlNode node in encryptedAssertionXml.GetElementsByTagName(Schema.XEnc.EncryptedData.ElementName, Saml20Constants.Xenc)[0].ChildNodes)
            {
                if (node.LocalName == Schema.XEnc.EncryptionMethod.ElementName && node.NamespaceURI == Saml20Constants.Xenc)
                {
                    var element = (XmlElement)node;
                    Assert.Equal(EncryptedXml.XmlEncAES128Url, element.GetAttribute("Algorithm"));
                    encryptionMethodFound = true;
                }
            }

            Assert.True(encryptionMethodFound, "Unable to find EncryptionMethod element in EncryptedData.");

            // Verify that the class has discovered the correct algorithm and set the SessionKeyAlgorithm property accordingly.
            Assert.Equal(EncryptedXml.XmlEncAES128Url, decrypter.SessionKeyAlgorithm);
            Assert.NotNull(decrypter.Assertion);
        }

        /// <summary>
        /// Verify there is no assertion after initialization but before decryption.
        /// </summary>
        [Fact]
        public void HasNoAssertionBeforeDecrypt()
        {
            // Arrange
            var doc = AssertionUtil.LoadXmlDocument(@"Assertions\EncryptedAssertion_01");

            // Act
            var encryptedAssertion = new Saml20EncryptedAssertion((RSA)_context.Sts_Dev_cetificate.PrivateKey, doc);

            // Assert
            Assert.Null(encryptedAssertion.Assertion);
        }

        /// <summary>
        /// Decrypt method tests.
        /// </summary>
        public class DecryptMethod : IClassFixture<TestContext>
        {
            private readonly TestContext _context;

            public DecryptMethod(TestContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Attempts to decrypt the assertion in the file "EncryptedAssertion_01".
            /// </summary>
            [Fact]
            public void CanDecryptAssertion()
            {
                // Arrange
                var doc = AssertionUtil.LoadXmlDocument(@"Assertions\EncryptedAssertion_01");
                var encryptedAssertion = new Saml20EncryptedAssertion((RSA)_context.Sts_Dev_cetificate.PrivateKey, doc);

                // Act
                encryptedAssertion.Decrypt();
                var assertion = new Saml20Assertion(encryptedAssertion.Assertion.DocumentElement, null, false, TestConfiguration.Configuration);

                // Assert
                Assert.NotNull(encryptedAssertion.Assertion);
            }

            /// <summary>
            /// Test that the <code>Saml20EncryptedAssertion</code> class is capable of finding keys that are "peer" included,
            /// i.e. the &lt;EncryptedKey&gt; element is a sibling of the &lt;EncryptedData&gt; element.
            /// </summary>
            [Fact]
            public void CanDecryptAssertionWithPeerIncludedKeys()
            {
                // Arrange
                var doc = AssertionUtil.LoadXmlDocument(@"Assertions\EncryptedAssertion_02");
                var encryptedAssertion = new Saml20EncryptedAssertion((RSA)_context.Sts_Dev_cetificate.PrivateKey, doc);

                // Act
                encryptedAssertion.Decrypt();

                // Assert
                Assert.NotNull(encryptedAssertion.Assertion);
            }

            /// <summary>
            /// Test that the <code>Saml20EncryptedAssertion</code> class is capable using 3DES keys for the session key and OAEP-padding for
            /// the encryption of the session key.
            /// </summary>
            [Fact]
            public void CanDecryptAssertionWithPeerIncluded3DesKeys()
            {
                // Arrange
                var doc = AssertionUtil.LoadXmlDocument(@"Assertions\EncryptedAssertion_04");
                var encryptedAssertion = new Saml20EncryptedAssertion((RSA)_context.Sts_Dev_cetificate.PrivateKey, doc);

                // Act
                encryptedAssertion.Decrypt();

                // Assert
                Assert.NotNull(encryptedAssertion.Assertion);
                Assert.Equal(1, encryptedAssertion.Assertion.GetElementsByTagName(Assertion.ElementName, Saml20Constants.Assertion).Count);
            }

            /// <summary>
            /// Test that the <code>Saml20EncryptedAssertion</code> class is capable using AES keys for the session key and OAEP-padding for
            /// the encryption of the session key.
            /// </summary>
            [Fact]
            public void CanDecryptAssertionWithPeerIncludedAesKeys()
            {
                // Arrange
                var doc = AssertionUtil.LoadXmlDocument(@"Assertions\EncryptedAssertion_05");
                var encryptedAssertion = new Saml20EncryptedAssertion((RSA)_context.Sts_Dev_cetificate.PrivateKey, doc);

                // Act
                encryptedAssertion.Decrypt();

                // Assert
                Assert.NotNull(encryptedAssertion.Assertion);
                Assert.Equal(1, encryptedAssertion.Assertion.GetElementsByTagName(Assertion.ElementName, Saml20Constants.Assertion).Count);
            }

            /// <summary>
            /// Test that the <code>Saml20EncryptedAssertion</code> class is capable of finding keys that are "peer" included,
            /// i.e. the &lt;EncryptedKey&gt; element is a sibling of the &lt;EncryptedData&gt; element, and the assertion does
            /// not specify an encryption method.
            /// </summary>
            [Fact]
            public void CanDecryptAssertionWithPeerIncludedKeysWithoutSpecifiedEncryptionMethod()
            {
                // Arrange
                var doc = AssertionUtil.LoadXmlDocument(@"Assertions\EncryptedAssertion_03");
                var encryptedAssertion = new Saml20EncryptedAssertion((RSA)_context.Sts_Dev_cetificate.PrivateKey, doc);

                // Act
                encryptedAssertion.Decrypt();

                // Assert
                Assert.NotNull(encryptedAssertion.Assertion);
            }

            /// <summary>
            /// Decrypts an assertion we received.
            /// </summary>
            /// <remarks>
            /// The entire message is Base 64 encoded in this case.
            /// </remarks>
            [Fact]
            //[ExpectedException(typeof(Saml20Exception), ExpectedMessage = "Assertion is no longer valid.")]
            public void CanDecryptFOBSAssertion()
            {
                // Arrange
                var doc = AssertionUtil.LoadBase64EncodedXmlDocument(@"Assertions\fobs-assertion2");
                var encryptedList = doc.GetElementsByTagName(EncryptedAssertion.ElementName, Saml20Constants.Assertion);

                // Do some mock configuration.
                var config = new Saml2Configuration
                {
                    AllowedAudienceUris = new System.Collections.Generic.List<Uri>(),
                    IdentityProviders = new IdentityProviders()
                };
                config.AllowedAudienceUris.Add(new Uri("https://saml.safewhere.net"));
                config.IdentityProviders.AddByMetadataDirectory(@"Protocol\MetadataDocs\FOBS"); // Set it manually.

                var cert = _context.SafewhereTest_SFS;
                var encryptedAssertion = new Saml20EncryptedAssertion((RSA)cert.PrivateKey);

                encryptedAssertion.LoadXml((XmlElement)encryptedList[0]);

                // Act
                encryptedAssertion.Decrypt();

                // Retrieve metadata
                var assertion = new Saml20Assertion(encryptedAssertion.Assertion.DocumentElement, null, false, TestConfiguration.Configuration);
                var endp = config.IdentityProviders.FirstOrDefault(x => x.Id == assertion.Issuer);

                // Assert
                Assert.True(encryptedList.Count == 1);
                Assert.NotNull(endp);//, "Endpoint not found");
                Assert.NotNull(endp.Metadata);//, "Metadata not found");

                Assert.Throws(typeof(InvalidOperationException), () =>
                {
                    assertion.CheckValid(AssertionUtil.GetTrustedSigners(assertion.Issuer));
                    //Assert.Fail("Verification should fail. Token does not include its signing key.");
                });

                Assert.Null(assertion.SigningKey); //, "Signing key is already present on assertion. Modify test.");
                //Assert.IsTrue("We have tested this next test" == "");
                //Assert.True(assertion.CheckSignature(Saml20SignonHandler.GetTrustedSigners(endp.Metadata.GetKeys(KeyTypes.Signing), endp)));
                //Assert.IsNotNull(assertion.SigningKey, "Signing key was not set on assertion instance.");
            }
        }

        /// <summary>
        /// Encrypt method tests.
        /// </summary>

        public class EncrypteMethod : IClassFixture<TestContext>
        {
            private readonly TestContext _context;

            public EncrypteMethod(TestContext context)
            {
                _context = context;
            }
            /// <summary>
            /// Verify that assertions can be encrypted.
            /// </summary>
            [Fact]
            public void CanEncryptAssertion()
            {
                // Arrange
                var encryptedAssertion = new Saml20EncryptedAssertion { Assertion = AssertionUtil.GetTestAssertion() };
                encryptedAssertion.TransportKey = (RSA)_context.Sts_Dev_cetificate.PublicKey.Key;

                // Act
                encryptedAssertion.Encrypt();
                var encryptedAssertionXml = encryptedAssertion.GetXml();

                // Assert
                Assert.NotNull(encryptedAssertionXml);
                Assert.Equal(1, encryptedAssertionXml.GetElementsByTagName(EncryptedAssertion.ElementName, Saml20Constants.Assertion).Count);
                Assert.Equal(1, encryptedAssertionXml.GetElementsByTagName(Schema.XEnc.EncryptedKey.ElementName, Saml20Constants.Xenc).Count);
            }
        }

        /// <summary>
        /// SessionKeyAlgorithm property tests.
        /// </summary>

        public class SessionKeyAlgorithmProperty
        {
            /// <summary>
            /// Verify that exception is thrown on incorrect algorithm URI being passed in.
            /// </summary>
            [Fact]
            public void ThrowsArgumentExceptionOnIncorrectAlgorithmUri()
            {

                Assert.Throws(typeof(ArgumentException), () =>
                {
                    // Act
                    var encryptedAssertion = new Saml20EncryptedAssertion { SessionKeyAlgorithm = "RSA" };
                });
                // Assert
                //Assert.Fail("\"Saml20EncryptedAssertion\" class does not respond to incorrect algorithm identifying URI.");
            }
        }
    }
}
