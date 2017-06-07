using Microsoft.Extensions.Logging;
using SAMLSilly.Config;
using SAMLSilly.Schema.Core;
using SAMLSilly.Schema.Metadata;
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

namespace SAMLSilly
{
    /// <summary>
    /// The <see cref="Saml20MetadataDocument"/> class handles functionality related to the &lt;EntityDescriptor&gt; element.
    /// If a received metadata document contains a &lt;EntitiesDescriptor&gt; element, it is necessary to use an
    /// instance of this class for each &lt;EntityDescriptor&gt; contained.
    /// </summary>
    public class Saml20MetadataDocument
    {
        private List<Endpoint> _attributeQueryEndpoints;
        private Dictionary<int, IndexedEndpoint> _spArsEndpoints;
        private object _idpArsEndpoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saml20MetadataDocument"/> class.
        /// </summary>
        public Saml20MetadataDocument() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Saml20MetadataDocument"/> class.
        /// </summary>
        /// <param name="entityDescriptor">The entity descriptor.</param>
        public Saml20MetadataDocument(XmlDocument entityDescriptor)
            : this()
        {
            Initialize(entityDescriptor);
        }

        public bool Signed { get; private set; }
        public bool ValidSignature { get; private set; }

        private void Initialize(XmlDocument entityDescriptor)
        {
            if (XmlSignatureUtils.IsSigned(entityDescriptor))
            {
                Signed = true;
                ValidSignature = XmlSignatureUtils.CheckSignature(entityDescriptor);
            }

            ExtractKeyDescriptors(entityDescriptor);
            Entity = Serialization.DeserializeFromXmlString<EntityDescriptor>(entityDescriptor.OuterXml);
            ExtractEndpoints();
        }

        public Saml20MetadataDocument(Stream document, IEnumerable<Encoding> encodings)
        {
            LoadXmlStream(document, encodings);
        }

        public static IEnumerable<Encoding> DefaultEncodings => new[] { Encoding.UTF8, Encoding.GetEncoding("iso-8859-1") };

        public Saml20MetadataDocument LoadXmlStream(Stream stream)
            => LoadXmlStream(stream, null);

        public Saml20MetadataDocument LoadXmlStream(Stream stream, IEnumerable<Encoding> encodings)
            => LoadXmlDocument(x => x.Load(stream), encodings);
        public Saml20MetadataDocument LoadXmlString(string xml)
            => LoadXmlString(xml, null);

        public Saml20MetadataDocument LoadXmlString(string xml, IEnumerable<Encoding> encodings)
            => LoadXmlDocument(x => x.LoadXml(xml), encodings);

        public Saml20MetadataDocument LoadXmlDocument(Action<XmlDocument> docLoad, IEnumerable<Encoding> encodings)
        {
            var doc = new XmlDocument { PreserveWhitespace = true };
            docLoad(doc);

            return LoadXmlDocument(doc, encodings);
        }

        public Saml20MetadataDocument LoadXmlDocument(XmlDocument document, IEnumerable<Encoding> encodings)
        {
            if (document == null) throw new InvalidOperationException("Metadata not a valid document");

            var isInitialized = false;
            foreach (var child in document.ChildNodes.Cast<XmlNode>().Where(child => child.NamespaceURI == Saml20Constants.Metadata))
            {
                if (child.LocalName == EntityDescriptor.ElementName)
                {
                    Initialize(document);
                    isInitialized = true;
                }

                // TODO: Decide how to handle several entities in one metadata file.
                if (child.LocalName == EntitiesDescriptor.ElementName)
                {
                    throw new NotImplementedException($"Multiple {EntitiesDescriptor.ElementName} elements");
                }
            }

            // No entity descriptor found.
            if (!isInitialized) throw new InvalidDataException();

            return this;
        }


