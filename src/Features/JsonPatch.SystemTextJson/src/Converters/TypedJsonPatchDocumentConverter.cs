// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson.Operations;

namespace Microsoft.AspNetCore.JsonPatch.SystemTextJson.Converters;

public class TypedJsonPatchDocumentConverter : JsonPatchDocumentConverter
{

    public override JsonPatchDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            var genericType = typeToConvert.GenericTypeArguments;

            // Load JsonArray
            var jsonArray = JsonArray.Parse(ref reader);

            // Create target object for Json => list of operations, typed to genericType
            var genericOperation = typeof(Operation<>);
            var concreteOperationType = genericOperation.MakeGenericType(genericType);

            var genericList = typeof(List<>);
            var concreteList = genericList.MakeGenericType(concreteOperationType);

            var targetOperations = Activator.CreateInstance(concreteList);

            // Create a new reader for this jsonArray, and set all properties to match the original reader.
            var jsonArrayString = jsonArray.ToJsonString();
            var jsonArrayBytes = System.Text.Encoding.UTF8.GetBytes(jsonArrayString);
            var jsonArraySequence = new ReadOnlySequence<byte>(jsonArrayBytes);
            var jsonArrayReader = new Utf8JsonReader(jsonArraySequence);

            // Populate the object properties
            targetOperations = JsonSerializer.Deserialize(ref jsonArrayReader, concreteList, options);

            // Container target: the typed JsonPatchDocument.
            var container = (JsonPatchDocument)Activator.CreateInstance(typeToConvert, targetOperations, JsonPatchDocumentConverter.DefaultContractResolver);

            return container;
        }
        catch (Exception ex)
        {
            throw new JsonException("Invalid JsonPatchDocument", ex);
        }

    }
}
