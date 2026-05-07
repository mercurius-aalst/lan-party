using Mercurius.LAN.Web.DTOs.Users;
using Refit;

namespace Mercurius.LAN.Web.APIClients
{
    public interface IUserClient
    {
        [Get("/lan/users/me")]
        Task<CurrentUserProfileResponse> GetCurrentUserProfileAsync();

        [Post("/lan/users/me/complete-profile")]
        Task<UserProfileDTO> CompleteCurrentUserProfileAsync([Body] CompleteUserProfileRequest request);

        [Get("/lan/users")]
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();

        [Get("/lan/users/{id}")]
        Task<UserDTO> GetUserByIdAsync(int id);

        [Delete("/lan/users/{username}")]
        Task DeleteUserAsync(string username);
    }
}
