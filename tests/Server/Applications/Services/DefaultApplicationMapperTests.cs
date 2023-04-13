using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Roles;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationMapperTests
    {
        [TestFixture]
        public class MapRequestToEntity
        {
            [Test]
            [Auto]
            public async Task ShouldFindAllRolesByName(
                ApplicationRequest request,
                [Frozen, Substitute] IRoleRepository roleRepository,
                [Target] DefaultApplicationMapper mapper
            )
            {
                var roles = (from role in request.Roles select new Role { Name = role }).ToList();
                var cancellationToken = new CancellationToken(false);

                roleRepository.FindAllByName(Any<IEnumerable<string>>(), Any<CancellationToken>()).Returns(roles);

                var result = await mapper.MapRequestToEntity(request, cancellationToken);

                result.Roles.Should().BeEquivalentTo(roles);
                await roleRepository.Received().FindAllByName(Is(request.Roles), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldThrowIfAnyRolesWereNotFound(
                ApplicationRequest request,
                [Frozen, Substitute] IRoleRepository roleRepository,
                [Target] DefaultApplicationMapper mapper
            )
            {
                var roles = (from role in request.Roles select new Role { Name = role }).ToList();
                var missingRole = roles[0];
                roles.RemoveAt(0);

                var cancellationToken = new CancellationToken(false);

                roleRepository.FindAllByName(Any<IEnumerable<string>>(), Any<CancellationToken>()).Returns(roles);

                Func<Task> func = () => mapper.MapRequestToEntity(request, cancellationToken);

                (await func.Should().ThrowAsync<AggregateException>())
                .And
                .InnerExceptions.Should().ContainEquivalentOf(new RoleNotFoundException(missingRole.Name!));
            }
        }
    }
}
