namespace Mercurius.LAN.Web.Models.Participants;

public sealed record TeamPage(
    IReadOnlyList<Team> Teams,
    int Page,
    int PageSize,
    bool HasMore)
{
    public const int MaximumPageSize = 50;
    public const int DefaultPageSize = MaximumPageSize;
}
