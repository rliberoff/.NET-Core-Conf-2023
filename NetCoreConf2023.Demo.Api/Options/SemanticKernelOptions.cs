using System.ComponentModel.DataAnnotations;

using NetCoreConf2023.Demo.Api.Infrastructure;

namespace NetCoreConf2023.Demo.Api.Options;

/// <summary>
/// Options to configure a Semantic Kernel.
/// </summary>
/// <seealso href="https://learn.microsoft.com/en-us/semantic-kernel/overview/"/>
public sealed class SemanticKernelOptions
{
    /// <summary>
    /// Gets the model deployment name on the LLM (for example OpenAI) to use for chat.
    /// </summary>
    /// <remarks>
    /// <b>WARNING:</b> The model deployment name does not necessarily have to be the same as the model name. For example, a model of type «GPT-4» might be called «MyGPT»;
    /// this means that the value of this property does not necessarily indicate the model implemented behind it.
    /// </remarks>
    [Required]
    public string ChatModelDeploymentName { get; init; }

    /// <summary>
    /// Gets the model deployment name on the LLM (for example OpenAI) to use for embeddings.
    /// </summary>
    /// <remarks>
    /// <b>WARNING:</b> The model name does not necessarily have to be the same as the model ID. For example, a model of type «text-embedding-ada-002» might be called «MyEmbeddings»;
    /// this means that the value of this property does not necessarily indicate the model implemented behind it.
    /// </remarks>
    [Required]
    public string EmbeddingsModelDeploymentName { get; init; }

    /// <summary>
    /// Gets the <see cref="Uri"/> for an LLM resource (like OpenAI). This should include protocol and host name.
    /// </summary>
    [Required]
    [Uri]
    public Uri Endpoint { get; init; }

    /// <summary>
    /// Gets the key credential used to authenticate to an LLM resource.
    /// </summary>
    [Required]
    public string Key { get; init; }
}
