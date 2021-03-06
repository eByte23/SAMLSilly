﻿using System.Collections.Generic;
using System.Configuration;

namespace SAMLSilly.Config
{
    /// <summary>
    /// SAML2 Configuration Section.
    /// </summary>
    public class Saml2Configuration
    {
        /// <summary>
        /// Gets the section name.
        /// </summary>
        public static string Name => "saml2";

        /// <summary>
        /// Gets or sets the allowed audience uris.
        /// </summary>
        /// <value>The allowed audience uris.</value>
        public List<System.Uri> AllowedAudienceUris { get; set; }

        /// <summary>
        /// Gets or sets the if allow any AuthContextDelcRef if when you have non complaint saml
        /// </summary>
        /// <value>if allow any AuthContextDelcRef if when you have non complaint saml</value>
        public bool AllowAnyAuthContextDeclRef { get; set; }

        /// <summary>
        /// Gets or sets the assertion profile.
        /// </summary>
        /// <value>The assertion profile configuration.</value>
        public string AssertionProfileValidator { get; set; }
        /// <summary>
        /// Gets or sets the common domain cookie configuration.
        /// </summary>
        /// <value>The common domain cookie configuration.</value>
        public CommonDomainCookie CommonDomainCookie { get; set; }

        /// <summary>
        /// Gets or sets the identity providers.
        /// </summary>
        /// <value>The identity providers.</value>
        public IdentityProviders IdentityProviders { get; set; }
        /// <summary>
        /// Gets or sets the logging configuration.
        /// </summary>
        /// <value>The logging configuration.</value>
        public string LoggingFactoryType { get; set; }

        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        /// <value>The metadata.</value>
        public Metadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        /// <value>The service provider.</value>
        public ServiceProvider ServiceProvider { get; set; }

        public Saml2Configuration()
        {
            IdentityProviders = new IdentityProviders();
            AllowedAudienceUris = new List<System.Uri>();
            Metadata = new Metadata();
        }

        public AlgorithmType SigningAlgorithm { get; set; }
    }
}
