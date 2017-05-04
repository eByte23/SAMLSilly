using Microsoft.Extensions.Logging;
using SAMLSilly.Config;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SAMLSilly.Utils
{
    public class MetadataUtils
    {
        private readonly Saml2Configuration configuration;
        private readonly ILogger _logger;

        public MetadataUtils(Config.Saml2Configuration configuration, ILogger logger)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (logger == null) throw new ArgumentNullException("logger");

            this.configuration = configuration;
            this._logger = logger;
        }

        /// <summary>
        /// Creates the metadata document.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="sign">if set to <c>true</c> sign the document.</param>
        public string CreateMetadataDocument(Encoding encoding, bool sign)
        {
            _logger.LogDebug(TraceMessages.MetadataDocumentBeingCreated);

            var keyinfo = new System.Security.Cryptography.Xml.KeyInfo();
            var keyClause = new System.Security.Cryptography.Xml.KeyInfoX509Data(configuration.ServiceProvider.SigningCertificate, X509IncludeOption.EndCertOnly);
            keyinfo.AddClause(keyClause);

            var metaDoc = new Saml20MetadataDocument().Load(configuration);

            _logger.LogDebug(TraceMessages.MetadataDocumentCreated);
            return metaDoc.ToXml(encoding, configuration.ServiceProvider.SigningCertificate);
        }
    }
}