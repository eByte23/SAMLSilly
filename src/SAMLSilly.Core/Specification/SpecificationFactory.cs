﻿using System;
using System.Collections.Generic;
using SAMLSilly.Config;
using Microsoft.Extensions.Logging;

namespace SAMLSilly.Specification
{
    /// <summary>
    /// Specification factory for getting certificate specification.
    /// </summary>
    public class SpecificationFactory
    {
        /// <summary>
        /// Gets the certificate specifications.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns>A list of certificate validation specifications for this endpoint</returns>
        public static List<ICertificateSpecification> GetCertificateSpecifications(IdentityProvider endpoint)
        {
            var specs = new List<ICertificateSpecification>();
            if (endpoint.CertificateValidationTypes != null && endpoint.CertificateValidationTypes.Count > 0)
            {
                foreach (var elem in endpoint.CertificateValidationTypes)
                {
                    try
                    {
                        var val = (ICertificateSpecification)Activator.CreateInstance(Type.GetType(elem));
                        specs.Add(val);
                    }
                    catch (Exception e)
                    {
                        var loggerFactory = Activator.CreateInstance<ILoggerFactory>();
                        loggerFactory.CreateLogger<SpecificationFactory>().LogError(e.Message, e);
                    }
                }
            }

            if (specs.Count == 0)
            {
                // Add default specification
                specs.Add(new DefaultCertificateSpecification());
            }

            return specs;
        }
    }
}
