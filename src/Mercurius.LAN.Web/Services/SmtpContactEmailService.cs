using Mercurius.LAN.Web.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Mercurius.LAN.Web.Services;

public sealed class SmtpContactEmailService(
    IOptions<ContactEmailOptions> emailOptions,
    IOptions<LanEventOptions> eventOptions,
    ILogger<SmtpContactEmailService> logger) : IContactEmailService
{
    private readonly ContactEmailOptions _emailOptions = emailOptions.Value;
    private readonly LanEventOptions _eventOptions = eventOptions.Value;

    public async Task SendAsync(ContactMessage message, CancellationToken cancellationToken = default)
    {
        if(!_emailOptions.Enabled || string.IsNullOrWhiteSpace(_emailOptions.Host))
            throw new InvalidOperationException("Contact email is not configured.");

        var recipient = string.IsNullOrWhiteSpace(_emailOptions.RecipientEmail)
            ? _eventOptions.ContactEmail
            : _emailOptions.RecipientEmail;
        var sender = string.IsNullOrWhiteSpace(_emailOptions.SenderEmail)
            ? recipient
            : _emailOptions.SenderEmail;

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(sender, _emailOptions.SenderName),
            Subject = $"Mercurius LAN contact: {message.Name}",
            Body = $"""
                New message from the Mercurius LAN info page.

                Name:
                {message.Name}

                Contact:
                {message.Contact}

                Message:
                {message.Message}
                """,
            IsBodyHtml = false
        };

        mailMessage.To.Add(recipient);
        mailMessage.ReplyToList.Add(new MailAddress(GetReplyToAddress(message.Contact, sender), message.Name));

        using var client = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
        {
            EnableSsl = _emailOptions.EnableSsl
        };

        if(!string.IsNullOrWhiteSpace(_emailOptions.Username))
            client.Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password);

        try
        {
            await client.SendMailAsync(mailMessage, cancellationToken);
        }
        catch(Exception exception)
        {
            logger.LogError(exception, "Could not send LAN info contact email.");
            throw;
        }
    }

    private static string GetReplyToAddress(string contact, string fallback)
    {
        return MailAddress.TryCreate(contact, out var mailAddress)
            ? mailAddress.Address
            : fallback;
    }
}
