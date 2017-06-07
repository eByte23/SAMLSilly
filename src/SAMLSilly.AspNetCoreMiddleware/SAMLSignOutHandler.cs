using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SAMLSilly.AspNetCore.Models;
using SAMLSilly.Config;

namespace SAMLSilly.AspNetCore
{
    public class SAMLSignOutHandler
    {
        public SAMLSignOutHandler()
        {
        }

        public Task<IActionResult> Handle(Saml2Configuration config, SAMLInputModel input)
        {
            return Task.Factory.StartNew<IActionResult>(() => { return new ContentResult(); });
        }
    }
}