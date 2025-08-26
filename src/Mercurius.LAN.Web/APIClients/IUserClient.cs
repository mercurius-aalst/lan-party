using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Auth;
using Refit;

namespace Mercurius.LAN.Web.APIClients
{
    public interface IUserClient
    {
        [Get("/users")]
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();

        [Get("/users/{id}")]
        Task<UserDTO> GetUserByIdAsync(int id);
        [Delete("/users/{username}")]
        Task DeleteUserAsync(string username);

        [Post("/users/{username}/roles")]
        Task AddRoleToUserAsync(string username, [Body] AddUserRoleRequest request);
        [Delete("/users/{username}/roles/{role}")]
        Task DeleteRoleFromUserAsync(string username, string role);

        [Patch("/users/{username}/password")]
        Task ChangePasswordAsync(string username, [Body] ChangePasswordRequest request);

    }
}
