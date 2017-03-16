using System.Xml;
using SAMLSilly.Schema.Core;
using SAMLSilly.Validation;
using Xunit;

namespace SAMLSilly.Tests.Validation
{
    /// <summary>
    /// <see cref="Saml20SubjectConfirmationValidator"/> tests.
    /// </summary>

    public class Saml20SubjectConfirmationValidatorTests
    {
        /// <summary>
        /// ValidateSubjectConfirmation method tests.
        /// </summary>

        public class ValidateSubjectConfirmationMethod
        {
            /// <summary>
            /// Tests the validation of the SubjectConfirmation element
            /// </summary>
            [Fact]
            //ExpectedMessage = "SubjectConfirmationData element MUST have at least one " + KeyInfo.ElementName + " subelement")]
            public void ThrowsExceptionWhenSubjectConfirmationDataDoesNotContainKeyInfo()
            {
                // Arrange
                var subjectConfirmation = new SubjectConfirmation
                {
                    Method = Saml20Constants.SubjectConfirmationMethods.HolderOfKey,
                    SubjectConfirmationData = new SubjectConfirmationData()
                };

                var validator = new Saml20SubjectConfirmationValidator();

                // Act
                Assert.Throws(typeof(Saml20FormatException), () =>
                {
                    validator.ValidateSubjectConfirmation(subjectConfirmation);
                });
            }

            /// <summary>
            /// Tests the validation of the SubjectConfirmation element's method attribute.
            /// </summary>
            [Fact]
            //ExpectedMessage = "Method attribute of SubjectConfirmation MUST contain at least one non-whitespace character")]
            public void ThrowsExceptionWhenSubjectConfirmationHasEmptyMethod()
            {
                // Arrange
                var subjectConfirmation = new SubjectConfirmation { Method = " " };
                var validator = new Saml20SubjectConfirmationValidator();

                // Act
                Assert.Throws(typeof(Saml20FormatException), () =>
                {
                    validator.ValidateSubjectConfirmation(subjectConfirmation);
                });
            }

            /// <summary>
            /// Tests the validation of the SubjectConfirmation element's method attribute.
            /// </summary>
            [Fact]
            //ExpectedMessage = "SubjectConfirmation element has Method attribute which is not a wellformed absolute uri.")]
            public void ThrowsExceptionWhenSubjectConfirmationHasWrongMethod()
            {
                // Arrange
                var subjectConfirmation = new SubjectConfirmation { Method = "malformed uri" };
                var validator = new Saml20SubjectConfirmationValidator();

                // Act
                Assert.Throws(typeof(Saml20FormatException), () =>
                {
                    validator.ValidateSubjectConfirmation(subjectConfirmation);
                });
            }

            /// <summary>
            /// Subjects the confirmation data_ method_ holder of key_ valid.
            /// </summary>
            [Fact]
            public void ValidatesSubjectConfirmationData_Method_HolderOfKey_Valid()
            {
                // Arrange
                var subjectConfirmation = new SubjectConfirmation
                {
                    Method = Saml20Constants.SubjectConfirmationMethods.HolderOfKey,
                    SubjectConfirmationData = new SubjectConfirmationData()
                };
                var doc = new XmlDocument();
                var elem = doc.CreateElement("ds", "KeyInfo", Saml20Constants.Xmldsig);
                elem.AppendChild(doc.CreateElement("lalala"));

                subjectConfirmation.SubjectConfirmationData.AnyElements = new[] { elem };

                var validator = new Saml20SubjectConfirmationValidator();

                // Act
                validator.ValidateSubjectConfirmation(subjectConfirmation);
            }
        }
    }
}
