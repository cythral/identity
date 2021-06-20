using System;
using System.Linq;
using System.Reflection;

using NSubstitute;

internal static class TestUtils
{
    internal static void SetReadonlyProperty<T>(this T target, string key, object value)
    {
        var field = typeof(T).GetField($"<{key}>k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (field == null)
        {
            throw new Exception();
        }

        field.SetValue(target, value);
    }

    internal static TArg GetArg<TArg>(object target, string methodName, int arg)
    {
        return (from call in target.ReceivedCalls()
                let methodInfo = call.GetMethodInfo()
                where methodInfo.Name == methodName
                select (TArg)call.GetArguments()[arg]).First();
    }
}
