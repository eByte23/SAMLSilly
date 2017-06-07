using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAMLSilly.AspNetCore.Utils
{
    public static class HttpUtils
    {
        public const string HTTP_GET = "GET";
        public const string HTTP_POST = "POST";

        public static bool IsGet(this HttpContext context)
            => context.Request.Method == HTTP_GET;

        public static bool IsPost(this HttpContext context)
            => context.Request.Method == HTTP_POST;

        public static bool IsGetOrPost(this HttpContext context)
            => context.IsGet() || context.IsPost();
    }
}
