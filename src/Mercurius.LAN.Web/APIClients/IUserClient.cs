using Mercurius.LAN.Web.DTOs.Users;
using Refit;

namespace Mercurius.LAN.Web.APIClients;

/// <summary>
/// Refit contract for the version 1 identity API. Paths are relative to the configured
/// <c>.../v1/</c> base address so the version prefix is retained by URI resolution.
/// </summary>
public interface IUserClient
{
    [Get("lan/users/me")]
    Task<CurrentUserProfileResponse> GetCurrentUserProfileAsync();

    [Put("lan/users/me")]
    Task<UserProfileDTO> CompleteCurrentUserProfileAsync([Body] CompleteUserProfileRequest request);

    [Patch("lan/users/me")]
    Task<UserProfileDTO> UpdateCurrentUserProfileAsync([Body] UpdateUserProfileRequest request);

    [Get("lan/users/me/username-availability")]
    Task<UsernameAvailabilityResponse> CheckUsernameAvailabilityAsync([AliasAs("username")] string username);

    [Post("lan/users/me/resend-verification-email")]
    Task<UserActionResponse> ResendVerificationEmailAsync();

    [Post("lan/users/me/password-reset")]
    Task<UserActionResponse> SendPasswordResetEmailAsync();

    [Delete("lan/users/me")]
    Task<UserActionResponse> DeleteCurrentUserAsync();

    [Post("lan/users")]
    Task<UserDTO> CreateUserAsync([Body] CreateUserProfileRequest request);

    [Get("lan/users")]
    Task<IEnumerable<UserDTO>> GetAllUsersAsync();

    [Get("lan/users/{id}")]
    Task<UserDTO> GetUserByIdAsync(Guid id);

    [Patch("lan/users/{id}")]
    Task<UserDTO> UpdateUserAsync(Guid id, [Body] UpdateUserProfileRequest request);

    [Delete("lan/users/{id}")]
    Task DeleteUserAsync(Guid id);

    [Delete("lan/users/{username}")]
    Task DeleteUserAsync(string username);

    [Delete("lan/users/{username}/account")]
    Task DeleteUserAccountAsync(string username);
}
