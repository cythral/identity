using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Roles;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

using static NSubstitute.Arg;

#pragma warning disable IDE0060, CA1801

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationRoleServiceTests
    {
        public class UpdateApplicationRolesTests
        {
            private static readonly List<ApplicationRole> ExistingRoles = new()
            {
                new ApplicationRole
                {
                    Role = new Role { NormalizedName = "A" }
                },

                new ApplicationRole
                {
                    Role = new Role { NormalizedName = "B" }
                }
            };

            private static readonly List<ApplicationRole> UpdatedRoles = new()
            {
                new ApplicationRole
                {
                    Role = new Role { Name = "A" }
                },

                new ApplicationRole
                {
                    Role = new Role { Name = "C" }
                }
            };

            [Test, Auto]
            public void ShouldRemoveRolesNotInTheUpdatedList(
                Application application,
                [Frozen, Substitute] IApplicationRoleRepository repository,
                [Target] DefaultApplicationRoleService service
            )
            {
                application.Roles = new List<ApplicationRole>(ExistingRoles);

                service.UpdateApplicationRoles(application, UpdatedRoles);

                application.Roles.Should().NotContain(appRole => appRole.Role.NormalizedName == "B");
                repository.Received().TrackAsDeleted(Is<ApplicationRole>(appRole =>
                    appRole.Role.NormalizedName == "B"
                ));
            }

            [Test, Auto]
            public void ShouldAddRolesNotInTheExistingList(
                Application application,
                [Frozen, Substitute] IApplicationRoleRepository repository,
                [Target] DefaultApplicationRoleService service
            )
            {
                application.Roles = new List<ApplicationRole>(ExistingRoles);

                service.UpdateApplicationRoles(application, UpdatedRoles);

                application.Roles.Should().Contain(appRole => appRole.Role.Name == "C");
            }
        }
    }
}
