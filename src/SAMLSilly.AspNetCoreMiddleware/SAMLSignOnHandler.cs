using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SAMLSilly.AspNetCore.Core;
using SAMLSilly.AspNetCore.Models;
using SAMLSilly;
using SAMLSilly.Bindings;
using SAMLSilly.Config;
using SAMLSilly.Utils;
using SAMLSilly.AspNetCore.Utils;
using System.Threading.Tasks;
using SAMLSilly.Protocol;

namespace SAMLSilly.AspNetCore
{
    public class SAMLSignOnHandler
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IProcessSAMLAssertion _assetionProcessor;
        public SAMLSignOnHandler(ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor, IProcessSAMLAssertion assetionProcessor)
        {
            _assetionProcessor = assetionProcessor;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger(nameof(SAMLSignOnHandler));
            _httpContextAccessor = httpContextAccessor;
        }

        public void Handle(Saml2Configuration config, SAMLInputModel input)
        {
            _logger.LogDebug(TraceMessages.SignOnHandlerCalled);

            //What should happen if this is invalid?
            ValidateConfig(config);

            _logger.LogDebug("Check if has SamlResponse");

            var hasSamlResponse = input.HasResponse();
            var response = new SAMLSignOnResponseModel { };

            _logger.LogDebug("Checking requets method");
            _logger.LogDebug($"Method: {_httpContextAccessor.HttpContext.Request.Method}");

            if (_httpContextAccessor.HttpContext.IsGet() && !hasSamlResponse)
            {
                HandleSAMLRequest(config);
            }
            else if (_httpContextAccessor.HttpContext.IsGetOrPost() && hasSamlResponse)
            {
                HandleSAMLAuthResponse(config, input, hasSamlResponse, response);
            }
            else
            {
                _logger.LogWarning("Method was not an expected method!", new
                {
                    Method = _httpContextAccessor.HttpContext.Request.Method,
                    QueryString = _httpContextAccessor.HttpContext.Request.QueryString,
                    Form = _httpContextAccessor.HttpContext.Request.Body
                });

            }
        }

        private void HandleSAMLAuthResponse(Saml2Configuration config, SAMLInputModel input, bool hasSamlResponse, SAMLSignOnResponseModel response)
        {
            _logger.LogInformation($"POST/GET {hasSamlResponse}");

            //log response
            Saml20Assertion assertion = default(Saml20Assertion);

            try
            {
                _logger.LogInformation("Try and validate saml response");

                var util = new Utility(_loggerFactory);
                assertion = util.HandleResponse(config, input.SAMLResponse, null, null, null);
            }
            catch (Saml20Exception samlEx)
            {
                _logger.LogError(samlEx.ToString());
                //logon failed

            }
            catch (Exception ex)
            {
                //something went really wrong
                _logger.LogError(ex.ToString());

            }

            _assetionProcessor.Process(assertion);
        }

        public void HandleSAMLRequest(Saml2Configuration config)
        {
            SendAuthNRequest(config);
        }

        private void SendAuthNRequest(Saml2Configuration config)
        {
            _logger.LogDebug("Begin Transfer User to IDP");

            if (!config.IdentityProviders.Any())
            {
                _logger.LogError("No IdentityProviders configured");
            }

            // order signon endpoints by index and then check for default.



            var idp = (GetFirstIdp(config)?.Metadata.SSOEndpoints.FirstOrDefault(x => x.Binding == BindingType.Post && x.Type == EndpointType.SignOn)
            ?? GetFirstIdp(config)?.Metadata.SSOEndpoints.FirstOrDefault(x => x.Binding == BindingType.Redirect && x.Type == EndpointType.SignOn));

            if (idp == null)
            {
                _logger.LogError("IdentityProvider configured does not have POST or Redirect binding", config.IdentityProviders.FirstOrDefault());
            }

            _logger.LogDebug($"IdentityProvider found: {idp.Binding.ToString()}");
            var authnRequest = CreateAuthNRequest(config);

            if (idp.Binding == BindingType.Post)
            {
                var post = new SAMLSilly.Bindings.HttpPostBindingBuilder(idp);

                if (string.IsNullOrEmpty(authnRequest.ProtocolBinding))
                {
                    authnRequest.ProtocolBinding = Saml20Constants.ProtocolBindings.HttpPost;
                }

                var destination = IdpSelectionUtil.DetermineEndpointConfiguration(BindingType.Post, idp, config.IdentityProviders[0].Metadata.SSOEndpoints);
                authnRequest.Destination = destination.Url;

                var requestXml = authnRequest.GetXml();
                if (config.ServiceProvider.AuthNRequestsSigned)
                {
                    _logger.LogDebug("Sign AuthNRequest");
                    XmlSignatureUtils.SignDocument(requestXml, authnRequest.Id, config);
                }
                post.Request = requestXml.OuterXml;

                _httpContextAccessor.HttpContext.Response.WriteAsync(post.GetPage());
                return;
            }
            else
            {
                var redirectBuilder = new SAMLSilly.AspNetCore.BindingBuilders.HttpRedirectBindingBuilder(config);

                // if (string.IsNullOrEmpty(authnRequest.ProtocolBinding))
                // {
                authnRequest.ProtocolBinding = Saml20Constants.ProtocolBindings.HttpPost;
                // }

                var destination = IdpSelectionUtil.DetermineEndpointConfiguration(BindingType.Redirect, idp, config.IdentityProviders[0].Metadata.SSOEndpoints);
                authnRequest.Destination = destination.Url;

                redirectBuilder.Request = authnRequest.GetXml().OuterXml;
                redirectBuilder.SigningKey = config.ServiceProvider.SigningCertificate.PrivateKey;

                var query = redirectBuilder.ToQuery();
                var url = $"{idp.Url}?{query}";

                _httpContextAccessor.HttpContext.Response.Redirect(url, false);
                return;
            }


            throw new NotImplementedException();
        }

        public IActionResult HandleSAMLResponse(Saml2Configuration config, SAMLInputModel input)
        {
            return new ContentResult();
        }

        //Helpers

        private IdentityProvider GetFirstIdp(Saml2Configuration config)
            => config.IdentityProviders.FirstOrDefault();

        private Saml20AuthnRequest CreateAuthNRequest(Saml2Configuration config)
        {
            var authnRequest = Saml20AuthnRequest.GetDefault(config);
            var requestXml = authnRequest.GetXml();
            if(config.ServiceProvider.AuthNRequestsSigned) {
                XmlSignatureUtils.SignDocument(requestXml, authnRequest.Id, config);
            }

            return authnRequest;
        }

        private void ValidateConfig(Saml2Configuration config)
        {
            _logger.LogDebug("ValidateConfiguration called");

            if (
                (config.ServiceProvider.SigningCertificate == null ||
                !config.ServiceProvider.SigningCertificate.HasPrivateKey))
            {
                //should probably error...
            }

        }
    }
}