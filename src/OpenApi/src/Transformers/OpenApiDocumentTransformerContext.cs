// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi;

/// <summary>
/// Represents the context in which an OpenAPI document transformer is executed.
/// </summary>
public sealed class OpenApiDocumentTransformerContext : IOpenApiSchemaProvider
{
    /// <summary>
    /// Gets the name of the associated OpenAPI document.
    /// </summary>
    public required string DocumentName { get; init; }

    /// <summary>
    /// Gets the API description groups associated with current document.
    /// </summary>
    public required IReadOnlyList<ApiDescriptionGroup> DescriptionGroups { get; init; }

    /// <summary>
    /// Gets the application services associated with current document.
    /// </summary>
    public required IServiceProvider ApplicationServices { get; init; }

    internal IOpenApiSchemaTransformer[] SchemaTransformers { get; init; } = [];

    internal OpenApiDocument? Document { get; init; }

    /// <inheritdoc />
    public Task<OpenApiSchema> GetOrCreateSchemaAsync(Type type, ApiParameterDescription? parameterDescription = null, CancellationToken cancellationToken = default)
    {
        Debug.Assert(Document is not null, "Document should have been initialized by framework.");
        var schemaService = ApplicationServices.GetRequiredKeyedService<OpenApiSchemaService>(DocumentName);
        return schemaService.GetOrCreateUnresolvedSchemaAsync(
            document: Document,
            type: type,
            parameterDescription: parameterDescription,
            scopedServiceProvider: ApplicationServices,
            schemaTransformers: SchemaTransformers,
            cancellationToken: cancellationToken);
    }
}
