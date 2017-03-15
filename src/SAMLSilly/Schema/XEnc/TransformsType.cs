using System;
using System.Xml.Serialization;
using SAMLSilly.Schema.XmlDSig;

namespace SAMLSilly.Schema.XEnc
{
    /// <summary>
    /// The Transforms type
    /// </summary>
    [Serializable]
    [XmlType(TypeName = "TransformsType", Namespace = Saml20Constants.Xenc)]
    public class TransformsType
    {
        /// <summary>
        /// The XML Element name of this class
        /// </summary>
        public const string ElementName = "TransformsType";

        #region Elements

        /// <summary>
        /// Gets or sets the transform.
        /// </summary>
        /// <value>The transform.</value>
        [XmlElement("Transform", Namespace = Saml20Constants.Xmldsig)]
        public Transform[] Transform { get; set; }

        #endregion
    }
}
