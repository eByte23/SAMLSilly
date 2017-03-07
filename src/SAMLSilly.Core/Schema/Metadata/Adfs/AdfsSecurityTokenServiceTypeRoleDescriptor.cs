using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SAMLSilly.Schema.Metadata.Adfs
{
    [Serializable]
    [XmlType(TypeName = "SecurityTokenServiceType", Namespace = Saml20Constants.Adfs)]
    [XmlRoot(ElementName, Namespace = Saml20Constants.Adfs, IsNullable = false)]
    public class AdfsSecurityTokenServiceTypeRoleDescriptor : RoleDescriptor
    {
    }
}
