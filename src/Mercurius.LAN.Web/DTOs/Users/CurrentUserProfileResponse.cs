namespace Mercurius.LAN.Web.DTOs.Users;

public record CurrentUserProfileResponse(bool IsComplete, UserProfileDTO? User, string? Email, bool EmailVerified);
