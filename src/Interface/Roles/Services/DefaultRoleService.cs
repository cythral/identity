using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Interface.Roles
{
    /// <inheritdoc />
    public class DefaultRoleService : IRoleService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions serializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRoleService" /> class.
        /// </summary>
        /// <param name="httpClient">Client to use for making HTTP requests.</param>
        public DefaultRoleService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        /// <inheritdoc />
        public Task<Role> Create(RoleRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Role> DeleteById(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Role?> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Role?> GetByName(string name, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Role>> List(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await httpClient.GetFromJsonAsync<Role[]>("/api/roles", serializerOptions, cancellationToken);
            return result ?? Array.Empty<Role>();
        }

        /// <inheritdoc />
        public Task<Role> UpdateById(Guid id, RoleRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ValidateRoleDelegations(IEnumerable<string> roles, ClaimsPrincipal principal)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ValidateUserHasRoles(IEnumerable<string> roles, ClaimsPrincipal principal)
        {
            throw new NotImplementedException();
        }
    }
}
