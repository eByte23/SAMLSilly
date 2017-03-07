using System.Configuration;

namespace SAMLSilly.AspNet.Config
{
    /// <summary>
    /// Action configuration collection.
    /// </summary>
    [ConfigurationCollection(typeof(ActionElement), AddItemName = "action", CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class ActionCollection : EnumerableConfigurationElementCollection<ActionElement>
    {
    }
}
