namespace Brighid.Identity.Sns
{
    public class CloudFormationResponse
    {
        public CloudFormationResponse(ICloudFormationRequest<object> request, string? givenPhysicalResourceId = null)
        {
            StackId = request.StackId;
            LogicalResourceId = request.LogicalResourceId;
            RequestId = request.RequestId;
            PhysicalResourceId = givenPhysicalResourceId ?? request.PhysicalResourceId;
        }

        public CloudFormationResponse()
        {
        }

        public CloudFormationResponseStatus Status { get; set; } = CloudFormationResponseStatus.SUCCESS;

        public string PhysicalResourceId { get; set; }

        public string StackId { get; set; }

        public string LogicalResourceId { get; set; }

        public string RequestId { get; set; }

        public string? Reason { get; set; }

        public object? Data { get; set; }
    }
}
