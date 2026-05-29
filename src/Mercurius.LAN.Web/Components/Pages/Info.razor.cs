using Blazored.Toast.Services;
using Mercurius.LAN.Web.Options;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Mercurius.LAN.Web.Components.Pages;

public partial class Info
{
    private ContactMessage _contactModel = new();
    private bool _isSendingContact;

    [Inject] private IOptions<LanEventOptions> EventOptions { get; set; } = null!;
    [Inject] private IContactEmailService ContactEmailService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    private LanEventOptions Event => EventOptions.Value;
    private string GamerTicketPrice => Event.Tickets
        .FirstOrDefault(ticket => string.Equals(ticket.Kind, "gamer", StringComparison.OrdinalIgnoreCase))
        ?.Price ?? "10 EUR";

    private async Task SendContactAsync()
    {
        if(_isSendingContact)
            return;

        _isSendingContact = true;
        try
        {
            await ContactEmailService.SendAsync(_contactModel);
            ToastService.ShowSuccess("Your message has been sent.");
            _contactModel = new ContactMessage();
        }
        catch(InvalidOperationException exception)
        {
            ToastService.ShowError(exception.Message);
        }
        catch(Exception)
        {
            ToastService.ShowError("Your message could not be sent. Please try Discord, Facebook, or Instagram.");
        }
        finally
        {
            _isSendingContact = false;
        }
    }

    private static string FormatMenuPrice(decimal price) => price % 1 == 0
        ? $"{price.ToString("0", CultureInfo.InvariantCulture)} EUR"
        : $"{price.ToString("0.00", CultureInfo.InvariantCulture)} EUR";
}
