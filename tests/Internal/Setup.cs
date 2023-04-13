using System;

using NUnit.Framework;

[SetUpFixture]
public class Setup
{
    [OneTimeSetUp]
    public void SetUpEnvironment()
    {
        Environment.SetEnvironmentVariable("AWS_XRAY_CONTEXT_MISSING", "LOG_ERROR");
    }
}
