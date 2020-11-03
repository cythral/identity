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
            if (context.RouteContext.HttpContext.Request.Headers.TryGetValue(Header, out var value) && value.Any())
            {
                return value[0] == Value;
            }

            return false;
        }
    }
}
