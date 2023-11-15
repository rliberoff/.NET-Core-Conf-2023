﻿using System.ComponentModel;

using MailKit.Net.Smtp;

using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

using MimeKit;

using NetCoreConf2023.Demo.Api.Options;

namespace NetCoreConf2023.Demo.Api.Plugins;

internal sealed class SendEmailPlugin
{
    private readonly SmtpClientOptions smtpClientOptions;

    public SendEmailPlugin(IOptions<SmtpClientOptions> smtpClientOptions)
    {
        this.smtpClientOptions = smtpClientOptions.Value;
    }

    [SKFunction]
    [Description("Given an e-mail address and message body, sends an email.")]
    public async Task<string> SendEmailAsync([Description("The body of the e-mail message to send.")] string body,
                                             [Description("The e-mail address to send the e-mail to.")] string address,
                                             CancellationToken cancellationToken)
    {
        using var mailMessage = new MimeMessage();
        mailMessage.Subject = @"An e-mail from Netcoreconf 2023 - Madrid!";
        mailMessage.Body = new BodyBuilder()
        {
            TextBody = body,
        }.ToMessageBody();
        mailMessage.To.Add(new MailboxAddress(address, address));

        using var smtpClient = new SmtpClient();

        if (smtpClientOptions.ServerCertificateValidationCallback != null)
        {
            smtpClient.ServerCertificateValidationCallback += smtpClientOptions.ServerCertificateValidationCallback;
        }

        await smtpClient.ConnectAsync(smtpClientOptions.Host, smtpClientOptions.Port, smtpClientOptions.UseSSL, cancellationToken);
        await smtpClient.AuthenticateAsync(smtpClientOptions.User, smtpClientOptions.Password, cancellationToken);
        var textResponseFromServer = await smtpClient.SendAsync(mailMessage, cancellationToken);
        await smtpClient.DisconnectAsync(true, cancellationToken);

        return textResponseFromServer;
    }
}
