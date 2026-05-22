using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.Services;

public sealed class ContactMessage
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string Contact { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;
}
