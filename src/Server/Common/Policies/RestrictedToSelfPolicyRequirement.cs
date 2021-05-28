using System;

using Microsoft.AspNetCore.Authorization;

namespace Brighid.Identity
{
    public class RestrictedToSelfPolicyRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictedToSelfPolicyRequirement" /> class. Restricts an action which manipulates a user to the logged-in user only, unless the logged-in user is an Administrator.
        /// This is done by matching a claim value to a route parameter value.
        /// </summary>
        /// <param name="routeParameter">Route parameter to use.</param>
        /// <param name="claimType">Type of claim to use.</param>
        /// <param name="parse">Parse function to use.</param>
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
