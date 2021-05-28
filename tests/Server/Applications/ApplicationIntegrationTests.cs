using System;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Brighid.Identity.Roles;
using Brighid.Identity.Sns;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

#pragma warning disable CA1031, CA1303, SA1117

namespace Brighid.Identity.Applications
{
    [Category("Integration")]
    public class ApplicationIntegrationTests
    {
        [TestFixture]
        [Category("Integration")]
        public class CloudFormationCustomResourceTests
        {
            [Test]
            [Auto]
            public async Task CreateThenUpdateThenDelete(
                string createRequestId,
                string updateRequestId,
                string deleteRequestId,
                string name,
                string description,
                AppFactory app
            )
            {
                await EnsureValidAwsCredentials(app);

                Guid applicationId;
                string encryptedSecret;

                var serial = 0UL;
                var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                var client = app.CreateClient();
                client.DefaultRequestHeaders.Add("x-amz-sns-message-type", "Notification");
                {
                    var response = await client.PostAsJsonAsync(ApplicationController.BasePath, new SnsMessage<CloudFormationRequest<ApplicationRequest>>
                    {
                        Message = new CloudFormationRequest<ApplicationRequest>
                        {
                            RequestId = createRequestId,
                            RequestType = CloudFormationRequestType.Create,
                            ResponseURL = new Uri($"{app.RootUri}mock"),
                            ResourceProperties = new ApplicationRequest
                            {
                                Name = name,
                                Description = description,
                                Serial = serial,
                                Roles = new[] { nameof(BuiltInRole.Basic) },
                            },
                        },
                    },
                    options
                );

                    response.EnsureSuccessStatusCode();
                }

                {
                    var calls = app.Services.GetRequiredService<MockControllerCalls>();

                    calls.Should().Contain(response =>
                        response.RequestId == createRequestId &&
                        response.Status == CloudFormationResponseStatus.SUCCESS
                    );
                    calls.Clear();
                }

                {
                    using var scope = app.Services.CreateScope();
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    var query = from application in databaseContext.Applications.AsQueryable() where application.Name == name select application;
                    var result = await query.FirstAsync();

                    applicationId = result.Id;
                    encryptedSecret = result.EncryptedSecret;

                    encryptedSecret.Should().NotBeNull();
                }

                {
                    var response = await client.PostAsJsonAsync(ApplicationController.BasePath, new SnsMessage<CloudFormationRequest<ApplicationRequest>>
                    {
                        Message = new CloudFormationRequest<ApplicationRequest>
                        {
                            RequestId = updateRequestId,
                            RequestType = CloudFormationRequestType.Update,
                            PhysicalResourceId = applicationId.ToString(),
                            ResponseURL = new Uri($"{app.RootUri}mock"),
                            ResourceProperties = new ApplicationRequest
                            {
                                Name = name,
                                Description = description,
                                Serial = ++serial,
                                Roles = new[] { nameof(BuiltInRole.Basic) },
                            },
                        },
                    },
                    options);

                    response.EnsureSuccessStatusCode();
                }

                {
                    var calls = app.Services.GetRequiredService<MockControllerCalls>();

                    calls.Should().Contain(response =>
                        response.RequestId == updateRequestId &&
                        response.Status == CloudFormationResponseStatus.SUCCESS
                    );
                    calls.Clear();
                }

                {
                    using var scope = app.Services.CreateScope();
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    var query = from application in databaseContext.Applications.AsQueryable() where application.Id == applicationId select application;
                    var result = await query.FirstAsync();

                    result.EncryptedSecret.Should().NotBe(encryptedSecret);
                    result.Serial.Should().Be(serial);
                }

                {
                    var response = await client.PostAsJsonAsync(ApplicationController.BasePath, new SnsMessage<CloudFormationRequest<ApplicationRequest>>
                    {
                        Message = new CloudFormationRequest<ApplicationRequest>
                        {
                            RequestId = deleteRequestId,
                            RequestType = CloudFormationRequestType.Delete,
                            PhysicalResourceId = applicationId.ToString(),
                            ResponseURL = new Uri($"{app.RootUri}mock"),
                            ResourceProperties = new ApplicationRequest
                            {
                                Name = name,
                                Description = description,
                                Serial = serial,
                                Roles = new[] { nameof(BuiltInRole.Basic) },
                            },
                        },
                    }, options);

                    response.EnsureSuccessStatusCode();
                }

                {
                    var calls = app.Services.GetRequiredService<MockControllerCalls>();

                    calls.Should().Contain(response =>
                        response.RequestId == deleteRequestId &&
                        response.Status == CloudFormationResponseStatus.SUCCESS
                    );
                    calls.Clear();
                }

                {
                    using var scope = app.Services.CreateScope();
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    var query = from application in databaseContext.Applications.AsQueryable() where application.Id == applicationId select application;
                    var exists = await query.AnyAsync();

                    exists.Should().Be(false);
                }
            }

            [Test]
            [Auto]
            public async Task CreateWithUnknownRoleFails(
                string createRequestId,
                string name,
                string randomRoleName,
                string description,
                AppFactory app
            )
            {
                await EnsureValidAwsCredentials(app);

                var serial = 0UL;
                var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                var client = app.CreateClient();
                client.DefaultRequestHeaders.Add("x-amz-sns-message-type", "Notification");
                {
                    var response = await client.PostAsJsonAsync(ApplicationController.BasePath, new SnsMessage<CloudFormationRequest<ApplicationRequest>>
                    {
                        Message = new CloudFormationRequest<ApplicationRequest>
                        {
                            RequestId = createRequestId,
                            RequestType = CloudFormationRequestType.Create,
                            ResponseURL = new Uri($"{app.RootUri}mock"),
                            ResourceProperties = new ApplicationRequest
                            {
                                Name = name,
                                Description = description,
                                Serial = serial,
                                Roles = new[] { randomRoleName },
                            },
                        },
                    }, options);

                    response.StatusCode.Should().Be(400);
                }

                {
                    var calls = app.Services.GetRequiredService<MockControllerCalls>();

                    calls.Should().Contain(response =>
                        response.RequestId == createRequestId &&
                        response.Status == CloudFormationResponseStatus.FAILED
                    );
                    calls.Clear();
                }
            }

            private async Task EnsureValidAwsCredentials(AppFactory app)
            {
                try
                {
                    var encryptionService = app.Services.GetRequiredService<IEncryptionService>();
                    await encryptionService.Encrypt("plaintext");
                }
                catch (Exception)
                {
                    Assert.Ignore("This test requires valid AWS Credentials with kms:Encrypt allowed.");
                }
            }
        }
    }
}
