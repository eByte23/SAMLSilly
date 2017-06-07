using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAMLSilly.AspNetCore.Models
{
    public class SAMLInputModel
    {
        public string SAMLResponse { get; set; }
        public string SAMLRequest { get; set; }
        public string SAMLart { get; set; }
        public string Signature { get; set; }
        public string SigAlg { get; set; }
        public string RelayState { get; set; }
    }

    public static class SAMLInputModelExtensions
    {
        public static bool HasResponse(this SAMLInputModel input)
        {
            return !String.IsNullOrEmpty(input?.SAMLResponse);
        }

        public static SAMLInputModel BindModel(this SAMLInputModel model, HttpContext context)
        {
            var isPost = context.Request.Method == "POST";

            List<KeyValuePair<string, string>> body = null;

            if (!context.Request.HasFormContentType)
            {
                 body = GetBodyValues(context);
            }

            model.SetValue("SAMLRequest", context, isPost, body, (x, v) => x.SAMLRequest = v);
            model.SetValue("SAMLResponse", context, isPost, body, (x, v) => x.SAMLResponse = v);
            model.SetValue("SAMLart", context, isPost, body, (x, v) => x.SAMLart = v);
            model.SetValue("RelayState", context, isPost, body, (x, v) => x.RelayState = v);
            model.SetValue("SigAlg", context, isPost, body, (x, v) => x.SigAlg = v);
            model.SetValue("Signature", context, isPost, body, (x, v) => x.Signature = v);

            return model;
        }

        public static void SetValue(this SAMLInputModel model, string key, HttpContext context, bool isPost, List<KeyValuePair<string, string>> body, Action<SAMLInputModel, string> setAction)
        {
            if (body != null && isPost)
            {
                foreach (var a in body)
                {
                    if (a.Key != key) continue;

                    if (!string.IsNullOrEmpty(a.Value))
                    {
                        setAction(model, a.Value);
                        break;
                    }
                }

                return;
            }

            StringValues value;
            if (context.TryGetValue(key, out value, isPost))
            {
                foreach (var a in value)
                {
                    if(!string.IsNullOrEmpty(a))
                    {
                        setAction(model, a);
                        break;
                    }
                }
            }
        }


        public static bool TryGetValue(this HttpContext context, string key, out StringValues value, bool isPost)
        {
            if (isPost)
            {
                bool? result = context.Request.Form?.TryGetValue(key, out value);
                return result.HasValue;
            }
            else
            {
                bool? result = context.Request.Query?.TryGetValue(key, out value);
                return result.HasValue;
            }
        }

        public static List<KeyValuePair<string, string>> GetBodyValues(this HttpContext context)
        {
            List<KeyValuePair<string, string>> bodyValues = new List<KeyValuePair<string, string>>();
            using (StreamReader reader = new StreamReader(context.Request.Body))
            {
                var bodyString = reader.ReadToEnd();
                var valuePairs = bodyString?.Split('&');
                if (valuePairs != null)
                {
                    foreach (var valuePair in valuePairs)
                    {
                        string[] values = valuePair?.Split('=');
                        if (values?.Count() == 2)
                        {
                            bodyValues.Add(new KeyValuePair<string, string>(values[0], values[1]));
                        }
                    }
                }
            }

            return bodyValues;
        }
    }

}