using System;
using System.Collections.Generic;
using System.Text;

public static class TypeExtensions
{
    public static string GetFriendlyName(this Type type)
    {
        string friendlyName = type.FullName;
        if (type.IsGenericType)
        {
            friendlyName = GetTypeString(type);
        }
        return friendlyName;
    }

    private static string GetTypeString(Type type)
    {
        var t = type.AssemblyQualifiedName;

        var output = new StringBuilder();
        List<string> typeStrings = new List<string>();  

        int iAssyBackTick = t.IndexOf('`') + 1;
        output.Append(t.Substring(0, iAssyBackTick - 1).Replace("[", string.Empty));
        var genericTypes = type.GetGenericArguments();

        foreach (var genType in genericTypes)
        {
            typeStrings.Add(genType.IsGenericType ? GetTypeString(genType) : genType.ToString());
        }

        output.Append($"<{string.Join(",", typeStrings)}>");
        return output.ToString();
    }
}