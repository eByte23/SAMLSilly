namespace SAMLSilly
{
    public static class SAMLConstants
    {
        //Value from System.Security.Cryptography.Xml.SignedXml.XmlDsigRSASHA1Url
        public const string XmlDsigRSASHA1Url = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

        //This is not included in the SAML standard but everyone supports it as SHA1 is deprecated
        public const string XmlDsigRSASHA256Url = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        //For future proofing
        public const string XmlDsigRSASHA512Url = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";
    }
}