        #region stuff i want to remove
        // /// <summary>
        // /// Loads a file into an XmlDocument. If the loading or the signature check fails, the method will retry using another encoding.
        // /// </summary>
        // /// <param name="filename">The filename.</param>
        // /// <returns>The XML document.</returns>
        // private static XmlDocument LoadAsXmlDocument(IEnumerable<Encoding> encodings, Action<XmlDocument> docLoad, Action<XmlDocument, Encoding> quirksModeDocLoad)
        // {
        //     var doc = new XmlDocument { PreserveWhitespace = true };

        //     try
        //     {
        //         // First attempt a standard load, where the XML document is expected to declare its encoding by itself.
        //         docLoad(doc);
        //         try
        //         {
        //             if (XmlSignatureUtils.IsSigned(doc) && !XmlSignatureUtils.CheckSignature(doc))
        //             {
        //                 // Bad, bad, bad... never use exceptions for control flow! Who wrote this?
        //                 // Throw an exception to get into quirksmode.
        //                 throw new InvalidOperationException("Invalid file signature");
        //             }
        //         }
        //         catch (CryptographicException)
        //         {
        //             // Ignore cryptographic exception caused by Geneva server's inability to generate a
        //             // .NET compliant xml signature
        //             return ParseGenevaServerMetadata(doc);
        //         }

        //         return doc;
        //     }
        //     catch (XmlException)
        //     {
        //         // Enter quirksmode
        //         foreach (var encoding in encodings)
        //         {
        //             StreamReader reader = null;
        //             try
        //             {
        //                 quirksModeDocLoad(doc, encoding);
        //                 if (XmlSignatureUtils.IsSigned(doc) && !XmlSignatureUtils.CheckSignature(doc))
        //                 {
        //                     continue;
        //                 }
        //             }
        //             catch (XmlException)
        //             {
        //                 continue;
        //             }
        //             finally
        //             {
        //                 if (reader != null)
        //                 {
        //                     reader.Close();
        //                 }
        //             }

        //             return doc;
        //         }
        //     }

        //     return null;
        // }
        #endregion

        #region stuff i want to remove
        // /// <summary>
        // /// Parses the geneva server metadata.
        // /// </summary>
        // /// <param name="doc">The doc.</param>
        // /// <returns>The XML document.</returns>
        // private static XmlDocument ParseGenevaServerMetadata(XmlDocument doc)
        // {
        //     if (doc == null)
        //     {
        //         throw new ArgumentNullException("doc");
        //     }

        //     if (doc.DocumentElement == null)
        //     {
        //         throw new ArgumentException("DocumentElement cannot be null", "doc");
        //     }

        //     var other = new XmlDocument { PreserveWhitespace = true };
        //     other.LoadXml(doc.OuterXml);

        //     foreach (var node in other.DocumentElement.ChildNodes.Cast<XmlNode>().Where(node => node.Name != IdpSsoDescriptor.ElementName).ToList())
        //     {
        //         other.DocumentElement.RemoveChild(node);
        //     }

        //     return other;
        // }

        #endregion

        /// <summary>
        /// Gets the endpoints specified in the <c>&lt;AssertionConsumerService&gt;</c> element in the <c>SpSsoDescriptor</c>.
        /// These endpoints are only applicable if we are reading metadata issued by a service provider.
        /// </summary>
        public List<IdentityProviderEndpoint> AssertionConsumerServiceEndpoints { get; set; }


        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <value>The entity.</value>
        public EntityDescriptor Entity { get; set; }

        /// <summary>
        /// Gets the ID of the entity described in the document.
        /// </summary>
        public string EntityId => Entity?.EntityId;

        /// <summary>
        /// Gets the IDP SLO endpoints.
        /// </summary>
        public List<IdentityProviderEndpoint> IDPSLOEndpoints { get; set; } = new List<IdentityProviderEndpoint>();


        /// <summary>
        /// Gets the keys contained in the metadata document.
        /// </summary>
        public List<KeyDescriptor> Keys { get; set; } = new List<KeyDescriptor>();


        /// <summary>
        /// Gets or sets a value indicating whether the metadata should be signed when the ToXml() method is called.
        /// </summary>
        public bool Sign { get; set; }

