using System;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using OpenIddict.Server;

using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

using SR = OpenIddict.Abstractions.OpenIddictResources;

namespace Brighid.Identity.Auth
{
    public class ValidateAccessTokenParameter : IOpenIddictServerHandler<ValidateTokenRequestContext>
    {
        private static readonly string[] RequiredRoles = new[] { nameof(BuiltInRole.Impersonator) };
        private readonly IAuthService authService;
        private readonly IRoleService roleService;

        public ValidateAccessTokenParameter(
           IAuthService authService,
           IRoleService roleService
       )
        {
            this.authService = authService;
            this.roleService = roleService;
        }

        /// <summary>
        /// Gets the default descriptor definition assigned to this handler.
        /// </summary>
        public static OpenIddictServerHandlerDescriptor Descriptor { get; }
            = OpenIddictServerHandlerDescriptor.CreateBuilder<ValidateTokenRequestContext>()
                .UseScopedHandler<ValidateAccessTokenParameter>()
                .SetOrder(ValidateClientIdParameter.Descriptor.Order + 1_000)
                .SetType(OpenIddictServerHandlerType.Custom)
                .Build();

        /// <inheritdoc/>
        public ValueTask HandleAsync(ValidateTokenRequestContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Request.GrantType != Constants.GrantTypes.Impersonate)
            {
                return default;
            }

            try
            {
                context.Principal = authService.ExtractPrincipalFromRequestContext(context);
                roleService.ValidateUserHasRoles(RequiredRoles, context.Principal);
            }
            catch (RoleRequiredException)
            {
                var name = context.Principal?.Identity?.Name ?? "Unknown";
                context.Reject($"Client {name} is not allowed to use the 'impersonate' grant type.");
            }
            catch (InvalidAccessTokenException)
            {
                context.Reject(
                    error: Errors.InvalidRequest,
                    description: SR.FormatID2052(Parameters.AccessToken),
                    uri: SR.FormatID8000(SR.ID2052)
                );
            }

            return default;
        }
    }
}
