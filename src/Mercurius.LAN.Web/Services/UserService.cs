using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Models.Auth;

namespace Mercurius.LAN.Web.Services
{
    public class UserService : IUserService
    {
        private readonly IUserClient _userClient;

        public UserService(IUserClient userClient)
        {
            _userClient = userClient;
        }

        public async Task ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            var request = new ChangePasswordRequest
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            await _userClient.ChangePasswordAsync(username, request);
        }
    }
}