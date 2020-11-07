using System;
using System.Reflection;

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
}
