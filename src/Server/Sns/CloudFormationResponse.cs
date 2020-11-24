namespace Brighid.Identity.Sns
{
    public record CloudFormationResponse
    {
        public CloudFormationResponse(ICloudFormationRequest<object> request, string? givenPhysicalResourceId = null)
        {
            StackId = request.StackId;
            LogicalResourceId = request.LogicalResourceId;
            RequestId = request.RequestId;
            PhysicalResourceId = givenPhysicalResourceId ?? request.PhysicalResourceId; ;
        }

        public CloudFormationResponseStatus Status { get; init; } = CloudFormationResponseStatus.SUCCESS;

        public string PhysicalResourceId { get; init; }

        public string StackId { get; init; }

        public string LogicalResourceId { get; init; }

        public string RequestId { get; init; }

        public string? Reason { get; init; }

        public object? Data { get; init; }

    }
}
