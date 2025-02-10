// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Xunit;

namespace Microsoft.AspNetCore.JsonPatch.SystemTextJson.IntegrationTests;

public class HeterogenousCollectionTests
{
    [Fact]
    public void AddItemToList()
    {
        // Arrange
        var targetObject = new Canvas()
        {
            Items = new List<Shape>()
        };

        var circleJObject = JsonObject.Parse(@"{
              Type: 'Circle',
              ShapeProperty: 'Shape property',
              CircleProperty: 'Circle property'
            }");

        var patchDocument = new JsonPatchDocument
        {
            TypeInfoResolver = new CanvasContractResolver()
        };

        patchDocument.Add("/Items/-", circleJObject);

        // Act
        patchDocument.ApplyTo(targetObject);

        // Assert
        var circle = targetObject.Items[0] as Circle;
        Assert.NotNull(circle);
        Assert.Equal("Shape property", circle.ShapeProperty);
        Assert.Equal("Circle property", circle.CircleProperty);
    }
}

public class CanvasContractResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        if (type == typeof(Shape))
        {
            return new ShapeJsonConverter();
        }

        return base.GetTypeInfo(type, options);
    }
}

public class ShapeJsonConverter : JsonConverter<Shape>
{
    private const string TypeProperty = "Type";

    //public override bool CanRead => true;

    //public override Shape Create(Type objectType)
    //{
    //    throw new NotImplementedException();
    //}

    //public override object ReadJson(
    //    JsonReader reader,
    //    Type objectType,
    //    object existingValue,
    //    JsonSerializer serializer)
    //{
    //    var jObject = JsonObject.Load(reader);

    //    var target = CreateShape(jObject);
    //    serializer.Populate(jObject.CreateReader(), target);

    //    return target;
    //}

    private Shape CreateShape(JsonNode jsonNode)
    {
        var typeProperty = jsonNode[TypeProperty].AsValue().ToString();
        return typeProperty switch
        {
            "Circle" => new Circle(),
            "Rectangle" => new Rectangle(),
            _ => throw new NotSupportedException(),
        };
    }
    public override Shape Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jObject = JsonObject.Parse(reader: ref reader, new JsonNodeOptions { PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive });
        var target = CreateShape(jObject);
        options.GetConverter(target.GetType()).Convert(ref reader, target.GetType(), options);
        return target;
    }

    public override void Write(Utf8JsonWriter writer, Shape value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
