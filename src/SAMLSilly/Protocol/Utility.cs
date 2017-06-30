using Microsoft.Extensions.Logging;
using SAMLSilly.Bindings;
using SAMLSilly.Config;
using SAMLSilly.Schema.Core;
using SAMLSilly.Schema.Metadata;
using SAMLSilly.Schema.Protocol;
using SAMLSilly.Schema.XmlDSig;
using SAMLSilly.Specification;
using SAMLSilly.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace SAMLSilly.Protocol
{
    public class Utility
    {
        private readonly ILogger _logger;
        public Utility(ILoggerFactory _loggerFactory)
        {
            _logger = _loggerFactory.CreateLogger<Utility>();
        }

        /// <summary>
        /// Expected responses if session support is not present
        /// </summary>
        private readonly HashSet<string> expectedResponses = new HashSet<string>();

        /// <summary>
        /// Session key used to save the current message id with the purpose of preventing replay attacks
        /// </summary>
        private const string ExpectedInResponseToSessionKey = "ExpectedInResponseTo";

        /// <summary>
        /// Gets the trusted signers.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="identityProvider">The identity provider.</param>
        /// <returns>List of trusted certificate signers.</returns>
        public IEnumerable<AsymmetricAlgorithm> GetTrustedSigners(ICollection<KeyDescriptor> keys, IdentityProvider identityProvider)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            foreach (var clause in keys.SelectMany(k => k.KeyInfo.Items.AsEnumerable().Where(x => x is X509Data || x is KeyInfoClause)))
            {
                // Check certificate specifications
                KeyInfoClause keyClause;
                if (clause is X509Data)
                {
                    var cert = new X509Certificate2((byte[])((X509Data)clause).Items.First());
                    var keyInfo = new KeyInfoX509Data(cert, X509IncludeOption.WholeChain);

                    //TODO: @eByte23: this was old it we must check if there is actually a valid scenario for this
                    //var cert = XmlSignatureUtils.GetCertificateFromKeyInfo((KeyInfoX509Data)clause2);
                    if (!CertificateSatisfiesSpecifications(identityProvider, cert, _logger))
                    {
                        continue;
                    }

                    keyClause = keyInfo;
                }
                else
                {
                    keyClause = (KeyInfoClause)clause;
                }

                yield return XmlSignatureUtils.ExtractKey(keyClause);
            }
        }

        /// <summary>
        /// Determines whether the certificate is satisfied by all specifications.
        /// </summary>
        /// <param name="idp">The identity provider.</param>
        /// <param name="cert">The cert.</param>
        /// <returns><c>true</c> if certificate is satisfied by all specifications; otherwise, <c>false</c>.</returns>
        private static bool CertificateSatisfiesSpecifications(IdentityProvider idp, X509Certificate2 cert, ILogger logger)
        {
            return SpecificationFactory.GetCertificateSpecifications(idp).All(spec =>
            {

                bool isValid = spec.IsSatisfiedBy(cert);
                if (!isValid)
                {
                    logger.LogWarning(ErrorMessages.CertificateIsNotRFC3280Valid, cert.SubjectName.Name, cert.Thumbprint);
                }

                return isValid;
            });
        }

        /// <summary>
        /// Retrieves the name of the issuer from an XmlElement containing an assertion.
        /// </summary>
        /// <param name="assertion">An XmlElement containing an assertion</param>
        /// <returns>The identifier of the Issuer</returns>
        public static string GetIssuer(XmlElement assertion)
        {
            var result = string.Empty;
            var list = assertion.GetElementsByTagName("Issuer", Saml20Constants.Assertion);
            if (list.Count > 0)
            {
                var issuer = (XmlElement)list[0];
                result = issuer.InnerText;
            }

            return result;
        }

        /// <summary>
        /// Gets the assertion.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <param name="isEncrypted">if set to <c>true</c> [is encrypted].</param>
        /// <returns>The assertion XML.</returns>
        public XmlElement GetAssertion(XmlElement el, out bool isEncrypted)
        {
            _logger.LogDebug(TraceMessages.AssertionParse);

            var encryptedList = el.GetElementsByTagName(EncryptedAssertion.ElementName, Saml20Constants.Assertion);
            if (encryptedList.Count == 1)
            {
                isEncrypted = true;
                var encryptedAssertion = (XmlElement)encryptedList[0];

                _logger.LogDebug(TraceMessages.EncryptedAssertionFound, encryptedAssertion.OuterXml);

                return encryptedAssertion;
            }

            var assertionList = el.GetElementsByTagName(Assertion.ElementName, Saml20Constants.Assertion);
            if (assertionList.Count == 1)
            {
                isEncrypted = false;
                var assertion = (XmlElement)assertionList[0];

                _logger.LogDebug(TraceMessages.AssertionFound, assertion.OuterXml);

                return assertion;
            }

            _logger.LogWarning(ErrorMessages.AssertionNotFound);

            isEncrypted = false;
            return null;
        }

        public void AddExpectedResponseId(string id)
        {
            expectedResponses.Add(id);
        }

        /// <summary>
        /// Is called before the assertion is made into a strongly typed representation
        /// </summary>
        /// <param name="elem">The assertion element.</param>
        /// <param name="endpoint">The endpoint.</param>
        ///
        public void PreHandleAssertion(XmlElement elem, IdentityProvider endpoint)
        {
            _logger.LogDebug(TraceMessages.AssertionPrehandlerCalled);

            if (!string.IsNullOrEmpty(endpoint?.Endpoints?.DefaultLogoutEndpoint?.TokenAccessor))
            {
                var idpTokenAccessor = Activator.CreateInstance(Type.GetType(endpoint.Endpoints.DefaultLogoutEndpoint.TokenAccessor, false)) as ISaml20IdpTokenAccessor;
                if (idpTokenAccessor != null)
                {
                    _logger.LogDebug("{0}.{1} called", idpTokenAccessor.GetType(), "ReadToken");
                    idpTokenAccessor.ReadToken(elem);
                    _logger.LogDebug("{0}.{1} finished", idpTokenAccessor.GetType(), "ReadToken");
                }
            }
        }

        /// <summary>
        /// Gets the decoded SAML response.
        /// </summary>
        /// <param name="samlResponse">This is base64 encoded SAML Response (usually SAMLResponse on query string)</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>The decoded SAML response XML.</returns>
        public XmlDocument GetDecodedSamlResponse(string samlResponse, Encoding encoding)
        {
            _logger.LogDebug(TraceMessages.SamlResponseDecoding);

            var doc = new XmlDocument { PreserveWhitespace = true };
            samlResponse = encoding.GetString(Convert.FromBase64String(samlResponse));
            doc.LoadXml(samlResponse);


            //TODO: @ebyte23 FIX! Make the validate whole doc sig as well as assertion
            foreach (XmlNode n in doc.ChildNodes)
            {
                if (n.LocalName.ToUpperInvariant() == Response.ElementName.ToUpperInvariant())
                {
                    foreach (XmlNode x in n.ChildNodes)
                    {
                        if (n.LocalName.ToUpperInvariant() == Schema.XmlDSig.Signature.ElementName.ToUpperInvariant())
                        {
                            _logger.LogWarning("Two Signatures found in response, removing extra signature", samlResponse);
                            n.RemoveChild(x);
                        }
                    }
                }
            }

            _logger.LogDebug(TraceMessages.SamlResponseDecoded, samlResponse);

            return doc;
        }

        /// <summary>
        /// Gets the decrypted assertion.
        /// </summary>
        /// <param name="elem">The elem.</param>
        /// <returns>The decrypted <see cref="Saml20EncryptedAssertion"/>.</returns>
        public Saml20EncryptedAssertion GetDecryptedAssertion(XmlElement elem, Saml2Configuration config)
        {
            _logger.LogDebug(TraceMessages.EncryptedAssertionDecrypting);

            var decryptedAssertion = new Saml20EncryptedAssertion((RSA)config.ServiceProvider.SigningCertificate.PrivateKey);
            decryptedAssertion.LoadXml(elem);
            decryptedAssertion.Decrypt();

            _logger.LogDebug(TraceMessages.EncryptedAssertionDecrypted, decryptedAssertion.Assertion.DocumentElement.OuterXml);

            return decryptedAssertion;
        }

        /// <summary>
        /// Gets the status element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The <see cref="Status" /> element.</returns>
        public Status GetStatusElement(XmlElement element)
        {
            var statElem = element.GetElementsByTagName(Status.ElementName, Saml20Constants.Protocol)[0];
            return Serialization.DeserializeFromXmlString<Status>(statElem.OuterXml);
        }

        /// <summary>
        /// Checks for replay attack.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="element">The element.</param>
        public void CheckReplayAttack(XmlElement element, bool requireInResponseTo, IDictionary<string, object> session)
        {
            _logger.LogDebug(TraceMessages.ReplayAttackCheck);

            var inResponseToAttribute = element.Attributes["InResponseTo"];
            if (!requireInResponseTo && inResponseToAttribute == null)
            {
                return;
            }
            if (inResponseToAttribute == null)
            {
                throw new Saml20Exception(ErrorMessages.ResponseMissingInResponseToAttribute);
            }

            var inResponseTo = inResponseToAttribute.Value;
            if (string.IsNullOrEmpty(inResponseTo))
            {
                throw new Saml20Exception(ErrorMessages.ExpectedInResponseToEmpty);
            }

            if (session != null)
            {
                if (!session.ContainsKey(ExpectedInResponseToSessionKey))
                {
                    throw new Saml20Exception(ErrorMessages.ExpectedInResponseToMissing);
                }
                var expectedInResponseTo = (string)session[ExpectedInResponseToSessionKey];

                if (inResponseTo != expectedInResponseTo)
                {
                    _logger.LogError(ErrorMessages.ReplayAttack, inResponseTo, expectedInResponseTo);
                    throw new Saml20Exception(string.Format(ErrorMessages.ReplayAttack, inResponseTo, expectedInResponseTo));
                }
            }
            else
            {
                if (!expectedResponses.Contains(inResponseTo))
                {
                    throw new Saml20Exception(ErrorMessages.ExpectedInResponseToMissing);
                }
                expectedResponses.Remove(inResponseTo);
            }
            _logger.LogDebug(TraceMessages.ReplaceAttackCheckCleared);
        }

        public void AddExpectedResponse(Saml20AuthnRequest request, IDictionary<string, object> session)
        {
            // Save request message id to session
            if (session != null)
            {
                session.Add(ExpectedInResponseToSessionKey, request.Id);
            }
            else
            {
                expectedResponses.Add(request.Id);
            }
        }

        /// <summary>
        /// Deserializes an assertion, verifies its signature and logs in the user if the assertion is valid.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="elem">The elem.</param>
        public Saml20Assertion HandleAssertion(XmlElement elem, Saml2Configuration config, Func<string, object> getFromCache, Action<string, object, DateTime> setInCache)
        {
            _logger.LogDebug(TraceMessages.AssertionProcessing, elem.OuterXml);

            var issuer = GetIssuer(elem);
            var endp = IdpSelectionUtil.RetrieveIDPConfiguration(issuer, config);

            PreHandleAssertion(elem, endp);

            if (endp == null || endp.Metadata == null)
            {
                _logger.LogError(ErrorMessages.AssertionIdentityProviderUnknown);
                throw new Saml20Exception(ErrorMessages.AssertionIdentityProviderUnknown);
            }

            var quirksMode = endp.QuirksMode;
            var assertion = new Saml20Assertion(elem, null, quirksMode, config);

            // Check signatures
            if (!endp.OmitAssertionSignatureCheck)
            {
                var keys = endp.Metadata.GetKeys(KeyTypes.Signing);
                if (keys == null || !keys.Any())
                {
                    keys = endp.Metadata.GetKeys(KeyTypes.Encryption);
                }
                var trusted = GetTrustedSigners(keys, endp);
                if (!assertion.CheckSignature(trusted))
                {
                    _logger.LogError(ErrorMessages.AssertionSignatureInvalid);
                    throw new Saml20Exception(ErrorMessages.AssertionSignatureInvalid);
                }
            }

            // Check expiration
            if (assertion.IsExpired)
            {
                _logger.LogError(ErrorMessages.AssertionExpired);
                throw new Saml20Exception(ErrorMessages.AssertionExpired);
            }

            // Check one time use
            if (assertion.IsOneTimeUse)
            {
                if (getFromCache(assertion.Id) != null)
                {
                    _logger.LogError(ErrorMessages.AssertionOneTimeUseExceeded);
                    throw new Saml20Exception(ErrorMessages.AssertionOneTimeUseExceeded);
                }

                setInCache(assertion.Id, string.Empty, assertion.NotOnOrAfter);
            }

            _logger.LogDebug(TraceMessages.AssertionParsed, assertion.Id);
            return assertion;
        }

        /// <summary>
        /// Decrypts an encrypted assertion, and sends the result to the HandleAssertion method.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="elem">The elem.</param>
        public Saml20Assertion HandleEncryptedAssertion(XmlElement elem, Saml2Configuration config, Func<string, object> getFromCache, Action<string, object, DateTime> setInCache)
        {
            return HandleAssertion(GetDecryptedAssertion(elem, config).Assertion.DocumentElement, config, getFromCache, setInCache);
        }

        /// <summary>
        /// Handles the SOAP.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="inputStream">The input stream.</param>
        public void HandleSoap(HttpArtifactBindingBuilder builder, Stream inputStream, Saml2Configuration config, Action<Saml20Assertion> signonCallback, Func<string, object> getFromCache, Action<string, object, DateTime> setInCache, IDictionary<string, object> session)
        {
            var parser = new HttpArtifactBindingParser(inputStream);
            _logger.LogDebug(TraceMessages.SOAPMessageParse, parser.SamlMessage.OuterXml);

            if (parser.IsArtifactResolve)
            {
                _logger.LogDebug(TraceMessages.ArtifactResolveReceived);

                var idp = IdpSelectionUtil.RetrieveIDPConfiguration(parser.Issuer, config);
                if (!parser.CheckSamlMessageSignature(idp.Metadata.Keys))
                {
                    _logger.LogError(ErrorMessages.ArtifactResolveSignatureInvalid);
                    throw new Saml20Exception(ErrorMessages.ArtifactResolveSignatureInvalid);
                }

                builder.RespondToArtifactResolve(parser.ArtifactResolve, ((XmlDocument)getFromCache(parser.ArtifactResolve.Artifact)).DocumentElement);
            }
            else if (parser.IsArtifactResponse)
            {
                _logger.LogDebug(TraceMessages.ArtifactResolveReceived);

                var idp = IdpSelectionUtil.RetrieveIDPConfiguration(parser.Issuer, config);
                if (!parser.CheckSamlMessageSignature(idp.Metadata.Keys))
                {
                    _logger.LogError(ErrorMessages.ArtifactResponseSignatureInvalid);
                    throw new Saml20Exception(ErrorMessages.ArtifactResponseSignatureInvalid);
                }

                var status = parser.ArtifactResponse.Status;
                if (status.StatusCode.Value != Saml20Constants.StatusCodes.Success)
                {
                    _logger.LogError(ErrorMessages.ArtifactResponseStatusCodeInvalid, status.StatusCode.Value);
                    throw new Saml20Exception(string.Format(ErrorMessages.ArtifactResponseStatusCodeInvalid, status.StatusCode.Value));
                }

                if (parser.ArtifactResponse.Any.LocalName == Response.ElementName)
                {
                    CheckReplayAttack(parser.ArtifactResponse.Any, true, session);

                    var responseStatus = GetStatusElement(parser.ArtifactResponse.Any);
                    if (responseStatus.StatusCode.Value != Saml20Constants.StatusCodes.Success)
                    {
                        _logger.LogError(ErrorMessages.ArtifactResponseStatusCodeInvalid, responseStatus.StatusCode.Value);
                        throw new Saml20Exception(string.Format(ErrorMessages.ArtifactResponseStatusCodeInvalid, responseStatus.StatusCode.Value));
                    }

                    bool isEncrypted;
                    var assertion = GetAssertion(parser.ArtifactResponse.Any, out isEncrypted);
                    if (assertion == null)
                    {
                        _logger.LogError(ErrorMessages.ArtifactResponseMissingAssertion);
                        throw new Saml20Exception(ErrorMessages.ArtifactResponseMissingAssertion);
                    }

                    var samlAssertion = isEncrypted
                        ? HandleEncryptedAssertion(assertion, config, getFromCache, setInCache)
                        : HandleAssertion(assertion, config, getFromCache, setInCache);
                    signonCallback(samlAssertion);
                }
                else
                {
                    _logger.LogError(ErrorMessages.ArtifactResponseMissingResponse);
                    throw new Saml20Exception(ErrorMessages.ArtifactResponseMissingResponse);
                }
            }
            else
            {
                _logger.LogError(ErrorMessages.SOAPMessageUnsupportedSamlMessage);
                throw new Saml20Exception(ErrorMessages.SOAPMessageUnsupportedSamlMessage);
            }
        }

        /// <summary>
        /// Handle the authentication response from the IDP.
        /// </summary>
        /// <param name="context">The context.</param>
        public Saml20Assertion HandleResponse(Saml2Configuration config, string samlResponse, IDictionary<string, object> session, Func<string, object> getFromCache, Action<string, object, DateTime> setInCache)
        {
            var defaultEncoding = Encoding.UTF8;
            var doc = GetDecodedSamlResponse(samlResponse, defaultEncoding);
            _logger.LogDebug(TraceMessages.SamlResponseReceived, doc.OuterXml);

            // Determine whether the assertion should be decrypted before being validated.
            bool isEncrypted;
            var assertion = GetAssertion(doc.DocumentElement, out isEncrypted);
            if (isEncrypted)
            {
                assertion = GetDecryptedAssertion(assertion, config).Assertion.DocumentElement;
            }

            // Check if an encoding-override exists for the IdP endpoint in question

            var status = GetStatusElement(doc.DocumentElement);
            if (status.StatusCode.Value != Saml20Constants.StatusCodes.Success)
            {
                if (status.StatusCode.Value == Saml20Constants.StatusCodes.NoPassive)
                {
                    _logger.LogError(ErrorMessages.ResponseStatusIsNoPassive);
                    throw new Saml20Exception(ErrorMessages.ResponseStatusIsNoPassive);
                }

                var error = string.Format("{0} {1}", status.StatusCode?.Value ?? "", status.StatusCode?.SubStatusCode?.Value ?? "");
                _logger.LogError(ErrorMessages.ResponseStatusNotSuccessful, error);
                throw new Saml20Exception(string.Format(ErrorMessages.ResponseStatusNotSuccessful, error));
            }

            if (assertion == null)
            {
                _logger.LogError(ErrorMessages.AssertionNotFound);
                throw new Saml20Exception(string.Format(ErrorMessages.AssertionNotFound, status));
            }

            var issuer = Utility.GetIssuer(assertion);
            var endpoint = IdpSelectionUtil.RetrieveIDPConfiguration(issuer, config);
            if (!endpoint.AllowReplayAttacks)
            {
                CheckReplayAttack(doc.DocumentElement, !endpoint.AllowIdPInitiatedSso, session);
            }

            if (!string.IsNullOrEmpty(endpoint.ResponseEncoding))
            {
                Encoding encodingOverride;
                try
                {
                    encodingOverride = Encoding.GetEncoding(endpoint.ResponseEncoding);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError(ErrorMessages.UnknownEncoding, endpoint.ResponseEncoding);
                    throw new ArgumentException(string.Format(ErrorMessages.UnknownEncoding, endpoint.ResponseEncoding), ex);
                }

                if (encodingOverride.CodePage != defaultEncoding.CodePage)
                {
                    var doc1 = GetDecodedSamlResponse(samlResponse, encodingOverride);
                    assertion = GetAssertion(doc1.DocumentElement, out isEncrypted);
                }
            }

            return HandleAssertion(assertion, config, getFromCache, setInCache);
        }
    }
}