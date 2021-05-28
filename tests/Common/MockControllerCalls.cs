using System.Collections.Concurrent;

using Brighid.Identity.Sns;

#pragma warning disable CA1010

public class MockControllerCalls : ConcurrentBag<CloudFormationResponse>
{
    public MockControllerCalls()
        : base()
    {
    }
}
