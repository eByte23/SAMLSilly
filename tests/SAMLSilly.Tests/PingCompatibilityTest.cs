//TODO: @ebyte23 find out what this is and if we can delete it completely

//using System;
//using System.IO;
//using System.Security.Cryptography;
//using System.Security.Cryptography.X509Certificates;
//using System.Xml;
//using SAMLSilly;
//using SAMLSilly.Schema.Core;
//using SAMLSilly.Schema.Protocol;
//using SAMLSilly.Tests;
//using Xunit;

//namespace SAML2.DotNet35.Tests
//{
//    /// <summary>
//    /// This class contains tests that can only be used when a Ping Identity server is running.
//    /// </summary>

//    public class PingCompatibilityTest
//    {
//        /// <summary>
//        /// Decrypts the ping assertion.
//        /// </summary>
//        [Fact]
//        public void DecryptPingAssertion()
//        {
//            // Load the assertion
//            var doc = new XmlDocument();
//            doc.Load(File.OpenRead(@"c:\tmp\pingassertion.txt"));

//            var xe = GetElement(EncryptedAssertion.ElementName, Saml20Constants.Assertion, doc);

//            var doc2 = new XmlDocument();
//            doc2.AppendChild(doc2.ImportNode(xe, true));

//            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
//            store.Open(OpenFlags.ReadOnly);
//            X509Certificate2Collection coll = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName,
//                                                                      "CN=SafewhereTest_SFS, O=Safewhere, C=DK",
//                                                                      true);

//            Assert.True(coll.Count == 1);

//            var cert = coll[0];

//            var encass = new Saml20EncryptedAssertion((RSA)cert.PrivateKey, doc2);

//            encass.Decrypt();

//            var writer = new XmlTextWriter(Console.Out)
//                             {
//                                 Formatting = Formatting.Indented,
//                                 Indentation = 3,
//                                 IndentChar = ' '
//                             };

//            encass.Assertion.WriteTo(writer);
//            writer.Flush();

//            var assertion = new Saml20Assertion(encass.Assertion.DocumentElement, AssertionUtil.GetTrustedSigners(encass.Assertion.Attributes["Issuer"].Value), false, TestConfiguration.Configuration);

//            Assert.NotNull(encass.Assertion);

//            Console.WriteLine();
//            foreach (SamlAttribute attribute in assertion.Attributes)
//            {
//                Console.WriteLine(attribute.Name + " : " + attribute.AttributeValue[0]);
//            }
//        }

//        /// <summary>
//        /// Gets the specified element.
//        /// </summary>
//        /// <param name="element">The element.</param>
//        /// <param name="ns">The ns.</param>
//        /// <param name="doc">The doc.</param>
//        /// <returns>The specified element from the document.</returns>
//        private static XmlElement GetElement(string element, string ns, XmlDocument doc)
//        {
//            var list = doc.GetElementsByTagName(element, ns);
//            Assert.True(list.Count == 1);

//            return (XmlElement)list[0];
//        }
//    }
//}