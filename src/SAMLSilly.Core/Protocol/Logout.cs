using SAMLSilly.Bindings;
using SAMLSilly.Schema.Protocol;
using SAMLSilly.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAMLSilly.Config;
using Microsoft.Extensions.Logging;

namespace SAMLSilly.Protocol
{
    public class Logout
    {
        private readonly Saml2Configuration config;
        private readonly ILogger logger;

        public Logout(ILoggerFactory logger, SAMLSilly.Config.Saml2Configuration config)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            this.logger = logger.CreateLogger<Logout>();
            this.config = config ?? throw new ArgumentNullException("config");
        }
        public void ValidateLogoutRequest(string requestType, System.Collections.Specialized.NameValueCollection requestParams, Uri requestUrl)
        {
            logger.LogDebug(TraceMessages.LogoutResponseReceived);

            var message = string.Empty;
            LogoutResponse response = null;
            switch (requestType) {
            case "GET":
                ValidateLogoutViaGet(requestUrl, out message, out response);
                break;
            case "POST":
                ValidateLogoutViaPost(requestParams, out message, out response);
                break;
            default:
                break;
            }

            if (response == null) {
                logger.LogError(ErrorMessages.UnsupportedRequestType, requestType);
                throw new Saml20Exception(string.Format(ErrorMessages.UnsupportedRequestType, requestType));
            }

            logger.LogDebug(TraceMessages.LogoutResponseParsed, message);

            if (response.Status.StatusCode.Value != Saml20Constants.StatusCodes.Success) {
                logger.LogError(ErrorMessages.ResponseStatusNotSuccessful, response.Status.StatusCode.Value);
                throw new Saml20Exception(string.Format(ErrorMessages.ResponseStatusNotSuccessful, response.Status.StatusCode.Value));
            }
        }

        private void ValidateLogoutViaPost(System.Collections.Specialized.NameValueCollection requestParams, out string message, out LogoutResponse response)
        {
            var parser = new HttpPostBindingParser(requestParams);
            logger.LogDebug(TraceMessages.LogoutResponsePostBindingParse, parser.Message);

            response = Serialization.DeserializeFromXmlString<LogoutResponse>(parser.Message);

            var idp = IdpSelectionUtil.RetrieveIDPConfiguration(response.Issuer.Value, config);
            if (idp.Metadata == null) {
                logger.LogError(ErrorMessages.UnknownIdentityProvider, idp.Id);
                throw new Saml20Exception(string.Format(ErrorMessages.UnknownIdentityProvider, idp.Id));
            }

            if (!parser.IsSigned) {
                logger.LogError(ErrorMessages.ResponseSignatureMissing);
                throw new Saml20Exception(ErrorMessages.ResponseSignatureMissing);
            }

            // signature on final message in logout
            if (!parser.CheckSignature(idp.Metadata.Keys)) {
                logger.LogError(ErrorMessages.ResponseSignatureInvalid);
                throw new Saml20Exception(ErrorMessages.ResponseSignatureInvalid);
            }

            message = parser.Message;
        }

        private void ValidateLogoutViaGet(Uri requestUrl, out string message, out LogoutResponse response)
        {
            var parser = new HttpRedirectBindingParser(requestUrl);
            response = Serialization.DeserializeFromXmlString<LogoutResponse>(parser.Message);

            logger.LogDebug(TraceMessages.LogoutResponseRedirectBindingParse, parser.Message, parser.SignatureAlgorithm, parser.Signature);

            var idp = IdpSelectionUtil.RetrieveIDPConfiguration(response.Issuer.Value, config);
            if (idp.Metadata == null) {
                logger.LogError(ErrorMessages.UnknownIdentityProvider, idp.Id);
                throw new Saml20Exception(string.Format(ErrorMessages.UnknownIdentityProvider, idp.Id));
            }

            if (!parser.VerifySignature(idp.Metadata.Keys)) {
                logger.LogError(ErrorMessages.ResponseSignatureInvalid);
                throw new Saml20Exception(ErrorMessages.ResponseSignatureInvalid);
            }

            message = parser.Message;
        }
    }
}
