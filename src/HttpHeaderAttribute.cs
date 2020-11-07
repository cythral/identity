using System;
using System.Linq;

using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Brighid.Identity
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpHeaderAttribute : Attribute, IActionConstraint
    {
        public int Order => 0;
        private string Header { get; }
        private string Value { get; }

        public HttpHeaderAttribute(string header, string value)
        {
            Header = header;
            Value = value;
        }

        public bool Accept(ActionConstraintContext context)
        {
            var headers = context.RouteContext.HttpContext.Request.Headers;
            headers.TryGetValue(Header, out var value);
            return value.FirstOrDefault() == Value;
        }
    }
}
