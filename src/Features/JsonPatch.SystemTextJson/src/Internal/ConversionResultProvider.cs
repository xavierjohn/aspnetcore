// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.AspNetCore.JsonPatch.SystemTextJson.Internal;

/// <summary>
/// This API supports infrastructure and is not intended to be used
/// directly from your code. This API may change or be removed in future releases.
/// </summary>
public static class ConversionResultProvider
{
    public static ConversionResult ConvertTo(object value, Type typeToConvertTo)
    {
        return ConvertTo(value, typeToConvertTo, null);
    }

    internal static ConversionResult ConvertTo(object value, Type typeToConvertTo, IJsonTypeInfoResolver typeInfoResolver)
    {
        if (value == null)
        {
            return new ConversionResult(IsNullableType(typeToConvertTo), null);
        }
        else if (typeToConvertTo.IsAssignableFrom(value.GetType()))
        {
            // No need to convert
            return new ConversionResult(true, value);
        }
        else
        {
            try
            {
                if (typeInfoResolver == null)
                {
                    var deserialized = JsonSerializer.Deserialize(JsonSerializer.SerializeToDocument(value), typeToConvertTo);
                    return new ConversionResult(true, deserialized);
                }
                else
                {
                    var serializerSettings = new JsonSerializerOptions { TypeInfoResolver = typeInfoResolver };
                    var deserialized = JsonSerializer.Deserialize(JsonSerializer.SerializeToDocument(value), typeToConvertTo, serializerSettings);
                    return new ConversionResult(true, deserialized);
                }
            }
            catch
            {
                return new ConversionResult(canBeConverted: false, convertedInstance: null);
            }
        }
    }

    public static ConversionResult CopyTo(object value, Type typeToConvertTo)
    {
        var targetType = typeToConvertTo;
        if (value == null)
        {
            return new ConversionResult(canBeConverted: true, convertedInstance: null);
        }
        else if (typeToConvertTo.IsAssignableFrom(value.GetType()))
        {
            // Keep original type
            targetType = value.GetType();
        }
        try
        {
            var deserialized = JsonSerializer.Deserialize(JsonSerializer.SerializeToDocument(value), targetType);
            return new ConversionResult(true, deserialized);
        }
        catch
        {
            return new ConversionResult(canBeConverted: false, convertedInstance: null);
        }
    }

    private static bool IsNullableType(Type type)
    {
        if (type.IsValueType)
        {
            // value types are only nullable if they are Nullable<T>
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        else
        {
            // reference types are always nullable
            return true;
        }
    }
}
