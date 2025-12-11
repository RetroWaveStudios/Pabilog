using System;
using System.Reflection;

public static class SaveDataMerger
{
    public static T MergeWithDefaults<T>(T oldData, T defaultData)
    {
        if (oldData == null) return defaultData;

        Type type = typeof(T);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            object oldValue = field.GetValue(oldData);
            object defaultValue = field.GetValue(defaultData);

            // ✅ If field is a nested class or complex type, merge it recursively
            if (field.Name != "Coin" && field.Name != "Crystal" && field.Name != "XP")
            {
                if (!field.FieldType.IsPrimitive &&
                    !field.FieldType.IsEnum &&
                    field.FieldType != typeof(string))
                {
                    if (oldValue == null && defaultValue != null)
                    {
                        field.SetValue(oldData, defaultValue);
                    }
                    else if (oldValue != null && defaultValue != null)
                    {
                        // Recursive merge
                        MethodInfo method = typeof(SaveDataMerger)
                            .GetMethod(nameof(MergeWithDefaults))
                            .MakeGenericMethod(field.FieldType);

                        object mergedValue = method.Invoke(null, new object[] { oldValue, defaultValue });
                        field.SetValue(oldData, mergedValue);
                    }

                    continue;
                }

                // ✅ If field is null or default (for value types), set default value
                if (oldValue == null ||
                    (field.FieldType.IsValueType && Activator.CreateInstance(field.FieldType).Equals(oldValue)))
                {
                    field.SetValue(oldData, defaultValue);
                }
            }
        }

        return oldData;
    }
}
