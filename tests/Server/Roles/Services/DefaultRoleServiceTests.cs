using System;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using MySqlConnector;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

#pragma warning disable IDE0060, CA1801

namespace Brighid.Identity.Roles
{
    [TestFixture]
    [Category("Unit")]
    public class DefaultRoleServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class ValidateRoleDelegationsTests
        {
            [Test]
            [Auto]
            public void ShouldThrowRoleDelegationDeniedException_IfUserIsAttemptingToDelegateImpersonator_AndIsNotAnAdmin(
                ClaimsPrincipal principal,
                [Target] DefaultRoleService service
            )
            {
                Action func = () => service.ValidateRoleDelegations(new[] { nameof(BuiltInRole.Impersonator) }, principal);

                func.Should().Throw<RoleDelegationDeniedException>();
            }

            [Test]
            [Auto]
            public void ShouldNotThrowRoleDelegationDeniedException_IfUserIsAttemptingToDelegateImpersonator_AndIsAnAdmin(
                ClaimsPrincipal principal,
                [Target] DefaultRoleService service
            )
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Role, nameof(BuiltInRole.Administrator)));
                principal.AddIdentity(identity);

                Action func = () => service.ValidateRoleDelegations(new[] { nameof(BuiltInRole.Impersonator) }, principal);

                func.Should().NotThrow<RoleDelegationDeniedException>();
            }

            [Test]
            [Auto]
            public void ShouldThrowRoleDelegationDeniedException_IfUserIsAttemptingToDelegateAdmin_AndIsNotAdmin(
                ClaimsPrincipal principal,
                [Target] DefaultRoleService service
            )
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Role, nameof(BuiltInRole.Basic)));
                principal.AddIdentity(identity);

                Action func = () => service.ValidateRoleDelegations(new[] { nameof(BuiltInRole.Administrator) }, principal);

                func.Should().Throw<RoleDelegationDeniedException>();
            }

            [Test]
            [Auto]
            public void ShouldThrowRoleDelegationDeniedException_IfUserIsAttemptingToDelegateApplicationManager_AndIsNotAdmin(
                ClaimsPrincipal principal,
                [Target] DefaultRoleService service
            )
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Role, nameof(BuiltInRole.Basic)));
                principal.AddIdentity(identity);

                Action func = () => service.ValidateRoleDelegations(new[] { nameof(BuiltInRole.ApplicationManager) }, principal);

                func.Should().Throw<RoleDelegationDeniedException>();
            }

            [Test]
            [Auto]
            public void ShouldThrowRoleDelegationDeniedException_IfUserIsAttemptingToDelegateRoleManager_AndIsNotAdmin(
                ClaimsPrincipal principal,
                [Target] DefaultRoleService service
            )
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Role, nameof(BuiltInRole.Basic)));
                principal.AddIdentity(identity);

                Action func = () => service.ValidateRoleDelegations(new[] { nameof(BuiltInRole.RoleManager) }, principal);

                func.Should().Throw<RoleDelegationDeniedException>();
            }

            [Test]
            [Auto]
            public void ShouldNotThrowRoleDelegationDeniedException_IfUserIsAttemptingToDelegateBasic_AndIsApplicationManager(
                ClaimsPrincipal principal,
                [Target] DefaultRoleService service
            )
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Role, nameof(BuiltInRole.ApplicationManager)));
                principal.AddIdentity(identity);

                Action func = () => service.ValidateRoleDelegations(new[] { nameof(BuiltInRole.Basic) }, principal);

                func.Should().NotThrow<RoleDelegationDeniedException>();
            }

            [Test]
            [Auto]
            public void ShouldNotThrowRoleDelegationDeniedException_IfUserIsAttemptingToDelegateArbitraryRole_AndIsApplicationManager(
                string role,
                ClaimsPrincipal principal,
                [Target] DefaultRoleService service
            )
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Role, nameof(BuiltInRole.ApplicationManager)));
                principal.AddIdentity(identity);

                Action func = () => service.ValidateRoleDelegations(new[] { role }, principal);

                func.Should().NotThrow<RoleDelegationDeniedException>();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class ValidateUserHasRolesTests
        {
            [Test]
            [Auto]
            public void ShouldThrowRoleRequiredExceptionIfPrincipalDoesntHaveAllRoles(
                string role1,
                string role2,
                string role3,
                [Target] DefaultRoleService service
            )
            {
                var identity = new ClaimsIdentity();
                var principal = new ClaimsPrincipal(identity);
                identity.AddClaim(new Claim(ClaimTypes.Role, role1));
                identity.AddClaim(new Claim(ClaimTypes.Role, role2));

                Action func = () => service.ValidateUserHasRoles(new[] { role1, role2, role3 }, principal);

                func.Should().Throw<RoleRequiredException>().And.Role.Should().Be(role3);
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class CreateTests
        {
            [Test]
            [Auto]
            public async Task ShouldAddToTheRepository(
                RoleRequest role,
                Role expected,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] DefaultRoleService service
            )
            {
                repository.Add(Any<Role>()).Returns(expected);

                var result = await service.Create(role);

                result.Should().Be(expected);
                await repository.Received().Add(Is<Role>(receivedRole =>
                    receivedRole.Id == role.Id &&
                    receivedRole.Name == role.Name &&
                    receivedRole.NormalizedName == role.Name.ToUpper(CultureInfo.InvariantCulture)
                ));
            }

            [Test]
            [Auto]
            public async Task ShouldThrowRoleAlreadyExistsException_IfRoleAlreadyExists(
                RoleRequest role,
                Role expected,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] DefaultRoleService service
            )
            {
                var errorCode = MySqlErrorCode.DuplicateKeyEntry;
                var exception = (MySqlException)Activator.CreateInstance(typeof(MySqlException), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { errorCode, string.Empty }, null, null)!;
                repository.Add(Any<Role>()).Returns<Role>(x => throw new DbUpdateException(string.Empty, exception));

                Func<Task> func = () => service.Create(role);

                await func.Should().ThrowAsync<EntityAlreadyExistsException>();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class UpdateByIdTests
        {
            [Test]
            [Auto]
            public async Task ShouldThrowExceptionIfRoleNotFound(
                Guid id,
                RoleRequest updatedRoleInfo,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] DefaultRoleService service
            )
            {
                repository.FindById(Any<Guid>()).Returns((Role)null!);

                Func<Task> func = () => service.UpdateById(id, updatedRoleInfo);

                await func.Should().ThrowAsync<EntityNotFoundException>();
            }

            [Test]
            [Auto]
            public async Task ShouldThrowExceptionIfNameChanged(
                Guid id,
                RoleRequest updatedRoleInfo,
                Role existingRole,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] DefaultRoleService service
            )
            {
                repository.FindById(Any<Guid>()).Returns(existingRole);

                Func<Task> func = () => service.UpdateById(id, updatedRoleInfo);

                await func.Should().ThrowAsync<NotSupportedException>();
            }

            [Test]
            [Auto]
            public async Task ShouldSavetoTheDatabaseWithUpdatedInfo(
                Guid id,
                RoleRequest updatedRoleInfo,
                Role existingRole,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] DefaultRoleService service
            )
            {
                updatedRoleInfo.Name = existingRole.Name;
                repository.FindById(Any<Guid>()).Returns(existingRole);

                var result = await service.UpdateById(id, updatedRoleInfo);

                await repository.Received().Save(Is<Role>(role =>
                    role.Id == existingRole.Id &&
                    role.Name == existingRole.Name &&
                    role.Description == updatedRoleInfo.Description
                ));
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class DeleteByIdTests
        {
            [Test]
            [Auto]
            public async Task ShouldThrowIfRoleIsAttachedToAPrincipal(
                Guid id,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] DefaultRoleService service
            )
            {
                repository.IsAttachedToAPrincipal(Any<Guid>()).Returns(true);

                Func<Task> func = () => service.DeleteById(id);

                await func.Should().ThrowAsync<NotSupportedException>();
            }

            [Test]
            [Auto]
            public async Task ShouldDeleteTheRoleFromTheRepository(
                Guid id,
                Role role,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] DefaultRoleService service
            )
            {
                repository.IsAttachedToAPrincipal(Any<Guid>()).Returns(false);
                repository.Remove(Any<Guid>()).Returns(role);

                var result = await service.DeleteById(id);

                result.Should().Be(role);
                await repository.Received().Remove(Is(id));
            }
        }
    }
}
