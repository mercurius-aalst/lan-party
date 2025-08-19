using Mercurius.LAN.Web.Models.Auth;
using Refit;

namespace Mercurius.LAN.Web.APIClients
{
    public interface IUserClient
    {
        [Delete("/users/{username}")]
        Task DeleteUser(string username);

        [Post("/users/{username}/roles")]
        Task AddRoleToUser(string username, [Body] AddUserRoleRequest request);

        [Patch("/users/{username}/password")]
        Task ChangePasswordAsync(string username, [Body] ChangePasswordRequest request);
    }
}
