namespace SAMLSilly.AspNetCore.Core
{
    public static class SAMLConstants
    {
        public static string SHA1Dsig => System.Security.Cryptography.Xml.SignedXml.XmlDsigRSASHA1Url;

        public const string SHA256Dsig = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
    }
}