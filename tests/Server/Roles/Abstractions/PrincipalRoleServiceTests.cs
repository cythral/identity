using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

#pragma warning disable IDE0060, CA1801, CA1040, IDE0022

namespace Brighid.Identity.Roles
{
    public class TestPrincipal : IPrincipalWithRoles<TestPrincipal, TestPrincipalRole>
    {
        public string Name { get; set; }

        public ICollection<TestPrincipalRole> Roles { get; set; }

        public virtual TestPrincipalRole? GetRoleJoin(string name) => null;
    }

    public class TestPrincipalRole : IPrincipalRoleJoin<TestPrincipal>
    {
        public TestPrincipal Principal { get; set; }

        public Role Role { get; set; }
    }

    public interface IPrincipalRoleRepository : IRepository<TestPrincipalRole, Guid>
    {

    }

    public class TestPrincipalRoleService : PrincipalRoleService<TestPrincipal, Guid, TestPrincipalRole, IPrincipalRoleRepository>
    {
        public TestPrincipalRoleService(
            IPrincipalRoleRepository principalRoleRepository,
            IRoleRepository roleRepository
        ) : base(principalRoleRepository, roleRepository)
        {
        }
    }

    [Category("Unit")]
    public class PrincipalRoleServiceTests
    {
        [Category("Unit")]
        public class UpdatePrincipalRolesTests
        {
            private static readonly List<TestPrincipalRole> ExistingRoles = new()
            {
                new TestPrincipalRole
                {
                    Role = new Role { NormalizedName = "A" }
                },

                new TestPrincipalRole
                {
                    Role = new Role { NormalizedName = "B" }
                }
            };

            private static readonly List<TestPrincipalRole> UpdatedRoles = new()
            {
                new TestPrincipalRole
                {
                    Role = new Role { Name = "A" }
                },

                new TestPrincipalRole
                {
                    Role = new Role { Name = "C" }
                }
            };

            [Test, Auto]
            public async Task ShouldRemoveRolesNotInTheUpdatedList(
                [Substitute] TestPrincipal principal,
                [Frozen, Substitute] IRoleRepository roleRepository,
                [Frozen, Substitute] IPrincipalRoleRepository repository,
                [Target] TestPrincipalRoleService service
            )
            {
                principal.Roles = new List<TestPrincipalRole>(ExistingRoles);
                roleRepository.FindByName(Any<string>()).Returns(x => new Role { Name = x.ArgAt<string>(0) });
                principal.GetRoleJoin(Any<string>()).Returns((TestPrincipalRole)null!);

                await service.UpdatePrincipalRoles(principal, UpdatedRoles);

                principal.Roles.Should().NotContain(appRole => appRole.Role.NormalizedName == "B");
                repository.Received().TrackAsDeleted(Is<TestPrincipalRole>(appRole =>
                    appRole.Role.NormalizedName == "B"
                ));
            }

            [Test, Auto]
            public async Task ShouldAddRolesNotInTheExistingList(
                [Substitute] TestPrincipal principal,
                [Frozen, Substitute] IRoleRepository roleRepository,
                [Frozen, Substitute] IPrincipalRoleRepository repository,
                [Target] TestPrincipalRoleService service
            )
            {
                principal.Roles = new List<TestPrincipalRole>(ExistingRoles);
                roleRepository.FindByName(Any<string>()).Returns(x => new Role { Name = x.ArgAt<string>(0) });
                principal.GetRoleJoin(Any<string>()).Returns((TestPrincipalRole)null!);

                await service.UpdatePrincipalRoles(principal, UpdatedRoles);

                principal.Roles.Should().Contain(appRole => appRole.Role.Name == "C");
            }
        }

        [Category("Unit")]
        public class AddRoleToPrincipalTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancellationWasRequested(
                TestPrincipal application,
                string roleName,
                [Target] TestPrincipalRoleService service
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => service.AddRoleToPrincipal(application, roleName, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldThrowIfRoleDoesntExist(
                TestPrincipal application,
                string roleName,
                [Substitute, Frozen] IRoleRepository roleRepository,
                [Target] TestPrincipalRoleService service
            )
            {
                var cancellationToken = new CancellationToken(false);
                roleRepository.FindByName(Any<string>(), Any<CancellationToken>()).Returns((Role)null!);

                Func<Task> func = () => service.AddRoleToPrincipal(application, roleName, cancellationToken);

                await func.Should().ThrowAsync<RoleNotFoundException>();
                await roleRepository.Received().FindByName(Is(roleName), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfRoleExists(
                Role role,
                TestPrincipal application,
                string roleName,
                [Substitute, Frozen] IRoleRepository roleRepository,
                [Target] TestPrincipalRoleService service
            )
            {
                var cancellationToken = new CancellationToken(false);
                roleRepository.FindByName(Any<string>(), Any<CancellationToken>()).Returns(role);

                Func<Task> func = () => service.AddRoleToPrincipal(application, roleName, cancellationToken);

                await func.Should().NotThrowAsync<RoleNotFoundException>();
                await roleRepository.Received().FindByName(Is(roleName), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldThrowIfPrincipalAlreadyHasExistingAppRoleWithGivenName(
                string roleName,
                TestPrincipalRole roleJoin,
                [Substitute] TestPrincipal principal,
                [Substitute, Frozen] IRoleRepository roleRepository,
                [Target] TestPrincipalRoleService service
            )
            {
                var cancellationToken = new CancellationToken(false);

                principal.GetRoleJoin(Any<string>()).Returns(roleJoin);

                Func<Task> func = () => service.AddRoleToPrincipal(principal, roleName, cancellationToken);

                await func.Should().ThrowAsync<PrincipalAlreadyHasRoleException>();

                principal.Received().GetRoleJoin(Is(roleName));
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfApplicationDoesntAlreadyHaveExistingAppRoleWithGivenName(
                string roleName,
                TestPrincipalRole roleJoin,
                [Substitute] TestPrincipal principal,
                [Substitute, Frozen] IRoleRepository roleRepository,
                [Target] TestPrincipalRoleService service
            )
            {
                var cancellationToken = new CancellationToken(false);

                roleRepository.FindByName(Any<string>(), Any<CancellationToken>()).Returns((Role)null!);
                principal.GetRoleJoin(Any<string>()).Returns((TestPrincipalRole)null!);

                Func<Task> func = () => service.AddRoleToPrincipal(principal, roleName, cancellationToken);

                await func.Should().NotThrowAsync<PrincipalAlreadyHasRoleException>();

                principal.Received().GetRoleJoin(Is(roleName));
            }

            [Test, Auto]
            public async Task ShouldAddNewAppRoleToApplication(
                string roleName,
                Role role,
                TestPrincipalRole roleJoin,
                [Substitute] TestPrincipal principal,
                [Substitute, Frozen] IRoleRepository roleRepository,
                [Target] TestPrincipalRoleService service
            )
            {
                var cancellationToken = new CancellationToken(false);

                roleRepository.FindByName(Any<string>(), Any<CancellationToken>()).Returns(role);
                principal.GetRoleJoin(Any<string>()).Returns((TestPrincipalRole)null!);

                await service.AddRoleToPrincipal(principal, roleName, cancellationToken);

                principal.Roles.Should().Contain(newRole => newRole.Role == role);
            }
        }
    }
}
