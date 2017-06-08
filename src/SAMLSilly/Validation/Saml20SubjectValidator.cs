using System;
using SAMLSilly.Schema.Core;
using SAMLSilly.Schema.Protocol;
using System.Linq;

namespace SAMLSilly.Validation
{
    /// <summary>
    /// SAML Subject validator.
    /// </summary>
    public class Saml20SubjectValidator : ISaml20SubjectValidator
    {
        /// <summary>
        /// NameID validator.
        /// </summary>
        private readonly ISaml20NameIdValidator _nameIdValidator = new Saml20NameIdValidator();

        /// <summary>
        /// SubjectConfirmation validator.
        /// </summary>
        private readonly ISaml20SubjectConfirmationValidator _subjectConfirmationValidator = new Saml20SubjectConfirmationValidator();

        /// <summary>
        /// Validates the subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        public virtual void ValidateSubject(Subject subject)
        {
            if (subject == null)            
                throw new ArgumentNullException("subject");            

            var validContentFound = false;
            if (subject.Items == null || !subject.Items.Any())            
                throw new Saml20FormatException("Subject MUST contain either an identifier or a subject confirmation");            

            foreach (var item in subject.Items)
            {
                if (item is NameId)
                {
                    validContentFound = true;
                    _nameIdValidator.ValidateNameId((NameId)item);
                }
                else if (item is EncryptedElement)
                {
                    validContentFound = true;
                    _nameIdValidator.ValidateEncryptedId((EncryptedElement)item);
                }
                else if (item is SubjectConfirmation)
                {
                    validContentFound = true;
                    _subjectConfirmationValidator.ValidateSubjectConfirmation((SubjectConfirmation)item);
                }
            }

            if (!validContentFound)            
                throw new Saml20FormatException("Subject must have either NameID, EncryptedID or SubjectConfirmation subelement.");            
        }
    }
}
