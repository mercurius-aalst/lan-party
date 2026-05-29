using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.Options;

public sealed class ContactEmailOptions
{
    public const string SectionName = "ContactEmail";

    public bool Enabled { get; set; }

    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = "Mercurius LAN website";

    public string RecipientEmail { get; set; } = string.Empty;
}
