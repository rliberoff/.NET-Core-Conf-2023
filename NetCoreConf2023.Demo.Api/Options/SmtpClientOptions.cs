using System.ComponentModel.DataAnnotations;
using System.Net.Security;

namespace NetCoreConf2023.Demo.Api.Options;

public class SmtpClientOptions
{
    /// <summary>
    /// Gets the host name of the SMTP service.
    /// </summary>
    [Required]
    public string Host { get; init; }

    /// <summary>
    /// Gets the password credential required to connect to the SMTP service.
    /// </summary>
    [Required]
    public string Password { get; init; }

    /// <summary>
    /// Gets the port for the SMTP service. Default value is <c>587</c>.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    /// <summary>
    /// Gets the user credential required to connect to the SMTP service.
    /// </summary>
    [Required]
    public string SenderAddress { get; init; }

    /// <summary>
    /// Gets the user credential required to connect to the SMTP service.
    /// </summary>
    [Required]
    public string User { get; init; }

    /// <summary>
    /// Gets a value indicating whether SSL should be use. Default is <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// When setting this property as <see langword="true"/>, check the value of <seealso cref="Port"/>.
    /// </remarks>
    public bool UseSSL { get; init; } = false;
}
