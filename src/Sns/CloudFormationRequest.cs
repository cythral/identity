using System;

namespace Brighid.Identity.Sns
{
    public class CloudFormationRequest<T> where T : notnull, new()
    {

        public CloudFormationRequestType RequestType { get; init; }

        public Uri ResponseURL { get; init; }

        public string StackId { get; init; }

        public string RequestId { get; init; }

        public string ResourceType { get; init; }

        public string LogicalResourceId { get; init; }

        public string PhysicalResourceId { get; init; }

        public T ResourceProperties { get; init; } = new T();

        public T OldResourceProperties { get; init; } = new T();
    }
}
