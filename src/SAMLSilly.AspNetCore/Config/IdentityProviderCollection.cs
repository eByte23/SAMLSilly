using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using SAMLSilly.Schema.Metadata;
using SAMLSilly.Utils;

namespace SAMLSilly.AspNet.Config
{
    /// <summary>
    /// Identity Provider configuration collection.
    /// </summary>
    [ConfigurationCollection(typeof(IdentityProviderElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class IdentityProviderCollection : EnumerableConfigurationElementCollection<IdentityProviderElement>
    {
        /// <summary>
        /// Gets or sets the encodings.
        /// </summary>
        [ConfigurationProperty("encodings")]
        public string Encodings
        {
            get { return (string)base["encodings"]; }
            set { base["encodings"] = value; }
        }

        /// <summary>
        /// Gets or sets the metadata location.
        /// </summary>
        [ConfigurationProperty("metadata")]
        public string MetadataLocation
        {
            get
            {
                var value = (string)base["metadata"];
                if (!Path.IsPathRooted(value))
                {
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, value);
                }

                return value;
            }

            set
            {
                base["metadata"] = value;
            }
        }

        /// <summary>
        /// Gets the selection URL to use for choosing identity providers if multiple are available and none are set as default.
        /// </summary>
        [ConfigurationProperty("selectionUrl")]
        public string SelectionUrl
        {
            get { return (string)base["selectionUrl"]; }
        }

    }
}
