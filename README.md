# SAMLSilly

## What is SAMLSilly
SAMLSilly is a SAML2.0 implementation for .net and .net core (coming soon). This library was forked form [elerch/SAML2](https://github.com/elerch/SAML2) original to [eByte23/SAML2.DotNet35](https://github.com/eByte23/SAML2.DotNet35) to make it run on .net 35 and to make some large changes to the way it handles ADFS (Active Directory Federation Services). The code has diverged past the point of a merge back thus this repository.

### Build Statuses:
master | dev | vnext
-------|-----| ------
[![Build status](https://ci.appveyor.com/api/projects/status/m8its6r2l4p0v1rh/branch/master?svg=true)](https://ci.appveyor.com/project/eByte23/samlsilly/branch/master) | [![Build status](https://ci.appveyor.com/api/projects/status/m8its6r2l4p0v1rh/branch/dev?svg=true)](https://ci.appveyor.com/project/eByte23/samlsilly/branch/dev)| [![Build status](https://ci.appveyor.com/api/projects/status/m8its6r2l4p0v1rh/branch/vnext?svg=true)](https://ci.appveyor.com/project/eByte23/samlsilly/branch/vnext)


### NOTE: When using >= SHA256 SignatureType
When using SHA256 and above you must ensure you load your certificate using X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet flags.
If you do not do this you well get an exception like...

```System.Security.Cryptography.CryptographicException : Key not valid for use in specified state.```

For example
```
var certificate = new X509Certificate2(@"C:\My\Certificate\Path\cert.pfx", "mysuperduperpassword", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
config.ServiceProvider.SigningCertificate = certificate;
```
