using System;
using System.IO;
using System.Xml;
using SAMLSilly.Utils;
using Xunit;

namespace SAMLSilly.Tests.Utils
{
    /// <summary>
    /// <see cref="XmlSignatureUtils"/> tests.
    /// </summary>

    public class XmlSignatureUtilsTest
    {
        /// <summary>
        /// Loads the document.
        /// </summary>
        /// <param name="assertionFile">The assertion file.</param>
        /// <returns>The XML document.</returns>
        private static XmlDocument LoadDocument(string assertionFile)
        {
            using (var fs = File.OpenRead(assertionFile))
            {
                var document = new XmlDocument { PreserveWhitespace = true };
                document.Load(fs);
                fs.Close();

                return document;
            }
        }

        /// <summary>
        /// CheckSignature method tests.
        /// </summary>

        public class CheckSignatureMethod
        {
            /// <summary>
            /// Verify valid signatures can be checked.
            /// </summary>
            [Fact]
            public void CanCheckValidSignatures()
            {
                // Arrange
                var doc = LoadDocument(Path.Combine("Assertions","Saml2Assertion_01"));

                // Act
                var result = XmlSignatureUtils.CheckSignature(doc);

                // Assert
                Assert.True(result);
            }
        }

        /// <summary>
        /// ExtractSignatureKeys method tests.
        /// </summary>

        public class ExtractSignatureKeysMethod
        {
            /// <summary>
            /// Verify signature keys can be extracted.
            /// </summary>
            [Fact]
            public void CanExtractKeyInfo()
            {
                // Arrange
                var doc = LoadDocument(Path.Combine("Assertions","Saml2Assertion_01"));

                // Act
                var keyInfo = XmlSignatureUtils.ExtractSignatureKeys(doc);

                // Assert
                Assert.NotNull(keyInfo);
            }
        }

        /// <summary>
        /// IsSigned method tests.
        /// </summary>

        public class IsSignedMethod
        {
            /// <summary>
            /// Verify signed and unsigned documents can be detected.
            /// </summary>
            [Fact]
            public void CanDetectIfDocumentIsSigned()
            {
                // Arrange
                var badDocument = LoadDocument(Path.Combine("Assertions","EncryptedAssertion_01"));
                var goodDocument = LoadDocument(Path.Combine("Assertions","Saml2Assertion_01"));

                // Act
                var badResult = XmlSignatureUtils.IsSigned(badDocument);
                var goodResult = XmlSignatureUtils.IsSigned(goodDocument);

                // Assert
                Assert.False(badResult);
                Assert.True(goodResult);
            }

            /// <summary>
            /// Verify documents without preserve whitespace set will fail.
            /// </summary>
            [Fact]
            public void FailsOnDocumentWithoutPreserveWhitespace()
            {
                // Arrange
                var doc = LoadDocument(Path.Combine("Assertions","EncryptedAssertion_01"));
                doc.PreserveWhitespace = false;

                // Act
                Assert.Throws(typeof(InvalidOperationException), () =>
                {

                    XmlSignatureUtils.IsSigned(doc);

                    // Assert
                    //Assert.Fail("Signed documents that do not have PreserveWhitespace set should fail to be processed.");
                });
            }
        }
    }
}
