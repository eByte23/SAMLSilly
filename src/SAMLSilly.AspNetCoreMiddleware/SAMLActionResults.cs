using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SAMLSilly;
using SAMLSilly.Config;

namespace SAMLSilly.AspNetCore
{
    public class SAMLMetadataResult : ContentResult
    {
        public SAMLMetadataResult(Saml2Configuration config)
        {
            var metadata = new SAMLSilly.Saml20MetadataDocument().Load(config);
            metadata.Sign = true;
            StatusCode = (int)HttpStatusCode.OK;

            ContentType = Saml20Constants.MetadataMimetype;
#if DEBUG
            ContentType = "text/xml";
#endif
            Content = metadata.ToXml(Encoding.UTF8, config.ServiceProvider.SigningCertificate, config.SigningAlgorithm);
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
#if !DEBUG
            context.HttpContext.Response.Headers.Add("Content-Disposition", "attachment; filename=\"metadata.xml\"");
#endif
            return base.ExecuteResultAsync(context);
        }
    }
}