        /// <summary>
        /// Gets the SP SLO endpoints.
        /// </summary>
        public List<IdentityProviderEndpoint> SPSLOEndpoints { get; set; } = new List<IdentityProviderEndpoint>();


        /// <summary>
        /// Gets the SSO endpoints.
        /// </summary>
        public List<IdentityProviderEndpoint> SSOEndpoints { get; set; } = new List<IdentityProviderEndpoint>();

        public Dictionary<int, IndexedEndpoint> IDPArsEndpoints { get; set; } = new Dictionary<int, IndexedEndpoint>();


        /// <summary>
        /// Creates a default entity in the
        /// </summary>
        /// <returns>The default <see cref="EntityDescriptor"/>.</returns>
        public EntityDescriptor CreateDefaultEntity()
        {
            if (Entity != null)
            {
                throw new InvalidOperationException("An entity is already created in this document.");
            }

            Entity = new EntityDescriptor
            {
                Id = "id" + Guid.NewGuid().ToString("N")
            };

            return Entity;
        }

        /// <summary>
        /// Gets the IDP ArtifactResolutionService endpoint.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The artifact resolution service endpoint.</returns>
        public string GetIDPARSEndpoint(ushort index)
        {
            var ep = IDPArsEndpoints[index];
            return ep?.Location ?? string.Empty;
        }

        /// <summary>
        /// Gets all AttributeQuery endpoints.
        /// </summary>
        /// <returns>The List of attribute query endpoints.</returns>
        public List<Endpoint> GetAttributeQueryEndpoints()
        {
            if (_attributeQueryEndpoints == null)
            {
                ExtractEndpoints();
            }

            return _attributeQueryEndpoints;
            //return new List<Endpoint>();
        }

        /// <summary>
        /// Gets the location of the first AttributeQuery endpoint.
        /// </summary>
        /// <returns>The attribute query endpoint location.</returns>
        public string GetAttributeQueryEndpointLocation()
        {
            var endpoints = GetAttributeQueryEndpoints();
            if (endpoints.Count == 0)
            {
                throw new Saml20Exception("The identity provider does not support attribute queries.");
            }

            return endpoints[0].Location;
        }

        /// <summary>
        /// Retrieves the keys marked with the usage given as parameter.
        /// </summary>
        /// <param name="usage">The usage.</param>
        /// <returns>A list containing the keys. If no key is marked with the given usage, the method returns an empty list.</returns>
        public List<KeyDescriptor> GetKeys(KeyTypes usage) => Keys.FindAll(desc => desc.Use == usage);

        public string ToXml() => ToXml(null);
        public string ToXml(Encoding encoding) => ToXml(encoding, null, AlgorithmType.SHA256);

        /// <summary>
        /// Return a string containing the metadata XML based on the settings added to this instance.
        /// The resulting XML will be signed, if the AsymmetricAlgorithm property has been set.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="sign">if the document should be signed</param>
        /// <param name="certificate">Certificate to be used for signing (if appropriate)</param>
        /// <returns>The XML.</returns>
        public string ToXml(Encoding encoding, X509Certificate2 certificate, AlgorithmType signatureAlgorithm)
        {
            if (Entity == null)
            {
                throw new InvalidOperationException("You are trying to sign an empty or uninitialized metadata document");
            }

            encoding = encoding ?? Encoding.UTF8;
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(Serialization.SerializeToXmlString(Entity));

            // Add the correct encoding to the head element.
            if (doc.FirstChild is XmlDeclaration)
            {
                ((XmlDeclaration)doc.FirstChild).Encoding = encoding.WebName;
            }
            else
            {
                doc.PrependChild(doc.CreateXmlDeclaration("1.0", encoding.WebName, null));
            }

            if (certificate != null)
            {
                SignDocument(doc, certificate, signatureAlgorithm);
            }

            return doc.OuterXml;
        }

