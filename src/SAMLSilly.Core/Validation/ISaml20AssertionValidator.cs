using System;
using SAMLSilly.Schema.Core;

namespace SAMLSilly.Validation
{
    /// <summary>
    /// SAML2 Assertion Validator interface.
    /// </summary>
    public interface ISaml20AssertionValidator
    {
        /// <summary>
        /// Validates the assertion.
        /// </summary>
        /// <param name="assertion">The assertion.</param>
        void ValidateAssertion(Assertion assertion, bool allowAnyAuthContextDeclRef);

        /// <summary>
        /// Validates the time restrictions.
        /// </summary>
        /// <param name="assertion">The assertion.</param>
        /// <param name="allowedClockSkew">The allowed clock skew.</param>
        void ValidateTimeRestrictions(Assertion assertion, TimeSpan allowedClockSkew);
    }
}
