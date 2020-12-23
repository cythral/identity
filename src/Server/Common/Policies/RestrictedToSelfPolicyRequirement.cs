using System;

using Microsoft.AspNetCore.Authorization;

namespace Brighid.Identity
{
    public class RestrictedToSelfPolicyRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Restricts an action which manipulates a user to the logged-in user only, unless the logged-in user is an Administrator.
        /// This is done by matching a claim value to a route parameter value.
        /// </summary>
        /// <param name="routeParameter"></param>
        public RestrictedToSelfPolicyRequirement(string routeParameter, string claimType, Func<string?, object?> parse)
        {
            RouteParameter = routeParameter;
            ClaimType = claimType;
            Parse = parse;
        }

        public string RouteParameter { get; }

        public string ClaimType { get; }

        public Func<string?, object?> Parse { get; }
    }
}
