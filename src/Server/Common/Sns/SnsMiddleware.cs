using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Flurl.Http;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using static Brighid.Identity.Sns.CloudFormationRequestType;

namespace Brighid.Identity.Sns
{
    public class SnsMiddleware
    {
        private readonly RequestDelegate next;
        private readonly JsonSerializerOptions jsonOptions;

        public SnsMiddleware(RequestDelegate next)
        {
            this.next = next;
            this.jsonOptions = new JsonSerializerOptions();
            this.jsonOptions.Converters.Add(new CloudFormationRequestTypeConverter());
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.Headers.TryGetValue("x-amz-sns-message-type", out var snsMessageType);
            if (snsMessageType == StringValues.Empty)
            {
                await next(context);
                return;
            }

            context.Items[Constants.RequestSource] = IdentityRequestSource.Sns;
            await (snsMessageType[0] switch
            {
                "SubscriptionConfirmation" => HandleSubscriptionConfirmation(context),
                "Notification" => HandleNotification(context),
                _ => throw new Exception("Message type not supported."),
            });
        }

        private async Task HandleSubscriptionConfirmation(HttpContext context)
        {
            var bodyStream = context.Request.Body;
            var request = await JsonSerializer.DeserializeAsync<SnsMessage<object>>(bodyStream, jsonOptions);

            if (request == null)
            {
                throw new Exception("SNS Message unexpectedly deserialized to null.");
            }

            await request.SubscribeUrl.GetAsync();
        }

        private async Task HandleNotification(HttpContext context)
        {
            var message = await ReadBody(context);
            var resourceProperties = message.ResourceProperties ?? new { };
            await WriteBody(context, resourceProperties);
            UpdateRequestProperties(context, message);

            try
            {
                await next(context);

                context.Items.TryGetValue(CloudFormationConstants.Id, out var physicalResourceId);
                context.Items.TryGetValue(CloudFormationConstants.Data, out var model);

                await message.ResponseURL.PutJsonAsync(new CloudFormationResponse(message, physicalResourceId?.ToString())
                {
                    Status = CloudFormationResponseStatus.SUCCESS,
                    Data = model,
                });
#pragma warning disable CA1031
            }
            catch (Exception e)
            {
#pragma warning restore CA1031
                await message.ResponseURL.PutJsonAsync(new CloudFormationResponse(message, null)
                {
                    Status = CloudFormationResponseStatus.FAILED,
                    Reason = e.Message,
                });
            }
        }

        private async Task<CloudFormationRequest<object>> ReadBody(HttpContext context)
        {
            context.Request.EnableBuffering();

            var bodyStream = context.Request.Body;
            var request = await JsonSerializer.DeserializeAsync<SnsMessage<CloudFormationRequest<object>>>(bodyStream, jsonOptions);
            var message = request?.Message;

            return message ?? throw new Exception("Invalid SNS Message");
        }

        private async Task WriteBody(HttpContext context, object body)
        {
            var originalBodyStream = context.Request.Body;
            var stream = new MemoryStream();

            await JsonSerializer.SerializeAsync(stream, body, jsonOptions);
            await originalBodyStream.DisposeAsync();

            context.Request.Body = stream;
            context.Request.Body.Position = 0;
            context.Request.EnableBuffering();
        }

        private void UpdateRequestProperties(HttpContext context, CloudFormationRequest<object> request)
        {
            context.Response.StatusCode = 200;
            context.Request.Method = request.RequestType switch
            {
                Create => HttpMethod.Post.ToString(),
                Update => HttpMethod.Put.ToString(),
                Delete => HttpMethod.Delete.ToString(),
                _ => throw new Exception("Not supported"),
            };

            if (request.RequestType != Create && request.PhysicalResourceId != null)
            {
                context.Request.Path += "/" + request.PhysicalResourceId;
                context.Request.RouteValues["id"] = new Guid(request.PhysicalResourceId);
            }
        }
    }
}


