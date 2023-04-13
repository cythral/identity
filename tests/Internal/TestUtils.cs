using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Brighid.Identity;

using NSubstitute;

internal static class TestUtils
{
    internal static void SetReadonlyProperty<T>(this T target, string key, object value)
    {
        var field = typeof(T).GetField($"<{key}>k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy) ?? throw new Exception();

        field.SetValue(target, value);
    }

    internal static IEnumerable<IExceptionMapping> GetExceptionMappings<T>(this T target, string method)
    {
        var type = target!.GetType();
        var decl = type.GetMethod(method)!;
        return from attr in decl.GetCustomAttributes() where attr is IExceptionMapping select (IExceptionMapping)attr;
    }

    internal static TArg GetArg<TArg>(object target, string methodName, int arg)
    {
        return (from call in target.ReceivedCalls()
                let methodInfo = call.GetMethodInfo()
                where methodInfo.Name == methodName
                select (TArg)call.GetArguments()[arg]).First();
    }
}
