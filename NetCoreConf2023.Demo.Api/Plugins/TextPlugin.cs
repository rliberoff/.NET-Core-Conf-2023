using System.ComponentModel;

using Microsoft.SemanticKernel;

namespace NetCoreConf2023.Demo.Api.Plugins;

public class TextPlugin
{
    /// <summary>
    /// Concatenates two strings into one.
    /// </summary>
    /// <param name="first">First input to concatenate with</param>
    /// <param name="second">Second input to concatenate with</param>
    /// <returns>Concatenation result from both inputs.</returns>
    [SKFunction]
    [Description("Concatenates two strings into one.")]
    public string Concat([Description("First input to concatenate with")] string first, [Description("Second input to concatenate with")] string second) => string.Concat(first, second);
}
