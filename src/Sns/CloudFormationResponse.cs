namespace Brighid.Identity.Sns
{
    public class CloudFormationResponse
    {

        public CloudFormationResponseStatus Status { get; init; } = CloudFormationResponseStatus.SUCCESS;

        public string PhysicalResourceId { get; init; }

        public string StackId { get; init; }

        public string LogicalResourceId { get; init; }

        public string RequestId { get; init; }

        public string Reason { get; init; }

        public object Data { get; init; }

    }
}
