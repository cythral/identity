using System;
using System.Globalization;
using System.Reflection;
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
    [TestFixture, Category("Unit")]
    public class DefaultRoleServiceTests
    {
        [TestFixture, Category("Unit")]
        public class GetPrimaryKeyTests
        {
            [Test, Auto]
            public void ShouldReturnId(
                Role role,
                [Target] DefaultRoleService service
            )
            {
                var result = service.GetPrimaryKey(role);

                result.Should().Be(role.Id);
            }
        }

        [TestFixture, Category("Unit")]
        public class CreateTests
        {
            [Test, Auto]
            public async Task ShouldAddToTheRepository(
                Role role,
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

            [Test, Auto]
            public async Task ShouldThrowRoleAlreadyExistsException_IfRoleAlreadyExists(
                Role role,
                Role expected,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] DefaultRoleService service
            )
            {
                var errorCode = MySqlErrorCode.DuplicateKeyEntry;
                var exception = (MySqlException)Activator.CreateInstance(typeof(MySqlException), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { errorCode, "" }, null, null)!;
                repository.Add(Any<Role>()).Returns<Role>(x => throw new DbUpdateException("", exception));

                Func<Task> func = () => service.Create(role);

                await func.Should().ThrowAsync<EntityAlreadyExistsException>();
            }
        }

        [TestFixture, Category("Unit")]
        public class UpdateByIdTests
        {
            [Test, Auto]
            public async Task ShouldThrowExceptionIfRoleNotFound(
                Guid id,
                Role updatedRoleInfo,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] DefaultRoleService service
            )
            {
                repository.FindById(Any<Guid>()).Returns((Role)null!);

                Func<Task> func = () => service.UpdateById(id, updatedRoleInfo);

                await func.Should().ThrowAsync<EntityNotFoundException>();
            }

            [Test, Auto]
            public async Task ShouldThrowExceptionIfNameChanged(
                Guid id,
                Role updatedRoleInfo,
                Role existingRole,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] DefaultRoleService service
            )
            {
                repository.FindById(Any<Guid>()).Returns(existingRole);

                Func<Task> func = () => service.UpdateById(id, updatedRoleInfo);

                await func.Should().ThrowAsync<NotSupportedException>();
            }

            [Test, Auto]
            public async Task ShouldSavetoTheDatabaseWithUpdatedInfo(
                Guid id,
                Role updatedRoleInfo,
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

        [TestFixture, Category("Unit")]
        public class DeleteByIdTests
        {
            [Test, Auto]
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

            [Test, Auto]
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
