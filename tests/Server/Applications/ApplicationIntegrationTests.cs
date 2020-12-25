using System;
using System.Collections.Generic;
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

#pragma warning disable CA1031, CA1303

namespace Brighid.Identity.Applications
{
    [Category("Integration")]
    public class ApplicationsIntegrationTests
    {
        private const string EndpointUrl = "/api/applications";

        [TestFixture, Category("Integration")]
        public class CloudFormationCustomResourceTests
        {
            [Test, Auto]
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

                #region Create Application
                {
                    var response = await client.PostAsJsonAsync(EndpointUrl, new SnsMessage<CloudFormationRequest<Application>>
                    {
                        Message = new CloudFormationRequest<Application>
                        {
                            RequestId = createRequestId,
                            RequestType = CloudFormationRequestType.Create,
                            ResponseURL = new Uri($"{app.RootUri}mock"),
                            ResourceProperties = new Application
                            {
                                Name = name,
                                Description = description,
                                Serial = serial,
                                Roles = new List<ApplicationRole>
                                {
                                    new ApplicationRole { Role = new Role { Name = nameof(BuiltInRole.Basic) } }
                                }
                            }
                        }
                    }, options);

                    response.EnsureSuccessStatusCode();
                }
                #endregion

                #region Ensure Successful Response Received
                {
                    var calls = app.Services.GetRequiredService<MockControllerCalls>();

                    calls.Should().Contain(response =>
                        response.RequestId == createRequestId &&
                        response.Status == CloudFormationResponseStatus.SUCCESS
                    );
                    calls.Clear();
                }
                #endregion

                #region Ensure Application Exists in Database
                {
                    using var scope = app.Services.CreateScope();
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    var query = from application in databaseContext.Applications.AsQueryable() where application.Name == name select application;
                    var result = await query.FirstAsync();

                    applicationId = result.Id;
                    encryptedSecret = result.EncryptedSecret;

                    encryptedSecret.Should().NotBeNull();
                }
                #endregion

                #region Update Application Serial
                {
                    var response = await client.PostAsJsonAsync(EndpointUrl, new SnsMessage<CloudFormationRequest<Application>>
                    {
                        Message = new CloudFormationRequest<Application>
                        {
                            RequestId = updateRequestId,
                            RequestType = CloudFormationRequestType.Update,
                            PhysicalResourceId = applicationId.ToString(),
                            ResponseURL = new Uri($"{app.RootUri}mock"),
                            ResourceProperties = new Application
                            {
                                Name = name,
                                Description = description,
                                Serial = ++serial,
                                Roles = new List<ApplicationRole>
                                {
                                    new ApplicationRole { Role = new Role { Name = nameof(BuiltInRole.Basic) } }
                                }
                            }
                        }
                    }, options);

                    response.EnsureSuccessStatusCode();
                }
                #endregion

                #region Ensure Successful Response Received
                {
                    var calls = app.Services.GetRequiredService<MockControllerCalls>();

                    calls.Should().Contain(response =>
                        response.RequestId == updateRequestId &&
                        response.Status == CloudFormationResponseStatus.SUCCESS
                    );
                    calls.Clear();
                }
                #endregion

                #region Ensure Application Serial and Encrypted Secret Changed
                {
                    using var scope = app.Services.CreateScope();
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    var query = from application in databaseContext.Applications.AsQueryable() where application.Id == applicationId select application;
                    var result = await query.FirstAsync();

                    result.EncryptedSecret.Should().NotBe(encryptedSecret);
                    result.Serial.Should().Be(serial);
                }
                #endregion

                #region Delete Application
                {
                    var response = await client.PostAsJsonAsync(EndpointUrl, new SnsMessage<CloudFormationRequest<Application>>
                    {
                        Message = new CloudFormationRequest<Application>
                        {
                            RequestId = deleteRequestId,
                            RequestType = CloudFormationRequestType.Delete,
                            PhysicalResourceId = applicationId.ToString(),
                            ResponseURL = new Uri($"{app.RootUri}mock"),
                            ResourceProperties = new Application
                            {
                                Name = name,
                                Description = description,
                                Serial = serial,
                                Roles = new List<ApplicationRole>
                                {
                                    new ApplicationRole { Role = new Role { Name = nameof(BuiltInRole.Basic) } }
                                }
                            }
                        }
                    }, options);

                    response.EnsureSuccessStatusCode();
                }
                #endregion

                #region Ensure Successful Response Received
                {
                    var calls = app.Services.GetRequiredService<MockControllerCalls>();

                    calls.Should().Contain(response =>
                        response.RequestId == deleteRequestId &&
                        response.Status == CloudFormationResponseStatus.SUCCESS
                    );
                    calls.Clear();
                }
                #endregion

                #region Ensure Application Does Not Exist in Database
                {
                    using var scope = app.Services.CreateScope();
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    var query = from application in databaseContext.Applications.AsQueryable() where application.Id == applicationId select application;
                    var exists = await query.AnyAsync();

                    exists.Should().Be(false);
                }
                #endregion
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
