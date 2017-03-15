using SAMLSilly.Schema.Core;

namespace SAMLSilly.Validation
{
    /// <summary>
    /// SAML2 Statement Validator interface.
    /// </summary>
    public interface ISaml20StatementValidator
    {
        /// <summary>
        /// Validates the statement.
        /// </summary>
        /// <param name="statement">The statement.</param>
        void ValidateStatement(StatementAbstract statement,bool allowAnyAuthContextDeclRef);
    }
}
