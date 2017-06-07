using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using SAMLSilly.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System.IO;
using SAMLSilly.AspNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;

namespace SAMLSilly.AspNetCore.Middleware
{
    public static class SamlMiddlewareExtentions
    {
        public static PathString SAML_PATH_STRING = new PathString("/auth/saml2");
        public const string SAML_PATH = "/auth/saml2";
        public const string SAML_MEATADATA_PATH = "/metadata";
        public const string SAML_ERROR_ERROR = "/error";

        public static IApplicationBuilder UseSamlAuthentication(this IApplicationBuilder app, CookieAuthenticationOptions options)
        {
            app.UseCookieAuthentication(options);

            app.IncludeSamlEndpoints();

            return app;
        }

        public static IApplicationBuilder IncludeSamlEndpoints(this IApplicationBuilder app)
        {
            app.Map(SAML_PATH_STRING, (x) =>
            {
                x.Map(SAML_ERROR_ERROR, (y) =>
                {
                    y.Use(async (w, z) =>
                    {
                        await w.Response.WriteAsync($"errrrrrroooorrr: {w.Request.Query["error"]}");
                    });
                });

                x.Map(SAML_MEATADATA_PATH, (y) =>
                {
                    y.UseMiddleware<SamlMetadataEndpointMiddleware>();
                });

                x.UseMiddleware<SamlAuthenticationEndpointMiddlware>();
            });

            return app;
        }

    }

    public class SamlMetadataEndpointMiddleware
    {
        private readonly RequestDelegate _next;

        public SamlMetadataEndpointMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context, Saml2Configuration config)
        {

            var metadata = new SAMLMetadataResult(config);

            context.Response.StatusCode = 200;
            context.Response.ContentType = metadata.ContentType;
#if !DEBUG
            context.Response.Headers.Add("Content-Disposition", "attachment; filename=\"metadata.xml\"");
#endif
            context.Response.WriteAsync(metadata.Content, encoding: System.Text.Encoding.UTF8);


            return Task.CompletedTask;
        }
    }

    public class SamlAuthenticationEndpointMiddlware
    {
        private readonly RequestDelegate _next;

        public SamlAuthenticationEndpointMiddlware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context, Saml2Configuration config)
        {
            var a ="4f87944e-fcfe-4824-b297-d8b922870d80";
            context.Items.Add("ClientId", a);
            var samlInput = new Models.SAMLInputModel().BindModel(context);
            SAMLSignOnHandler handler = (SAMLSignOnHandler)context.RequestServices.GetService(typeof(SAMLSignOnHandler));

            var result = handler.Handle(config, samlInput);

            if (result == null || (result is ContentResult && ((ContentResult)result).StatusCode == 500))
            {
                context.Response.Redirect($"{SamlMiddlewareExtentions.SAML_PATH}{SamlMiddlewareExtentions.SAML_ERROR_ERROR}?error={UrlEncoder.Default.Encode(((ContentResult)result).Content)}");
                return Task.CompletedTask;
            }

            result.ExecuteResultAsync(new ActionContext(context, new RouteData(), new ActionDescriptor())).Wait();


            return Task.CompletedTask;
            //return this._next(context);
        }
    }
}