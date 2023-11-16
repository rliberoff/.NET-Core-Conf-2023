using System.ComponentModel.DataAnnotations;

namespace NetCoreConf2023.Demo.Api.Options;

public sealed class BingOptions
{
    [Required]
    public string Key { get; init; }
}
