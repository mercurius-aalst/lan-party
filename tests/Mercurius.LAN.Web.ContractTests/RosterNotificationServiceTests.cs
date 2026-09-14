using System.Reflection;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.Registrations;
using Mercurius.LAN.Web.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class RosterNotificationServiceTests
{
    [Fact]
    public void RegistrationSurfacePrioritizesSelectedMemberDecisionOverCaptainWizard()
    {
        var markup = ReadRepositoryFile("src/Mercurius.LAN.Web/Components/Pages/Tournaments/Tabs/TournamentParticipantsTab.razor");
        var invitationIndex = markup.IndexOf("else if(_registrationState?.PendingRosterConfirmation is not null)", StringComparison.Ordinal);
        var wizardIndex = markup.IndexOf("else if(IsRegistrationOpen)", invitationIndex, StringComparison.Ordinal);

        Assert.True(invitationIndex >= 0);
        Assert.True(wizardIndex > invitationIndex);
        Assert.Contains("DeclineRosterMemberAsync", markup, StringComparison.Ordinal);
        Assert.Contains("Feature.tournaments.acceptRosterPlace", markup, StringComparison.Ordinal);
        Assert.Contains("Feature.tournaments.declineRosterPlace", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void InlineUnregistrationConfirmationMovesFocusIntoAndBackOutOfThePrompt()
    {
        var code = ReadRepositoryFile("src/Mercurius.LAN.Web/Components/Pages/Tournaments/Tabs/TournamentParticipantsTab.razor.cs");

        Assert.Contains("_focusTeamUnregistrationConfirmation = true", code, StringComparison.Ordinal);
        Assert.Contains("await _teamUnregistrationKeepButtonElement.FocusAsync()", code, StringComparison.Ordinal);
        Assert.Contains("await _teamUnregistrationTriggerElement.FocusAsync()", code, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RefreshMergesTeamInvitesAndRosterSelectionsWithDistinctStableIds()
    {
        var inviteId = Guid.NewGuid();
        var rosterMemberId = Guid.NewGuid();
        var teamService = CreateProxy<ITeamService>((method, _) => method.Name switch
        {
            nameof(ITeamService.GetCurrentUserTeamSummaryAsync) => Task.FromResult(new CurrentUserTeamSummaryDTO
            {
                ReceivedPendingInvites = [new TeamInviteSummaryDTO
                {
                    Id = inviteId,
                    TeamId = Guid.NewGuid(),
                    TeamName = "Inviters",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-2)
                }]
            }),
            _ => throw new NotSupportedException(method.Name)
        });
        var tournamentService = CreateProxy<ITournamentService>((method, _) => method.Name switch
        {
            nameof(ITournamentService.GetPendingRosterConfirmationsAsync) => Task.FromResult<IReadOnlyList<PendingRosterConfirmationDTO>>
            ([new PendingRosterConfirmationDTO
            {
                RosterMemberId = rosterMemberId,
                TournamentId = Guid.NewGuid(),
                TournamentName = "Finals",
                TeamId = Guid.NewGuid(),
                TeamName = "Selected",
                SelectedAtUtc = DateTime.UtcNow
            }]),
            _ => throw new NotSupportedException(method.Name)
        });
        var service = new TeamNotificationService(teamService, tournamentService, TestLocalizationService.Instance);

        await service.RefreshAsync();

        Assert.Equal(2, service.Notifications.Count);
        Assert.Contains(service.Notifications, item => item.Id == $"team-invite:{inviteId:N}" && item.Kind == TeamNotificationKind.TeamInvite);
        Assert.Contains(service.Notifications, item => item.Id == $"roster-selection:{rosterMemberId:N}" && item.Kind == TeamNotificationKind.RosterSelection);
    }

    [Fact]
    public async Task RosterDeclineUsesRosterRouteThenRefreshesWithoutRemovingTeamInvite()
    {
        var roster = new PendingRosterConfirmationDTO
        {
            RosterMemberId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            TournamentName = "Finals",
            TeamId = Guid.NewGuid(),
            TeamName = "Selected",
            SelectedAtUtc = DateTime.UtcNow
        };
        var pending = new List<PendingRosterConfirmationDTO> { roster };
        var declineCalls = 0;
        var teamService = CreateProxy<ITeamService>((method, _) => method.Name switch
        {
            nameof(ITeamService.GetCurrentUserTeamSummaryAsync) => Task.FromResult(new CurrentUserTeamSummaryDTO
            {
                ReceivedPendingInvites = [new TeamInviteSummaryDTO
                {
                    Id = Guid.NewGuid(), TeamId = Guid.NewGuid(), TeamName = "Inviters", CreatedAt = DateTime.UtcNow
                }]
            }),
            _ => throw new NotSupportedException(method.Name)
        });
        var tournamentService = CreateProxy<ITournamentService>((method, _) => method.Name switch
        {
            nameof(ITournamentService.GetPendingRosterConfirmationsAsync) => Task.FromResult<IReadOnlyList<PendingRosterConfirmationDTO>>(pending.ToList()),
            nameof(ITournamentService.DeclineTournamentRosterMemberAsync) => Decline(),
            _ => throw new NotSupportedException(method.Name)
        });
        var service = new TeamNotificationService(teamService, tournamentService, TestLocalizationService.Instance);
        await service.RefreshAsync();
        var notification = Assert.Single(service.Notifications.Where(item => item.Kind == TeamNotificationKind.RosterSelection));

        await service.RespondToRosterSelectionAsync(notification.Id, accept: false);

        Assert.Equal(1, declineCalls);
        Assert.DoesNotContain(service.Notifications, item => item.Kind == TeamNotificationKind.RosterSelection);
        Assert.Contains(service.Notifications, item => item.Kind == TeamNotificationKind.TeamInvite);

        Task Decline()
        {
            declineCalls++;
            pending.Clear();
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task RosterAcceptUsesExistingConfirmationMutationThenRefreshes()
    {
        var roster = new PendingRosterConfirmationDTO
        {
            RosterMemberId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            TournamentName = "Finals",
            TeamId = Guid.NewGuid(),
            TeamName = "Selected",
            SelectedAtUtc = DateTime.UtcNow
        };
        var pending = new List<PendingRosterConfirmationDTO> { roster };
        var confirmCalls = 0;
        var teamService = CreateProxy<ITeamService>((method, _) => method.Name == nameof(ITeamService.GetCurrentUserTeamSummaryAsync)
            ? Task.FromResult(new CurrentUserTeamSummaryDTO())
            : throw new NotSupportedException(method.Name));
        var tournamentService = CreateProxy<ITournamentService>((method, _) => method.Name switch
        {
            nameof(ITournamentService.GetPendingRosterConfirmationsAsync) => Task.FromResult<IReadOnlyList<PendingRosterConfirmationDTO>>(pending.ToList()),
            nameof(ITournamentService.ConfirmTournamentRosterMemberAsync) => Confirm(),
            _ => throw new NotSupportedException(method.Name)
        });
        var service = new TeamNotificationService(teamService, tournamentService, TestLocalizationService.Instance);
        await service.RefreshAsync();
        var notification = Assert.Single(service.Notifications);

        await service.RespondToRosterSelectionAsync(notification.Id, accept: true);

        Assert.Equal(1, confirmCalls);
        Assert.Empty(service.Notifications);

        Task<TournamentRegistrationDTO> Confirm()
        {
            confirmCalls++;
            pending.Clear();
            return Task.FromResult(new TournamentRegistrationDTO());
        }
    }

    [Fact]
    public async Task RosterDecisionChangedIdentifiesTournamentAfterBellMutation()
    {
        var roster = new PendingRosterConfirmationDTO
        {
            RosterMemberId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            TournamentName = "Finals",
            TeamId = Guid.NewGuid(),
            TeamName = "Selected",
            SelectedAtUtc = DateTime.UtcNow
        };
        var pending = new List<PendingRosterConfirmationDTO> { roster };
        var teamService = CreateProxy<ITeamService>((method, _) => method.Name == nameof(ITeamService.GetCurrentUserTeamSummaryAsync)
            ? Task.FromResult(new CurrentUserTeamSummaryDTO())
            : throw new NotSupportedException(method.Name));
        var tournamentService = CreateProxy<ITournamentService>((method, _) => method.Name switch
        {
            nameof(ITournamentService.GetPendingRosterConfirmationsAsync) => Task.FromResult<IReadOnlyList<PendingRosterConfirmationDTO>>(pending.ToList()),
            nameof(ITournamentService.ConfirmTournamentRosterMemberAsync) => Confirm(),
            _ => throw new NotSupportedException(method.Name)
        });
        var service = new TeamNotificationService(teamService, tournamentService, TestLocalizationService.Instance);
        var changedTournamentId = Guid.Empty;
        service.RosterDecisionChanged += tournamentId =>
        {
            changedTournamentId = tournamentId;
            return Task.CompletedTask;
        };

        await service.RefreshAsync();
        var notification = Assert.Single(service.Notifications);

        await service.RespondToRosterSelectionAsync(notification.Id, accept: true);

        Assert.Equal(roster.TournamentId, changedTournamentId);

        Task<TournamentRegistrationDTO> Confirm()
        {
            pending.Clear();
            return Task.FromResult(new TournamentRegistrationDTO());
        }
    }

    [Fact]
    public async Task TournamentServiceLoadsEveryPendingRosterPage()
    {
        var requestedPages = new List<int>();
        var client = CreateProxy<ILANClient>((method, args) =>
        {
            if(method.Name != nameof(ILANClient.GetPendingRosterConfirmationsAsync))
                throw new NotSupportedException(method.Name);

            var page = (int)args![0]!;
            requestedPages.Add(page);
            return Task.FromResult(new PendingRosterConfirmationPageDTO
            {
                Page = page,
                PageSize = 50,
                TotalCount = 51,
                Items = page == 1
                    ? Enumerable.Range(0, 50).Select(_ => new PendingRosterConfirmationDTO { RosterMemberId = Guid.NewGuid() }).ToList()
                    : [new PendingRosterConfirmationDTO { RosterMemberId = Guid.NewGuid() }]
            });
        });
        var service = new TournamentService(client, new ConfigurationBuilder().Build());

        var result = await service.GetPendingRosterConfirmationsAsync();

        Assert.Equal(51, result.Count);
        Assert.Equal([1, 2], requestedPages);
    }

    private static T CreateProxy<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, ServiceProxy>();
        ((ServiceProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while(directory is not null)
        {
            var path = Path.Combine(directory.FullName, relativePath);
            if(File.Exists(path))
                return File.ReadAllText(path);

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"Could not locate '{relativePath}'.");
    }

    private class ServiceProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = (_, _) => null;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            Handler(targetMethod ?? throw new InvalidOperationException("Missing proxy method."), args);
    }
}