        /// <summary>
        /// Gets the binding.
        /// </summary>
        /// <param name="samlBinding">The SAML binding.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The binding.</returns>
        private static string GetBinding(BindingType samlBinding, string defaultValue)
        {
            switch (samlBinding)
            {
                case BindingType.Artifact:
                    return Saml20Constants.ProtocolBindings.HttpArtifact;

                case BindingType.Post:
                    return Saml20Constants.ProtocolBindings.HttpPost;

                case BindingType.Redirect:
                    return Saml20Constants.ProtocolBindings.HttpRedirect;

                case BindingType.Soap:
                    return Saml20Constants.ProtocolBindings.HttpSoap;

                case BindingType.NotSet:
                    return defaultValue;

                default:
                    throw new InvalidOperationException(string.Format("Unsupported SAML binding {0}", Enum.GetName(typeof(BindingType), samlBinding)));
            }
        }

        /// <summary>
        /// Signs the document.
        /// </summary>
        /// <param name="doc">The doc.</param>
        private static void SignDocument(XmlDocument doc, X509Certificate2 certificate, AlgorithmType signatureAlgorithm)
        {
            if (!certificate.HasPrivateKey)
            {
                throw new InvalidOperationException("Private key access to the signing certificate is required.");
            }

            XmlSignatureUtils.GenericSign(doc, doc.DocumentElement.GetAttribute("ID"), certificate, (appDoc, xmlElm) =>
            {
                // Append the computed signature. The signature must be placed as the sibling of the Issuer element.
                appDoc.DocumentElement.InsertBefore(appDoc.ImportNode(xmlElm, true), appDoc.DocumentElement.FirstChild);
            }, signatureAlgorithm);
        }

