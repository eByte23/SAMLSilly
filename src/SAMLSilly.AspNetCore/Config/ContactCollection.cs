using System.Configuration;

namespace SAMLSilly.AspNet.Config
{
    /// <summary>
    /// Service Provider Endpoint configuration collection.
    /// </summary>
    [ConfigurationCollection(typeof(ContactElement), AddItemName = "contact", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class ContactCollection : EnumerableConfigurationElementCollection<ContactElement>
    {
    }
}
