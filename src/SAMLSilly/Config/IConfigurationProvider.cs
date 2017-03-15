namespace SAMLSilly.Config
{
    public interface IConfigurationProvider
    {
        Saml2Configuration GetConfiguration();
    }
}