        /// <summary>
        /// Takes the configuration class and converts it to a SAML2.0 metadata document.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="keyInfo">The keyInfo.</param>
        private void ConvertToMetadata(Saml2Configuration config)
        {
            var entity = CreateDefaultEntity();
            entity.EntityId = config.ServiceProvider.Id;
            if (config.ServiceProvider.UseValidUntil)
            {
                entity.ValidUntil = DateTime.Now.AddDays(7);
            }

            var serviceProviderDescriptor = new SpSsoDescriptor
            {
                ProtocolSupportEnumeration = new[] { Saml20Constants.Protocol },
                AuthnRequestsSigned = XmlConvert.ToString(config.ServiceProvider.AuthNRequestsSigned),
                WantAssertionsSigned = XmlConvert.ToString(config.ServiceProvider.WantAssertionsSigned)
            };

            if (config.ServiceProvider.NameIdFormats.Count > 0)
            {
                serviceProviderDescriptor.NameIdFormat = new string[config.ServiceProvider.NameIdFormats.Count];
                var count = 0;
                foreach (var elem in config.ServiceProvider.NameIdFormats)
                {
                    serviceProviderDescriptor.NameIdFormat[count++] = elem.Format;
                }
            }

            var baseUrl = new Uri(config.ServiceProvider.Server);
            var logoutServiceEndpoints = new List<Endpoint>();
            var signonServiceEndpoints = new List<IndexedEndpoint>();

            var artifactResolutionEndpoints = new List<IndexedEndpoint>(2);

            // Include endpoints.
            foreach (var endpoint in config.ServiceProvider.Endpoints)
            {
                if (endpoint.Type == EndpointType.SignOn)
                {
                    var loginEndpoint = new IndexedEndpoint
                    {
                        Index = endpoint.Index,
                        IsDefault = endpoint.Default,
                        Location = new Uri(baseUrl, endpoint.LocalPath).ToString(),
                        Binding = GetBinding(endpoint.Binding, Saml20Constants.ProtocolBindings.HttpPost)
                    };
                    signonServiceEndpoints.Add(loginEndpoint);

                    if (config.ServiceProvider.IncludeArtifactResolutionEndpoints)
                    {
                        var artifactSignonEndpoint = new IndexedEndpoint
                        {
                            Binding = Saml20Constants.ProtocolBindings.HttpSoap,
                            Index = loginEndpoint.Index,
                            Location = loginEndpoint.Location
                        };
                        artifactResolutionEndpoints.Add(artifactSignonEndpoint);
                    }

                    continue;
                }

                if (endpoint.Type == EndpointType.Logout)
                {
                    var location = new Uri(baseUrl, endpoint.LocalPath).ToString();
                    var logoutEndpoint = new Endpoint
                    {
                        Location = location,
                        ResponseLocation = location,
                        Binding = GetBinding(endpoint.Binding, Saml20Constants.ProtocolBindings.HttpPost)
                    };
                    logoutServiceEndpoints.Add(logoutEndpoint);

                    if (config.ServiceProvider.IncludeArtifactResolutionEndpoints)
                    {
                        var artifactLogoutEndpoint = new IndexedEndpoint
                        {
                            Binding = Saml20Constants.ProtocolBindings.HttpSoap,
                            Index = endpoint.Index,
                            Location = logoutEndpoint.Location
                        };
                        artifactResolutionEndpoints.Add(artifactLogoutEndpoint);
                    }
                    continue;
                }
            }

            serviceProviderDescriptor.SingleLogoutService = logoutServiceEndpoints.ToArray();
            serviceProviderDescriptor.AssertionConsumerService = signonServiceEndpoints.ToArray();

            // Attribute consuming service.
            if (config.Metadata.RequestedAttributes.Count > 0)
            {
                var attConsumingService = new AttributeConsumingService();
                serviceProviderDescriptor.AttributeConsumingService = new[] { attConsumingService };
                attConsumingService.Index = signonServiceEndpoints[0].Index;
                attConsumingService.IsDefault = true;
                attConsumingService.ServiceName = new[] { new LocalizedName("SP", "en") };

                attConsumingService.RequestedAttribute = new RequestedAttribute[config.Metadata.RequestedAttributes.Count];

                for (var i = 0; i < config.Metadata.RequestedAttributes.Count; i++)
                {
                    attConsumingService.RequestedAttribute[i] = new RequestedAttribute
                    {
                        Name = config.Metadata.RequestedAttributes[i].Name,
                        NameFormat = SamlAttribute.NameformatBasic
                    };

                    if (config.Metadata.RequestedAttributes[i].IsRequired)
                    {
                        attConsumingService.RequestedAttribute[i].IsRequired = true;
                    }
                }
            }
            else
            {
                serviceProviderDescriptor.AttributeConsumingService = new AttributeConsumingService[0];
            }

            if (config.Metadata == null || !config.Metadata.ExcludeArtifactEndpoints)
            {
                serviceProviderDescriptor.ArtifactResolutionService = artifactResolutionEndpoints.ToArray();
            }

            entity.Items = new object[] { serviceProviderDescriptor };

            // Keyinfo
            var keySigning = new KeyDescriptor();
            var keyEncryption = new KeyDescriptor();
            serviceProviderDescriptor.KeyDescriptor = new[] { keySigning, keyEncryption };

            keySigning.Use = KeyTypes.Signing;
            keySigning.UseSpecified = true;

            keyEncryption.Use = KeyTypes.Encryption;
            keyEncryption.UseSpecified = true;

            var keyinfo = new System.Security.Cryptography.Xml.KeyInfo();
            var keyClause = new System.Security.Cryptography.Xml.KeyInfoX509Data(config.ServiceProvider.SigningCertificate, X509IncludeOption.EndCertOnly);
            keyinfo.AddClause(keyClause);

            // Ugly conversion between the .Net framework classes and our classes ... avert your eyes!!
            keySigning.KeyInfo = Serialization.DeserializeFromXmlString<Schema.XmlDSig.KeyInfo>(keyinfo.GetXml().OuterXml);
            keyEncryption.KeyInfo = keySigning.KeyInfo;

            // apply the <Organization> element
            if (config.Metadata.Organization != null)
            {
                entity.Organization = new Schema.Metadata.Organization
                {
                    OrganizationName = new[] { new LocalizedName { Value = config.Metadata.Organization.Name, Language = "en" } },
                    OrganizationDisplayName = new[] { new LocalizedName { Value = config.Metadata.Organization.DisplayName, Language = "en" } },
                    OrganizationURL = new[] { new LocalizedURI { Value = config.Metadata.Organization.Url, Language = "en" } }
                };
            }

            if (config.Metadata.Contacts != null && config.Metadata.Contacts.Any())
            {
                entity.ContactPerson = config.Metadata.Contacts.Select(x => new Schema.Metadata.Contact
                {
                    ContactType =
                        (Schema.Metadata.ContactType)
                        ((int)x.Type),
                    Company = x.Company,
                    GivenName = x.GivenName,
                    SurName = x.SurName,
                    EmailAddress = new[] { x.Email },
                    TelephoneNumber = new[] { x.Phone }
                }).ToArray();
            }
        }

