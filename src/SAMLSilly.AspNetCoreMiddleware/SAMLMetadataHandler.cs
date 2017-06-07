using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAMLSilly.Config;

namespace SAMLSilly.AspNetCore
{
    public class SAMLMetadataHandler
    {
        public IActionResult Handle(Saml2Configuration config) => new SAMLMetadataResult(config);
    }
}