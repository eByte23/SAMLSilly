using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using SAMLSilly.Schema.Metadata;
using SAMLSilly.Utils;
using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Primitives;

namespace SAMLSilly.Bindings
{
    /// <summary>
    /// Parses and validates the query parameters of a HttpRedirectBinding. [SAMLBind] section 3.4.
    /// </summary>
    public class HttpRedirectBindingParser : IHttpBindingParser
    {
        /// <summary>
        /// <c>RelaystateDecoded</c> backing field.
        /// </summary>
        private string _relaystateDecoded;

        /// <summary>
        /// The signed part of the query is recreated in this string.
        /// </summary>
        private string _signedquery;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRedirectBindingParser"/> class.
        /// </summary>
        /// <param name="uri">The URL that the user was redirected to by the IDP. It is essential for the survival of the signature,
        /// that the URL is not modified in any way, e.g. by URL-decoding it.</param>
        public HttpRedirectBindingParser(IEnumerable<KeyValuePair<string, StringValues>> queryParams)
        {
            var paramDict = queryParams.ToDictionary(x => x.Key, x => x.Value.Where(v => v != StringValues.Empty).FirstOrDefault() ?? string.Empty);
            // If the message is signed, save the original, encoded parameters so that the signature can be verified.

            foreach (var param in paramDict)
            {
                SetParam(param.Key, param.Value);
            }

            if (IsSigned)
            {
                CreateSignatureSubject(paramDict);
            }

            ReadMessageParameter();
        }

        /// <summary>
        /// Gets a value indicating whether the parsed message contains a request message.
        /// </summary>
        public bool IsRequest
        {
            get { return !IsResponse; }
        }

        /// <summary>
        /// Gets a value indicating whether the parsed message contains a response message.
        /// </summary>
        public bool IsResponse { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the parsed message contains a signature.
        /// </summary>
        public bool IsSigned
        {
            get { return Signature != null; }
        }

        /// <summary>
        /// Gets the message that was contained in the query. Use the <code>IsResponse</code> or the <code>IsRequest</code> property
        /// to determine the kind of message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the relay state that was included with the query. The result will still be encoded according to the
        /// rules given in section 3.4.4.1 of [SAMLBind], i.e. base64-encoded and DEFLATE-compressed. Use the property
        /// <code>RelayStateDecoded</code> to get the decoded contents of the RelayState parameter.
        /// </summary>
        public string RelayState { get; private set; }

        /// <summary>
        /// Gets a decoded and decompressed version of the RelayState parameter.
        /// </summary>
        public string RelayStateDecoded
        {
            get { return _relaystateDecoded ?? (_relaystateDecoded = Utils.Compression.Inflate(RelayState)); }
        }

        /// <summary>
        /// Gets the signature value
        /// </summary>
        public string Signature { get; private set; }

        /// <summary>
        /// Gets the signature algorithm.
        /// </summary>
        /// <value>The signature algorithm.</value>
        public string SignatureAlgorithm { get; private set; }

        /// <summary>
        /// Validates the signature using the public part of the asymmetric key given as parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><code>true</code> if the signature is present and can be verified using the given key.
        /// <code>false</code> if the signature is present, but can't be verified using the given key.</returns>
        /// <exception cref="InvalidOperationException">If the query is not signed, and therefore cannot have its signature verified. Use
        /// the <code>IsSigned</code> property to check for this situation before calling this method.</exception>
        public bool CheckSignature(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (!(key is DSA || key is RSA))
            {
                throw new ArgumentException("The key must be an instance of either DSA or RSACryptoServiceProvider.");
            }

            if (!IsSigned)
            {
                throw new InvalidOperationException("Query is not signed, so there is no signature to verify.");
            }

            var hashAlgorithm = XmlSignatureUtils.GetHashAlgorithm(SignatureAlgorithm);
            var hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(_signedquery));

            if (key is RSA)
            {
                var rsa = (RSA)key;
                return VerifyHash(hashAlgorithm, rsa, hash, DecodeSignature());
            }
            else
            {
                var dsa = (DSA)key;
                return dsa.VerifySignature(hash, DecodeSignature());
            }
        }

        /// <summary>
        /// Check the signature of a HTTP-Redirect message using the list of keys.
        /// </summary>
        /// <param name="keys">A list of KeyDescriptor elements. Probably extracted from the metadata describing the IDP that sent the message.</param>
        /// <returns>True, if one of the given keys was able to verify the signature. False in all other cases.</returns>
        public bool VerifySignature(IEnumerable<KeyDescriptor> keys)
        {
            foreach (var keyDescriptor in keys)
            {
                foreach (KeyInfoClause clause in (KeyInfo)keyDescriptor.KeyInfo)
                {
                    var key = XmlSignatureUtils.ExtractKey(clause);
                    if (key != null && CheckSignature(key))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Re-creates the list of parameters that are signed, in order to verify the signature.
        /// </summary>
        /// <param name="queryParams">The query parameters.</param>
        private void CreateSignatureSubject(Dictionary<string, string> queryParams)
        {
            var signedQuery = string.Empty;
            if (IsResponse)
            {
                signedQuery += $"{HttpRedirectBindingConstants.SamlResponse}={Uri.EscapeDataString(queryParams[HttpRedirectBindingConstants.SamlResponse])}";
            }
            else
            {
                signedQuery += $"{HttpRedirectBindingConstants.SamlRequest}={Uri.EscapeDataString(queryParams[HttpRedirectBindingConstants.SamlRequest])}";
            }

            if (RelayState != null)
            {
                signedQuery += $"&{HttpRedirectBindingConstants.RelayState}={Uri.EscapeDataString(queryParams[HttpRedirectBindingConstants.RelayState])}";
            }

            if (Signature != null)
            {
                signedQuery += $"&{HttpRedirectBindingConstants.SigAlg}={Uri.EscapeDataString(queryParams[HttpRedirectBindingConstants.SigAlg])}";
            }

            _signedquery = signedQuery;
        }

        /// <summary>
        /// Decodes the Signature parameter.
        /// </summary>
        /// <returns>The decoded signature.</returns>
        private byte[] DecodeSignature()
        {
            if (!IsSigned)
            {
                throw new InvalidOperationException("Query does not contain a signature.");
            }

            return Convert.FromBase64String(Signature);
        }

        /// <summary>
        /// Decodes the message parameter.
        /// </summary>
        private void ReadMessageParameter()
        {
            Message = Compression.Inflate(Message);
        }

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void SetParam(string key, string value)
        {
            switch (key.ToLower())
            {
                case "samlrequest":
                    IsResponse = false;
                    Message = value;
                    return;
                case "samlresponse":
                    IsResponse = true;
                    Message = value;
                    return;
                case "relaystate":
                    RelayState = value;
                    return;
                case "sigalg":
                    SignatureAlgorithm = value;
                    return;
                case "signature":
                    Signature = value;
                    return;
            }
        }


        private bool VerifyHash(HashAlgorithm hashAlg, RSA rsa, byte[] hash, byte[] v)
        {
            HashAlgorithmName hashAlgName = HashAlgorithmName.SHA1;

            if (hashAlg is SHA1Managed)
            {
                hashAlgName = HashAlgorithmName.SHA1;
            }
            else if (hashAlg is SHA256Managed)
            {
                hashAlgName = HashAlgorithmName.SHA256;
            }
            else if (hashAlg is SHA512Managed)
            {
                hashAlgName = HashAlgorithmName.SHA512;
            }

            return rsa.VerifyHash(hash, v, hashAlgName, RSASignaturePadding.Pkcs1);
        }
    }


}
