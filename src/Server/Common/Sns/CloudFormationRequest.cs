using System;
using System.Text.Json.Serialization;

namespace Brighid.Identity.Sns
{
    public class CloudFormationRequest<T> : ICloudFormationRequest<T>
    {
        [JsonConverter(typeof(CloudFormationRequestTypeConverter))]
        public CloudFormationRequestType RequestType { get; init; }

        public Uri ResponseURL { get; init; }

        public string StackId { get; init; }

        public string RequestId { get; init; }

        public string ResourceType { get; init; }

        public string LogicalResourceId { get; init; }

        public string PhysicalResourceId { get; init; }

        public T? ResourceProperties { get; init; }

        public T? OldResourceProperties { get; init; }
    }
}
