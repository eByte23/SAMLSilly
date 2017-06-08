### Metadata class structure



#### Constructor

- empty
- SamlConfiguration


#### Props




#### Methods

- string EntityId { get; }
- bool Sign { get; set; }
- List<KeyDescriptor> Keys { get; }


- List<IdentityProviderEndpoint> AssertionConsumerServiceEndpoints { get; }
- List<IdentityProviderEndpoint> SSOEndpoints { get; }
- List<IdentityProviderEndpoint> SPSLOEndpoints { get; }
- List<IdentityProviderEndpoint> IDPSLOEndpoints { get; }


#### Extention Methods

-  EntityDescriptor CreateDefaultEntity();
-  string GetAttributeQueryEndpointLocation();
-  List<Endpoint> GetAttributeQueryEndpoints();
-  string GetIDPARSEndpoint(ushort index);
-  List<KeyDescriptor> GetKeys(KeyTypes usage);
-  string ToXml(Encoding encoding, X509Certificate2 certificate);