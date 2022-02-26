using Amazon.CDK;

using Brighid.Identity.Artifacts;

#pragma warning disable SA1516

var app = new App();
_ = new ArtifactsStack(app, "identity-cicd", new StackProps
{
    Synthesizer = new BootstraplessSynthesizer(new BootstraplessSynthesizerProps()),
});

app.Synth();
