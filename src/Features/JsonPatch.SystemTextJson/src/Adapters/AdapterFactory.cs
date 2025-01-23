// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Shared;

namespace Microsoft.AspNetCore.JsonPatch.Adapters;

/// <summary>
/// The default AdapterFactory to be used for resolving <see cref="IAdapter"/>.
/// </summary>
public class AdapterFactory : IAdapterFactory
{
    internal static AdapterFactory Default { get; } = new();

    /// <inheritdoc />
#pragma warning disable PUB0001
    public virtual IAdapter Create(object target, IJsonTypeInfoResolver typeInfoResolver)
#pragma warning restore PUB0001
    {
        ArgumentNullThrowHelper.ThrowIfNull(target);
        ArgumentNullThrowHelper.ThrowIfNull(typeInfoResolver);

        var jsonContract = typeInfoResolver.GetTypeInfo(target.GetType(), JsonSerializerOptions.Default);

        return jsonContract.Kind switch
        {
            JsonTypeInfoKind.Object => new JObjectAdapter(),
            JsonTypeInfoKind.Enumerable => new ListAdapter(),
            JsonTypeInfoKind.Dictionary => (IAdapter)Activator.CreateInstance(typeof(DictionaryAdapter<,>).MakeGenericType(jsonContract.KeyType, jsonContract.ElementType)),
            JsonTypeInfoKind.None => new PocoAdapter(),
            _ => new PocoAdapter()
        };
    }
}

