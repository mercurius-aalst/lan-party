namespace Mercurius.LAN.Web.DTOs.Users;

public record CurrentUserProfileResponse(bool HasProfile, UserProfileDTO? Profile);
