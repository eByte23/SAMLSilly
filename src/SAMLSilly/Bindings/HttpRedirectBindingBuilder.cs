using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using CONSTS = SAMLSilly.Bindings.HttpRedirectBindingConstants;
using SAMLSilly.Bindings;
using SAMLSilly.Utils;
using SAMLSilly.Config;
using System.Security.Cryptography.X509Certificates;

namespace SAMLSilly.AspNetCore.BindingBuilders
{
    /// <summary>
    /// Handles the creation of redirect locations when using the HTTP redirect binding, which is outlined in [SAMLBind]
    /// section 3.4.
    /// </summary>
    public class HttpRedirectBindingBuilder
    {
        /// <summary>
        /// SAML configuration field
        /// </summary>
        private AlgorithmType _signingAlgorithm;

        /// <summary>
        /// Request backing field.
        /// </summary>
        private string _request;

        /// <summary>
        /// Response backing field.
        /// </summary>
        private string _response;

        /// <summary>
        /// SigningKey backing field.
        /// </summary>
        private X509Certificate2 _signingCertificate;

        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        /// <value>The request.</value>
        public string Request
        {
            get { return _request; }
            set
            {
                if (!string.IsNullOrEmpty(_response))
                {
                    throw new ArgumentException("Response property is already specified. Unable to set Request property.");
                }

                _request = value;
            }
        }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>The response.</value>
        public string Response
        {
            get { return _response; }
            set
            {
                if (!string.IsNullOrEmpty(_request))
                {
                    throw new ArgumentException("Request property is already specified. Unable to set Response property.");
                }

                _response = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the relay state of the message.</para>
        /// <para>If the message being built is a response message, the relay state will be included unmodified.</para>
        /// <para>If the message being built is a request message, the relay state will be encoded and compressed before being included.</para>
        /// </summary>
        public string RelayState { get; set; }

        /// <summary>
        /// Gets or sets the signing key.
        /// </summary>
        /// <value>The signing key.</value>
        public X509Certificate2 SigningCertificate
        {
            get { return _signingCertificate; }
            set
            {
                // Check if the key is of a supported type. [SAMLBind] sect. 3.4.4.1 specifies this.
                if (!(value.PrivateKey is RSA || value.PrivateKey is DSA || value.PrivateKey == null))
                {
                    throw new ArgumentException("Signing key must be an instance of either RSACryptoServiceProvider or DSA.");
                }

                _signingCertificate = value;
            }
        }

        public AlgorithmType SigningAlgorithm { get => _signingAlgorithm; set => _signingAlgorithm = value; }

        /// <summary>
        /// Returns the query part of the url that should be redirected to.
        /// The resulting string should be pre-pended with either ? or &amp; before use.
        /// </summary>
        /// <returns>The query string part of the redirect URL.</returns>
        public string ToQuery()
        {
            var result = new StringBuilder();

            AddMessageParameter(result);
            AddRelayState(result);
            AddSignature(result);

            return result.ToString();
        }

        /// <summary>
        /// If the RelayState property has been set, this method adds it to the query string.
        /// </summary>
        /// <param name="result">The result.</param>
        private void AddRelayState(StringBuilder result)
        {
            if (RelayState == null)
            {
                return;
            }

            result.Append("&RelayState=");

            // Encode the relay state if we're building a request. Otherwise, append unmodified.
            result.Append(_request != null ? Uri.EscapeDataString(Compression.Deflate(RelayState)) : RelayState);
        }

        /// <summary>
        /// If an asymmetric key has been specified, sign the request.
        /// </summary>
        /// <param name="result">The result.</param>
        private void AddSignature(StringBuilder result)
        {
            if (_signingCertificate == null || _signingCertificate.PrivateKey == null)
            {
                return;
            }

            result.Append(string.Format("&{0}=", HttpRedirectBindingConstants.SigAlg));
            string signatureUri = SignedXml.XmlDsigRSASHA1Url;

            if (_signingCertificate.PrivateKey is RSA)
            {
                signatureUri = XmlSignatureUtils.GetHashAlgorithmUri(SigningAlgorithm);
            }
            else
            {
                signatureUri = SignedXml.XmlDsigDSAUrl;
            }

            result.Append(Uri.EscapeDataString(signatureUri));

            // Calculate the signature of the URL as described in [SAMLBind] section 3.4.4.1.
            var signature = SignData(Encoding.UTF8.GetBytes(result.ToString()));

            result.AppendFormat("&{0}=", HttpRedirectBindingConstants.Signature);
            result.Append(Uri.EscapeDataString(Convert.ToBase64String(signature)));
        }

        /// <summary>
        /// Create the signature for the data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>SignData based on passed data and SigningKey.</returns>
        private byte[] SignData(byte[] data)
        {
            if (_signingCertificate.PrivateKey is RSA)
            {
                var rsa = (RSA)XmlSignatureUtils.GetPrivateKey(_signingCertificate);
                HashAlgorithmName hashAlgorithmName = XmlSignatureUtils.GetHashAlgorithmName(SigningAlgorithm);

                return rsa.SignData(data, hashAlgorithmName, RSASignaturePadding.Pkcs1);
            }
            else
            {
                var dsa = (DSACryptoServiceProvider)_signingCertificate.PrivateKey;
                return dsa.SignData(data);
            }
        }

        /// <summary>
        /// Depending on which one is specified, this method adds the SAMLRequest or SAMLResponse parameter to the URL query.
        /// </summary>
        /// <param name="result">The result.</param>
        private void AddMessageParameter(StringBuilder result)
        {
            if (!(_response == null || _request == null))
            {
                throw new Exception("Request or Response property MUST be set.");
            }

            string value;
            if (_request != null)
            {
                result.AppendFormat("{0}=", CONSTS.SamlRequest);
                value = _request;
            }
            else
            {
                result.AppendFormat("{0}=", HttpRedirectBindingConstants.SamlResponse);
                value = _response;
            }

            var encoded = Compression.Deflate(value);
            result.Append(Uri.EscapeDataString(encoded));
        }
    }
}