        /// <summary>
        /// Extracts the endpoints.
        /// </summary>
        private void ExtractEndpoints()
        {
            if (Entity != null)
            {

                SSOEndpoints = new List<IdentityProviderEndpoint>();
                IDPArsEndpoints = new Dictionary<int, IndexedEndpoint>();
                SPSLOEndpoints = new List<IdentityProviderEndpoint>();
                _spArsEndpoints = new Dictionary<int, IndexedEndpoint>();
                AssertionConsumerServiceEndpoints = new List<IdentityProviderEndpoint>();
                _attributeQueryEndpoints = new List<Endpoint>();

                foreach (var item in Entity.Items)
                {
                    if (item is IdpSsoDescriptor)
                    {
                        var descriptor = (IdpSsoDescriptor)item;
                        foreach (var endpoint in descriptor.SingleSignOnService)
                        {
                            BindingType binding;
                            switch (endpoint.Binding)
                            {
                                case Saml20Constants.ProtocolBindings.HttpPost:
                                    binding = BindingType.Post;
                                    break;

                                case Saml20Constants.ProtocolBindings.HttpRedirect:
                                    binding = BindingType.Redirect;
                                    break;

                                case Saml20Constants.ProtocolBindings.HttpArtifact:
                                    binding = BindingType.Artifact;
                                    break;

                                case Saml20Constants.ProtocolBindings.HttpSoap:
                                    binding = BindingType.Artifact;
                                    break;

                                case "urn:mace:shibboleth:1.0:profiles:AuthnRequest":
                                    // This is a SAML 1.1 binding, it is silly, we shall ignore it
                                    continue;
                                case "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST-SimpleSign":
                                    // This is a another binding we will ignore for the moment
                                    continue;
                                default:
                                    throw new InvalidOperationException("Binding not supported: " + endpoint.Binding);
                            }

                            SSOEndpoints.Add(new IdentityProviderEndpoint { Url = endpoint.Location, Binding = binding });
                        }

                        if (descriptor.SingleLogoutService != null)
                        {
                            foreach (var endpoint in descriptor.SingleLogoutService)
                            {
                                BindingType binding;
                                switch (endpoint.Binding)
                                {
                                    case Saml20Constants.ProtocolBindings.HttpPost:
                                        binding = BindingType.Post;
                                        break;

                                    case Saml20Constants.ProtocolBindings.HttpRedirect:
                                        binding = BindingType.Redirect;
                                        break;

                                    case Saml20Constants.ProtocolBindings.HttpArtifact:
                                        binding = BindingType.Artifact;
                                        break;

                                    case Saml20Constants.ProtocolBindings.HttpSoap:
                                        binding = BindingType.Artifact;
                                        break;

                                    default:
                                        throw new InvalidOperationException("Binding not supported: " + endpoint.Binding);
                                }

                                IDPSLOEndpoints.Add(new IdentityProviderEndpoint { Url = endpoint.Location, Binding = binding });
                            }
                        }

                        if (descriptor.ArtifactResolutionService != null)
                        {
                            foreach (var ie in descriptor.ArtifactResolutionService)
                            {
                                //_idpArsEndpoints.Add(ie.Index, ie);
                            }
                        }
                    }

                    if (item is SpSsoDescriptor)
                    {
                        var descriptor = (SpSsoDescriptor)item;
                        foreach (var endpoint in descriptor.AssertionConsumerService)
                        {
                            BindingType binding;
                            switch (endpoint.Binding)
                            {
                                case Saml20Constants.ProtocolBindings.HttpPost:
                                    binding = BindingType.Post;
                                    break;

                                case Saml20Constants.ProtocolBindings.HttpRedirect:
                                    binding = BindingType.Redirect;
                                    break;

                                case Saml20Constants.ProtocolBindings.HttpArtifact:
                                    binding = BindingType.Artifact;
                                    break;

                                case Saml20Constants.ProtocolBindings.HttpSoap:
                                    binding = BindingType.Artifact;
                                    break;

                                default:
                                    throw new InvalidOperationException("Binding not supported: " + endpoint.Binding);
                            }

                            AssertionConsumerServiceEndpoints.Add(new IdentityProviderEndpoint { Url = endpoint.Location, Binding = binding });
                        }

                        if (descriptor.SingleLogoutService != null)
                        {
                            foreach (var endpoint in descriptor.SingleLogoutService)
                            {
                                BindingType binding;
                                switch (endpoint.Binding)
                                {
                                    case Saml20Constants.ProtocolBindings.HttpPost:
                                        binding = BindingType.Post;
                                        break;

                                    case Saml20Constants.ProtocolBindings.HttpRedirect:
                                        binding = BindingType.Redirect;
                                        break;

                                    case Saml20Constants.ProtocolBindings.HttpArtifact:
                                        binding = BindingType.Artifact;
                                        break;

                                    case Saml20Constants.ProtocolBindings.HttpSoap:
                                        binding = BindingType.Artifact;
                                        break;

                                    default:
                                        throw new InvalidOperationException("Binding not supported: " + endpoint.Binding);
                                }

                                SPSLOEndpoints.Add(new IdentityProviderEndpoint { Url = endpoint.Location, Binding = binding });
                            }
                        }

                        if (descriptor.ArtifactResolutionService != null)
                        {
                            foreach (var ie in descriptor.ArtifactResolutionService)
                            {
                                _spArsEndpoints.Add(ie.Index, ie);
                            }
                        }
                    }

                    if (item is AttributeAuthorityDescriptor)
                    {
                        var aad = (AttributeAuthorityDescriptor)item;
                        _attributeQueryEndpoints.AddRange(aad.AttributeService);
                    }
                }
            }
        }

