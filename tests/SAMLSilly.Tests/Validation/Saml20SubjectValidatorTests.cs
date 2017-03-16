using System.Collections.Generic;
using SAMLSilly;
using SAMLSilly.Schema.Core;
using SAMLSilly.Tests;
using SAMLSilly.Validation;
using Xunit;

namespace SAML2.DotNet35.Tests.Validation
{
    /// <summary>
    /// <see cref="Saml20SubjectValidator"/> tests.
    /// </summary>

    public class Saml20SubjectValidatorTests
    {
        /// <summary>
        /// ValidateSubject method tests.
        /// </summary>

        public class ValidateSubjectMethod
        {
            /// <summary>
            /// Tests the validation that ensures that a subject MUST have at least one sub element
            /// </summary>
            [Fact]
            //ExpectedMessage = "Subject MUST contain either an identifier or a subject confirmation")]
            public void ThrowsExceptionWhenSubjectConfirmationDoesNotContainSubject()
            {
                // Arrange
                var saml20Assertion = AssertionUtil.GetBasicAssertion();
                saml20Assertion.Subject.Items = new object[] { };

                var validator = new Saml20SubjectValidator();

                // Act
                Assert.Throws(typeof(Saml20FormatException), () =>
                {
                    validator.ValidateSubject(saml20Assertion.Subject);
                });
            }

            /// <summary>
            /// Tests the validation that ensures that a subject MUST have at least one sub element of correct type
            /// </summary>
            [Fact]
            //ExpectedMessage = "Subject must have either NameID, EncryptedID or SubjectConfirmation subelement.")]
            public void ThrowsExceptionWhenSubjectConfirmationContainsElementsOfWrongIdentifier()
            {
                // Arrange
                var saml20Assertion = AssertionUtil.GetBasicAssertion();
                saml20Assertion.Subject.Items = new object[] { string.Empty, 24, new List<object>(1), new Advice() };

                var validator = new Saml20SubjectValidator();

                // Act
                Assert.Throws(typeof(Saml20FormatException), () =>
                {
                    validator.ValidateSubject(saml20Assertion.Subject);
                });
            }
        }
    }
}
