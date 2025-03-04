// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi;

/// <summary>
/// Provides methods to create or retrieve OpenAPI schemas for specified types.
/// </summary>
public interface IOpenApiSchemaProvider
{
    /// <summary>
    /// Creates or retrieves an OpenAPI schema for the specified type.
    /// </summary>
    /// <param name="type">The <seealso cref="Type"/> type to create a schema for.</param>
    /// <param name="parameterDescription">Optional <see cref="ApiParameterDescription"/> providing additional metadata for the schema. This affects the schema generation for parameters.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created or retrieved OpenAPI schema.</returns>
    /// <remarks>
    /// The returned schema is not yet resolved for references. To use this schema in an OpenAPI document,
    /// add it to the document components using <c>document.AddComponent(name, schema)</c> and reference it
    /// using <c>new OpenApiSchemaReference(name, document)</c>.
    /// </remarks>
    Task<OpenApiSchema> GetOrCreateSchemaAsync(Type type, ApiParameterDescription? parameterDescription = null, CancellationToken cancellationToken = default);
}
