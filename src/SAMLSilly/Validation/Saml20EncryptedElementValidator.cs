using System;
using SAMLSilly.Schema.Protocol;

namespace SAMLSilly.Validation
{
    /// <summary>
    /// SAML2 Encrypted Element validator.
    /// </summary>
    public class Saml20EncryptedElementValidator
    {
        /// <summary>
        /// Validates the encrypted element.
        /// </summary>
        /// <param name="encryptedElement">The encrypted element.</param>
        /// <param name="parentNodeName">Name of the parent node.</param>
        public void ValidateEncryptedElement(EncryptedElement encryptedElement, string parentNodeName)
        {
            if (encryptedElement == null)
            {
                throw new ArgumentNullException("encryptedElement");
            }

            if (encryptedElement.EncryptedData == null)
            {
                throw new Saml20FormatException($"An {parentNodeName} MUST contain an xenc:EncryptedData element");
            }

            if (encryptedElement.EncryptedData.Type != null
                && !string.IsNullOrEmpty(encryptedElement.EncryptedData.Type)
                && encryptedElement.EncryptedData.Type != $"{Saml20Constants.Xenc}Element")
            {
                throw new Saml20FormatException($"Type attribute of EncryptedData MUST have value {Saml20Constants.Xenc}Element if it is present");
            }
        }
    }
}
