namespace NetCoreConf2023.Demo.Api.Options;

/// <summary>
/// Configuration options for the version support OpenAPI.
/// </summary>
public sealed class VersionSwaggerGenOptions
{
    /// <summary>
    /// Gets or sets the title for the OpenAPI document when using version support.
    /// </summary>
    public string Title { get; set; } = @"REST API";
}
