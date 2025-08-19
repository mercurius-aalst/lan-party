
namespace Mercurius.LAN.Web.Services
{
    public interface IUserService
    {
        Task ChangePasswordAsync(string username, string currentPassword, string newPassword);
    }
}