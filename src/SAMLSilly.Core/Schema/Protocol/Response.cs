using System;
using System.Xml.Serialization;
using SAMLSilly.Schema.Core;

namespace SAMLSilly.Schema.Protocol
{
    /// <summary>
    /// The &lt;Response&gt; message element is used when a response consists of a list of zero or more assertions
    /// that satisfy the request. It has the complex type ResponseType, which extends StatusResponseType
    /// </summary>
    [Serializable]
    [XmlType(Namespace = Saml20Constants.Protocol)]
    [XmlRoot(ElementName, Namespace = Saml20Constants.Protocol, IsNullable = false)]
    public class Response : StatusResponse
    {
        /// <summary>
        /// The XML Element name of this class
        /// </summary>
        public new const string ElementName = "Response";

        #region Elements

        /// <summary>
        /// Gets or sets the items.
        /// Specifies an assertion by value, or optionally an encrypted assertion by value.
        /// </summary>
        /// <value>The items.</value>
        [XmlElement(Assertion.ElementName, typeof(Assertion), Namespace = Saml20Constants.Assertion, Order = 1)]
        [XmlElement("EncryptedAssertion", typeof(EncryptedElement), Namespace = Saml20Constants.Assertion, Order = 1)]
        public object[] Items { get; set; }

        #endregion
    }
}
