using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SAMLSilly;

namespace SAMLSilly.AspNetCore.Core
{
    public interface IProcessSAMLAssertion
    {
        Task<IActionResult> ProcessAsync(Saml20Assertion assertion);
        IActionResult Process(Saml20Assertion assertion);
    }
}