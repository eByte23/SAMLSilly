# SAMLSilly

### Build Statuses
[![Build status](https://ci.appveyor.com/api/projects/status/m8its6r2l4p0v1rh/branch/master?svg=true)](https://ci.appveyor.com/project/eByte23/samlsilly/branch/master)
[![Build status](https://ci.appveyor.com/api/projects/status/m8its6r2l4p0v1rh/branch/dev?svg=true)](https://ci.appveyor.com/project/eByte23/samlsilly/branch/dev)

SAMLSilly is a fork of SAML2/SAML2.DotNet35. The need for this fork came a from a need for more stable code base and support for a larger variety of IDP's


### NOTE: When using >= SHA256 SignatureType
When using SHA256 and above you must ensure you load your certificate using X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet flags.
If you do not do this you well get an exception like...

```System.Security.Cryptography.CryptographicException : Key not valid for use in specified state.```

For example
```
var certificate = new X509Certificate2(@"C:\My\Certificate\Path\cert.pfx", "mysuperduperpassword", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
config.ServiceProvider.SigningCertificate = certificate;
```