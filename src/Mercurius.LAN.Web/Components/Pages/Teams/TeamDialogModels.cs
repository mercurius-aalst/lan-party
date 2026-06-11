namespace Mercurius.LAN.Web.Components.Pages.Teams;

public sealed record TeamLogoSelection(string FileName, string ContentType, byte[] Content, string PreviewDataUrl);

public sealed record CreateTeamDialogResult(string Name, TeamLogoSelection? Logo);

public sealed record InviteUserDialogResult(Guid UserId, string DisplayName);
