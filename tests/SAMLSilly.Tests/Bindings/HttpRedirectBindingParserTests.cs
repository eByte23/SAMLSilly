using System;
using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SAMLSilly.AspNetCore.BindingBuilders;
using SAMLSilly.Bindings;
using SAMLSilly.Utils;
using Xunit;

namespace SAMLSilly.Tests.Bindings
{
    /// <summary>
    /// <see cref="HttpRedirectBindingParser"/> tests.
    /// </summary>

    public class HttpRedirectBindingParserTests
    {
        /// <summary>
        /// Performs a simple split of an Url query, and stores the result in a NameValueCollection.
        /// This method may fail horribly if the query string is not correctly URL-encoded.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="NameValueCollection"/> that results from parsing the query.</returns>
        private static NameValueCollection QueryToNameValueCollection(string request)
        {
            if (request[0] == '?')
            {
                request = request.Substring(1);
            }

            var result = new NameValueCollection();
            foreach (var s in request.Split('&'))
            {
                var keyvalue = s.Split('=');
                result.Add(keyvalue[0], keyvalue[1]);
            }

            return result;
        }

        /// <summary>
        /// Constructor method tests.
        /// </summary>

        public class ConstructorMethod
        {
            /// <summary>
            /// Verify that basic encoding and decoding of a Request works.
            /// Verify that the parser correctly detects a Request parameter.
            /// </summary>
            [Fact]
            public void ParserCanEncodeAndDecodeRequest()
            {
                // Arrange
                var request = string.Empty.PadLeft(350, 'A') + "������";
                var bindingBuilder = new HttpRedirectBindingBuilder() { Request = request };

                var query = bindingBuilder.ToQuery();
                var coll = QueryToNameValueCollection(query);
                var url = new Uri("http://localhost/?" + query);

                // Act
                var bindingParser = new HttpRedirectBindingParser(BindingUtility.QueryStringToKeyValuePair(url.Query));

                // Assert
                Assert.True(coll.Count == 1);
                Assert.True(bindingParser.IsRequest);
                Assert.True(!bindingParser.IsResponse);
                Assert.True(!bindingParser.IsSigned);
                Assert.Equal(request, bindingParser.Message);
            }

            /// <summary>
            /// Verify that basic encoding and decoding of a RelayState works.
            /// </summary>
            [Fact]
            public void ParserCanEncodeAndDecodeRequestWithRelayState()
            {
                // Arrange
                var request = string.Empty.PadRight(140, 'l');
                var relaystate = "A relaystate test. @@@!!!&&&///";

                var bindingBuilder = new HttpRedirectBindingBuilder()
                {
                    Request = request,
                    RelayState = relaystate
                };

                var query = bindingBuilder.ToQuery();
                var coll = QueryToNameValueCollection(query);
                var url = new Uri("http://localhost/?" + query);

                // Act
                var bindingParser = new HttpRedirectBindingParser(BindingUtility.QueryStringToKeyValuePair(url.Query));

                // Assert
                Assert.Equal(2, coll.Count);
                Assert.True(bindingParser.IsRequest);
                Assert.False(bindingParser.IsResponse);
                Assert.False(bindingParser.IsSigned);
                Assert.NotNull(bindingParser.RelayState);
                Assert.Equal(relaystate, bindingParser.RelayStateDecoded);
                Assert.Equal(request, bindingParser.Message);
            }

            /// <summary>
            /// Uses a DSA key to sign and verify the Authentication request.
            /// </summary>
            [Fact(Skip = "new ctor param hmmmmm")]
            public void ParserCanSignAuthnRequestWithDsaKey()
            {
                // Arrange
                var key = new DSACryptoServiceProvider();
                var evilKey = new DSACryptoServiceProvider();

                var binding = new HttpRedirectBindingBuilder()
                {
                    Request = string.Empty.PadLeft(500, 'a'),
                    //SigningCertificate = key
                };

                var url = new Uri("http://localhost/?" + binding.ToQuery());

                // Act
                var parser = new HttpRedirectBindingParser(BindingUtility.QueryStringToKeyValuePair(url.Query));

                // Assert
                Assert.True(parser.IsSigned);
                Assert.True(parser.IsRequest);
                Assert.True(parser.CheckSignature(key));
                Assert.False(parser.CheckSignature(evilKey));
            }

            /// <summary>
            /// Uses a RSA key to sign and verify the Authentication request.
            /// </summary>
            [Fact(Skip = "new ctor param hmmmmmm")]
            public void ParserCanSignAuthnRequestWithRsaKey()
            {
                // Arrange
                var key = new RSACryptoServiceProvider();
                var evilKey = new RSACryptoServiceProvider();

                var binding = new HttpRedirectBindingBuilder()
                {
                    Request = string.Empty.PadLeft(500, 'a'),
                    //SigningCertificate = key
                };

                var url = new Uri("http://localhost/?" + binding.ToQuery());

                // Act
                var parser = new HttpRedirectBindingParser(BindingUtility.QueryStringToKeyValuePair(url.Query));

                // Assert
                Assert.True(parser.IsSigned);
                Assert.True(parser.IsRequest);
                Assert.True(!parser.IsResponse);
                Assert.True(parser.CheckSignature(key));
                Assert.False(parser.CheckSignature(evilKey));
            }
        }

        /// <summary>
        /// CheckSignature method tests.
        /// </summary>

