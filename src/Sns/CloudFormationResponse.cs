namespace Brighid.Identity.Sns
{
    public record CloudFormationResponse(ICloudFormationRequest<object> request, string? physicalResourceId = null)
    {

        public CloudFormationResponseStatus Status { get; init; } = CloudFormationResponseStatus.SUCCESS;

        public string PhysicalResourceId { get; init; } = physicalResourceId ?? request.PhysicalResourceId;

        public string StackId { get; init; } = request.StackId;

        public string LogicalResourceId { get; init; } = request.LogicalResourceId;

        public string RequestId { get; init; } = request.RequestId;

        public string? Reason { get; init; }

        public object? Data { get; init; }

    }
}
