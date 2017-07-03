using System.Security.Cryptography.Xml;
using System.Xml;

namespace SAMLSilly.Utils
{

    /// <summary>
    /// Signed XML with Id Resolvement class.
    /// </summary>
    public class SignedXmlWithIdResolvement : SignedXml
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignedXmlWithIdResolvement"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public SignedXmlWithIdResolvement(XmlDocument document) : base(document) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignedXmlWithIdResolvement"/> class from the specified <see cref="T:System.Xml.XmlElement"/> object.
        /// </summary>
        /// <param name="elem">The <see cref="T:System.Xml.XmlElement"/> object to use to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.SignedXml"/>.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="elem"/> parameter is null.
        /// </exception>
        public SignedXmlWithIdResolvement(XmlElement elem) : base(elem) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignedXmlWithIdResolvement"/> class.
        /// </summary>
        public SignedXmlWithIdResolvement() { }

        /// <summary>
        /// Returns the <see cref="T:System.Xml.XmlElement"/> object with the specified ID from the specified <see cref="T:System.Xml.XmlDocument"/> object.
        /// </summary>
        /// <param name="document">The <see cref="T:System.Xml.XmlDocument"/> object to retrieve the <see cref="T:System.Xml.XmlElement"/> object from.</param>
        /// <param name="idValue">The ID of the <see cref="T:System.Xml.XmlElement"/> object to retrieve from the <see cref="T:System.Xml.XmlDocument"/> object.</param>
        /// <returns>The <see cref="T:System.Xml.XmlElement"/> object with the specified ID from the specified <see cref="T:System.Xml.XmlDocument"/> object, or null if it could not be found.</returns>
        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            var elem = base.GetIdElement(document, idValue);
            if (elem == null)
            {
                var nl = document.GetElementsByTagName("*");
                var enumerator = nl.GetEnumerator();
                while (enumerator != null && enumerator.MoveNext())
                {
                    var node = (XmlNode)enumerator.Current;
                    if (node == null || node.Attributes == null)
                    {
                        continue;
                    }

                    var nodeEnum = node.Attributes.GetEnumerator();
                    while (nodeEnum != null && nodeEnum.MoveNext())
                    {
                        var attr = (XmlAttribute)nodeEnum.Current;
                        if (attr != null && (attr.LocalName.ToLower() == "id" && attr.Value == idValue && node is XmlElement))
                        {
                            return (XmlElement)node;
                        }
                    }
                }
            }

            return elem;
        }
    }
}