        public class CheckSignatureMethod
        {
            /// <summary>
            /// Tests an actual Ping Federate signed response response can be parsed.
            /// </summary>
            [Fact]
            public void ParserCanParsePingFederateSignedResponse()
            {
                // Arrange
                var url = new Uri("http://haiku.safewhere.local/Saml20TestWeb/SSOLogout.saml2.aspx?SAMLResponse=fZFRa8IwEMe%2FSsm7bZq2qMEWZN1DwSEY0eGLpGmqZTUpuYTpt19bGVMYPob7%2FX%2BXu1sAv7QdXemTdnYjodMKpJdLsI3ittEqRWdrOxoEZ958OR94Lb%2FP0ki%2F1YK3AevjBG97fi%2FLgLH13eQPWuJz6K7IK9SveKtT1FQ1qYSoSSRIVMVhPJ3hpMQRj6IwKUVcJn0CwMlCgeXKpohgPJvgaILjbUhoQmg49cl8dui5PEWH%2BoaB6P2arAtlq%2FqWF%2FNTXn8y3yBvJw2MQxAfI%2B96aRXQceIUOaOo5tAAVfwigVpB2fJjRXuSdkZbLXSLssVA0%2FE%2F5iH%2FOs4BpBmWh7JlvnrfHIcKwcciXwQPvru8o8xy6%2BD59aYr6e146%2BTrVjDSlDkhJAAKsnuHP2nw34GzHw%3D%3D&SigAlg=http%3A%2F%2Fwww.w3.org%2F2000%2F09%2Fxmldsig%23rsa-sha1&Signature=UoYGLeSCYOSvjIaBpTcgtq2O0Nbz%2BVk%2BaaLESje8%2FZKxGNmWrFXJjSPrA403J23NeQzbxxVgOwSP8idIM95BhlVwxpiG%2B7%2FhJyNNrjGPohmD3cQpBWoWqZ8IEudDc%2FwDCshPb6wTdr6%2FOdKXQ2uwSK5NA2LYI8AAN5sq9kPtVvk%3D");
                var parser = new HttpRedirectBindingParser(BindingUtility.QueryStringToKeyValuePair(url.Query));
                var cert = new X509Certificate2(Path.Combine("Certificates","pingcertificate.crt"));

                // Act
                var result = parser.CheckSignature(cert.PublicKey.Key);

                // Assert
                Assert.True(result);
            }

            /// <summary>
            /// Tests an actual Ping Federate signed request response can be parsed.
            /// </summary>
            [Fact]
            public void ParserCanParsePingFederateSignedRequest()
            {
                // Arrange
                //There seems to be and issue here.
                //The Uri.DataEscapeString() now uppercases the char so can cause invalid errors
                var url = new Uri("https://adler.safewhere.local:9031/idp/SSO.saml2?SAMLResponse=lJHNTsMwEITvSLyD5XsbO85PYyWpIjgQCS6l4sDNSVzVorWJ10Y8Pg4ppeoBievMzrej3XL9eTygD2lBGV1huiR4Xd%2FelCPljXd7vZGjl%2BBQe19hNaRxUeR5ypIuFgkb%2BoKwLB1Y2uVk18UritHLDykOJNQCeNlqcEK7IBGyWhC2IMmWJpxRzrJlnqWkYPErRqGHBj7SCnuruRGggGtxlMBdz5%2Bbp0cekPzdGmd6c8ChJELl9wI7Z%2F8OCgBpXaiG62nsQag3X0ZzfmbdGT2oaQL%2ByZvSId%2F4QUndy024l1X95J2sC%2FNy%2BVk7Ac7CNaGMfrtNz4muvlN%2FAQAA%2F%2F8DAA%3D%3D&SigAlg=http%3A%2F%2Fwww.w3.org%2F2000%2F09%2Fxmldsig%23rsa-sha1&Signature=Ioj0MxlpQ2yhcJjyZAmSDXdWyk6yBosyIMLPw%2ByYSM40B4x%2Fh%2BrvfryOamBdfQTEH23nmFCRQABe11gCVdrGAFK4IEVnL%2BwszFHI12Sgl%2FREU8e96hgdUA2%2BDk9hyg3VD4wk5vnnQER6Cxb6ZLvGrnSTEHB4tqq0f6QeVKxnH90%3D");
                var parser = new HttpRedirectBindingParser(BindingUtility.QueryStringToKeyValuePair(url.Query));
                var cert = Certificates.InMemoryResourceUtility.GetInMemoryCertificate("SafewhereTest_SFS.pfx", "test1234");

                // Act
                var result = parser.CheckSignature(cert.PublicKey.Key);
                // Assert
                Assert.True(result);
            }

            /// <summary>
            /// Verify parser throws exception on trying to verify signature of unsigned request.
            /// </summary>
            [Fact]
            public void ParserThrowsExceptionWhenTryingToVerifySignatureOfUnsignedRequest()
            {
                // Arrange
                var request = string.Empty.PadLeft(350, 'A') + "������";
                var bindingBuilder = new HttpRedirectBindingBuilder() { Request = request };

                var query = bindingBuilder.ToQuery();
                var url = new Uri("http://localhost/?" + query);
                var bindingParser = new HttpRedirectBindingParser(BindingUtility.QueryStringToKeyValuePair(url.Query));


                // Assert
                Assert.Throws(typeof(InvalidOperationException), () =>
                {
                    // Act
                    bindingParser.CheckSignature(new RSACryptoServiceProvider());
                });
            }
        }
    }
}