        /// <summary>
        /// Extract KeyDescriptors from the metadata document represented by this instance.
        /// </summary>
        private void ExtractKeyDescriptors()
        {
            if (Keys != null || Entity == null)
            {
                return;
            }

            Keys = new List<KeyDescriptor>();
            foreach (var keyDescriptor in Entity.Items.OfType<RoleDescriptor>().SelectMany(rd => rd.KeyDescriptor))
            {
                Keys.Add(keyDescriptor);
            }
        }

        /// <summary>
        /// Retrieves the key descriptors contained in the document
        /// </summary>
        /// <param name="doc">The doc.</param>
        private void ExtractKeyDescriptors(XmlDocument doc)
        {
            var list = doc.GetElementsByTagName(KeyDescriptor.ElementName, Saml20Constants.Metadata);
            Keys = new List<KeyDescriptor>(list.Count);

            foreach (XmlNode node in list)
            {
                Keys.Add(Serialization.DeserializeFromXmlString<KeyDescriptor>(node.OuterXml));
            }
        }


        public Saml20MetadataDocument Load(Saml2Configuration config)
        {
            ConvertToMetadata(config);
            return this;
        }

        public Saml20MetadataDocument Load(Stream metadataXmlStream) => LoadXmlStream(metadataXmlStream);

    }
}