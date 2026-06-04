using Mercurius.LAN.Web.DTOs.Users;
using Refit;

namespace Mercurius.LAN.Web.APIClients
{
    public interface IUserClient
    {
        [Get("/v1/lan/users/me")]
        Task<CurrentUserProfileResponse> GetCurrentUserProfileAsync();

        [Post("/v1/lan/users/me/complete-profile")]
        Task<UserProfileDTO> CompleteCurrentUserProfileAsync([Body] CompleteUserProfileRequest request);

        [Patch("/v1/lan/users/me")]
        Task<UserProfileDTO> UpdateCurrentUserProfileAsync([Body] UpdateUserProfileRequest request);

        [Get("/v1/lan/users/me/username-availability")]
        Task<UsernameAvailabilityResponse> CheckUsernameAvailabilityAsync([AliasAs("username")] string username);

        [Post("/v1/lan/users/me/resend-verification-email")]
        Task<UserActionResponse> ResendVerificationEmailAsync();

        [Post("/v1/lan/users/me/password-reset")]
        Task<UserActionResponse> SendPasswordResetEmailAsync();

        [Delete("/v1/lan/users/me")]
        Task<UserActionResponse> DeleteCurrentUserAsync();

        [Post("/v1/lan/users")]
        Task<UserDTO> CreateUserAsync([Body] CreateUserProfileRequest request);

        [Get("/v1/lan/users")]
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();

        [Get("/v1/lan/users/{id}")]
        Task<UserDTO> GetUserByIdAsync(Guid id);

        [Patch("/v1/lan/users/{id}")]
        Task<UserDTO> UpdateUserAsync(Guid id, [Body] UpdateUserProfileRequest request);

        [Delete("/v1/lan/users/{id}")]
        Task DeleteUserAsync(Guid id);

        [Delete("/v1/lan/users/{username}")]
        Task DeleteUserAsync(string username);

        [Delete("/v1/lan/users/{username}/account")]
        Task DeleteUserAccountAsync(string username);
    }
}
