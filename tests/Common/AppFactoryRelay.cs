using System.Reflection;

using AutoFixture.Kernel;

#pragma warning disable CA2000

public class AppFactoryRelay : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        var parameterRequest = request as ParameterInfo;

        return (parameterRequest?.ParameterType == typeof(AppFactory))
            ? AppFactory.Create().GetAwaiter().GetResult()
            : new NoSpecimen();
    }
}
