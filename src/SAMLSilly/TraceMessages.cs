using System;
namespace SAMLSilly
{
    public class TraceMessages
    {
        public const string ArtifactCreated = "Artifact created: {0}";
        public const string ArtifactRedirectReceived = "Artifact redirect received: {0}";
        public const string ArtifactResolved = "Artifact resolved: {0}";
        public const string ArtifactResolveForKnownIdentityProvider = "Resolving artifact \"{$1}\" from identity provider \"{$1}\" endpoint \"{$1}\"";
        public const string ArtifactResolveReceived = "Artifact resolve received: {0}";
        public const string ArtifactResolveResponseSent = "Sending response to artifact resolve request \"{$1}\": {1}";
        public const string ArtifactResponseReceived = "Artifact response received: {0}";
        public const string AttrQueryAssertionReceived = "AttrQuery assertion received: {0}";
        public const string AttrQuerySent = "AttrQuery sent to \"{$1}\": {1}";
        public const string AuthnRequestPrepared = "AuthRequest sent for identity provider \"{$1}\" using binding \"{$1}\"";
        public const string CommonDomainCookieReceived = "Common domain cookie received: {0}";
        public const string CommonDomainCookieRedirect = "Redirect to SignOn endpoint found in Common Domain Cookie";
        public const string CommonDomainCookieRedirectNotFound = "Redirect to SignOn endpoint \"{$1}\"";
        public const string LogoutActionsExecuting = "Executing Logout Actions";
        public const string LogoutRequestReceived = "Logout request received: {0}";
        public const string LogoutRequestSent = "Logout request sent for identity provider \"{0}\" using \"{1}\" binding: {2}";
        public const string SignOnActionsExecuting = "Executing SignOn Actions";
        public const string SignOnProcessed = "Successfully processed signon request for \"{1}\" using NameIdFormat \"{2}\" for session \"{0}\"";
        public const string SOAPMessageParse = "Parsing SOAP message: {0}";
        public const string AssertionFound = "Assertion found: {0}";
        public const string AssertionParse = "Assertion being parsed";
        public const string AssertionParsed = "Successfully parsed Assertion: {0}";
        public const string AssertionPrehandlerCalled = "Assertion prehandler called";
        public const string AssertionProcessing = "Processing assertion: {0}";
        public const string AudienceRestrictionValidated = "Audience restriction validated for intended URIs {0} against allowed URIs {1}";
        public const string AuthnRequestSent = "AuthnRequest sent: {0}";
        public const string CommonDomainCookieRedirectForDiscovery = "Redirecting to Common Domain for identity provider discovery";
        public const string EncryptedAssertionDecrypted = "EncryptedAssertion Decrypted: {0}";
        public const string EncryptedAssertionDecrypting = "Decrypting EncryptedAssertion";
        public const string EncryptedAssertionFound = "EncryptedAssertion found: {0}";
        public const string IdentityProviderRedirect = "Identity provider not found. Redirecting for identity provider selection";
        public const string IdentityProviderRetreivedFromCommonDomainCookie = "Identity provider retreived from Common Domain Cookie: {0}";
        public const string IdentityProviderRetreivedFromDefault = "Identity provider retreived from known providers: {0}";
        public const string IdentityProviderRetreivedFromQueryString = "Identity provider retreived from IDPChoiceParamater: {0}";
        public const string IdentityProviderRetreivedFromSelection = "Redirecting to idpSelectionUrl for selection of identity provider: {0}";
        public const string LogoutHandlerCalled = "Logout handler called";
        public const string LogoutRequestParsed = "Successfully parsed Logout request: {0}";
        public const string LogoutRequestPostBindingParse = "Parsing Logout request POST binding message: {0}";
        public const string LogoutRequestRedirectBindingParse = "Parsing Logout request Redirect binding message with signature algorithm {1} and signature {2}: {0}";
        public const string LogoutResponseParsed = "Successfully parsed Logout response: {0}";
        public const string LogoutResponsePostBindingParse = "Parsing Logout response POST binding message: {0}";
        public const string LogoutResponseReceived = "Logout response received";
        public const string LogoutResponseRedirectBindingParse = "Parsing Logout response Redirect binding message with signature algorithm {1} and signature {2}: {0}";
        public const string LogoutResponseSent = "Logout response sent: {0}";
        public const string MetadataDocumentBeingCreated = "Metadata document being created";
        public const string MetadataDocumentCreated = "Metadata document successfully created";
        public const string ReplaceAttackCheckCleared = "No replay attack detected";
        public const string ReplayAttackCheck = "Checking for replay attack";
        public const string SamlResponseDecoded = "Successfully decoded SamlResponse: {0}";
        public const string SamlResponseDecoding = "SamlResponse decoding";
        public const string SamlResponseReceived = "SamlResponse received: {0}";
        public const string SignOnHandlerCalled = "SignOn handler called";

    }
}