using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SAMLSilly;

namespace SAMLSilly.AspNetCore.Models
{
    public class SAMLSignOnResponseModel
    {
        public IActionResult Result { get; set; }

        public Saml20Assertion Assertion { get; set; }

        public bool SuccessfulSignOn { get; set; }
    }
}