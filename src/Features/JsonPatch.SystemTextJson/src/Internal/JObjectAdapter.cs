// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson.Helpers;

namespace Microsoft.AspNetCore.JsonPatch.Internal;

public class JObjectAdapter : IAdapter
{
    public virtual bool TryAdd(
        object target,
        string segment,
        IJsonTypeInfoResolver typeInfoResolver,
        object value,
        out string errorMessage)
    {
        var obj = (JsonObject)target;

        obj[segment] = value != null ? JsonSerializer.SerializeToNode(value) : new JsonObject();

        errorMessage = null;
        return true;
    }

    public virtual bool TryGet(
        object target,
        string segment,
        IJsonTypeInfoResolver typeInfoResolver,
        out object value,
        out string errorMessage)
    {
        var obj = (JsonObject)target;

        if (!obj.TryGetPropertyValue(segment, out var valueAsToken))
        {
            value = null;
            errorMessage = Resources.FormatTargetLocationAtPathSegmentNotFound(segment);
            return false;
        }

        value = valueAsToken;
        errorMessage = null;
        return true;
    }

    public virtual bool TryRemove(
        object target,
        string segment,
        IJsonTypeInfoResolver typeInfoResolver,
        out string errorMessage)
    {
        var obj = (JsonObject)target;

        if (!obj.Remove(segment))
        {
            errorMessage = Resources.FormatTargetLocationAtPathSegmentNotFound(segment);
            return false;
        }

        errorMessage = null;
        return true;
    }

    public virtual bool TryReplace(
        object target,
        string segment,
        IJsonTypeInfoResolver typeInfoResolver,
        object value,
        out string errorMessage)
    {
        var obj = (JsonObject)target;

        if (!obj.ContainsKey(segment))
        {
            errorMessage = Resources.FormatTargetLocationAtPathSegmentNotFound(segment);
            return false;
        }

        obj[segment] = value != null ? JsonSerializer.SerializeToNode(value) : new JsonObject();

        errorMessage = null;
        return true;
    }

    public virtual bool TryTest(
        object target,
        string segment,
        IJsonTypeInfoResolver typeInfoResolver,
        object value,
        out string errorMessage)
    {
        var obj = (JsonObject)target;

        if (!obj.TryGetPropertyValue(segment, out var currentValue))
        {
            errorMessage = Resources.FormatTargetLocationAtPathSegmentNotFound(segment);
            return false;
        }

        if (currentValue == null || string.IsNullOrEmpty(currentValue.ToString()))
        {
            errorMessage = Resources.FormatValueForTargetSegmentCannotBeNullOrEmpty(segment);
            return false;
        }

        if (!JsonHelper.DeepEquals(currentValue, value))
        {
            errorMessage = Resources.FormatValueNotEqualToTestValue(currentValue, value, segment);
            return false;
        }

        errorMessage = null;
        return true;
    }

    public virtual bool TryTraverse(
        object target,
        string segment,
        IJsonTypeInfoResolver typeInfoResolver,
        out object nextTarget,
        out string errorMessage)
    {
        var obj = (JsonObject)target;

        if (!obj.TryGetPropertyValue(segment, out var nextTargetToken))
        {
            nextTarget = null;
            errorMessage = null;
            return false;
        }

        nextTarget = nextTargetToken;
        errorMessage = null;
        return true;
    }
}
