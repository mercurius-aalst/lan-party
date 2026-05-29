namespace Mercurius.LAN.Web.Services;

public interface IContactEmailService
{
    Task SendAsync(ContactMessage message, CancellationToken cancellationToken = default);
}
