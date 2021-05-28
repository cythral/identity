using System;

namespace Brighid.Identity.Sns
{
    public interface ICloudFormationRequest<out T>
    {
        CloudFormationRequestType RequestType { get; }

        Uri ResponseURL { get; }

        string StackId { get; }

        string RequestId { get; }

        string ResourceType { get; }

        string LogicalResourceId { get; }

        string PhysicalResourceId { get; }

        T? ResourceProperties { get; }

        T? OldResourceProperties { get; }
    }
}
