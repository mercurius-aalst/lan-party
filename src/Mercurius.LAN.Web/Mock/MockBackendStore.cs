using System.Text.Json;
using System.Text.Json.Serialization;
using Mercurius.LAN.Web.DTOs.Tournaments;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.DTOs.Registrations;
using Mercurius.LAN.Web.DTOs.Search;
using Mercurius.LAN.Web.DTOs.Sponsors;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Options;
using Microsoft.Extensions.Options;

namespace Mercurius.LAN.Web.Mock;

internal sealed class MockBackendStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private static readonly Guid FeaturedDoubleEliminationTournamentId = Guid.Parse("11111111-1111-1111-1111-111111111112");
    private static readonly DateTime FeaturedFixtureCreatedAtUtc = new(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FeaturedFixtureUpdatedAtUtc = new(2026, 5, 11, 12, 0, 0, DateTimeKind.Utc);

    private readonly object _syncRoot = new();
    private readonly string _dataFilePath;
    private readonly Dictionary<Guid, List<TournamentRegistrationDTO>> _registrationDetails = [];
    private MockBackendDocument _document;

    public MockBackendStore(IHostEnvironment environment, IOptions<MockBackendOptions> options)
    {
        _dataFilePath = Path.Combine(environment.ContentRootPath, options.Value.DataFilePath);
        _document = LoadDocument(_dataFilePath);
        SeedFeaturedDoubleEliminationFixture();
        InitializeRegistrationDetails();
    }

    public List<Tournament> GetTournaments(int? page = null, int? pageSize = null)
    {
        lock(_syncRoot)
        {
            var tournaments = _document.Tournaments
                .OrderBy(tournament => tournament.PlannedStartTime)
                .ThenBy(tournament => tournament.Name, StringComparer.OrdinalIgnoreCase)
                .Select(ToTournament);

            if(pageSize is > 0)
            {
                var pageNumber = Math.Max(page ?? 1, 1);
                tournaments = tournaments.Skip((pageNumber - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return Clone(tournaments.ToList())!;
        }
    }

    public List<GlobalSearchResultDTO> SearchGlobal(string query)
    {
        lock(_syncRoot)
        {
            var trimmedQuery = query.Trim();
            if(trimmedQuery.Length < 3)
                return [];

            var userResults = _document.Users
                .Where(user =>
                    !user.IsDeleted &&
                    !string.IsNullOrWhiteSpace(user.Username) &&
                    user.Username.Contains(trimmedQuery, StringComparison.OrdinalIgnoreCase))
                .OrderBy(user => user.Username!.StartsWith(trimmedQuery, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(user => user.Username, StringComparer.OrdinalIgnoreCase)
                .Select(user => new GlobalSearchResultDTO
                {
                    Type = GlobalSearchResultType.User,
                    DisplayLabel = user.Username!,
                    SupportingText = "User",
                    UserId = user.Id,
                    Username = user.Username
                });

            var teamResults = _document.Teams
                .Where(team =>
                    !string.IsNullOrWhiteSpace(team.Name) &&
                    team.Name.Contains(trimmedQuery, StringComparison.OrdinalIgnoreCase))
                .OrderBy(team => team.Name.StartsWith(trimmedQuery, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(team => team.Name, StringComparer.OrdinalIgnoreCase)
                .Select(team => new GlobalSearchResultDTO
                {
                    Type = GlobalSearchResultType.Team,
                    DisplayLabel = team.Name,
                    SupportingText = "Team",
                    TeamName = team.Name
                });

            var tournamentResults = _document.Tournaments
                .Where(tournament =>
                    !string.IsNullOrWhiteSpace(tournament.Name) &&
                    tournament.Name.Contains(trimmedQuery, StringComparison.OrdinalIgnoreCase))
                .OrderBy(tournament => tournament.Name.StartsWith(trimmedQuery, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(tournament => tournament.Name, StringComparer.OrdinalIgnoreCase)
                .Select(tournament => new GlobalSearchResultDTO
                {
                    Type = GlobalSearchResultType.Tournament,
                    DisplayLabel = tournament.Name,
                    SupportingText = "Tournament",
                    TournamentId = tournament.Id
                });

            return userResults
                .Concat(teamResults)
                .Concat(tournamentResults)
                .ToList();
        }
    }

    public PublicUserProfileDTO? GetPublicUserByUsername(string username)
    {
        lock(_syncRoot)
        {
            var normalizedUsername = username.Trim();
            if(string.IsNullOrWhiteSpace(normalizedUsername))
                return null;

            var user = _document.Users.FirstOrDefault(candidate =>
                !candidate.IsDeleted &&
                string.Equals(candidate.Username, normalizedUsername, StringComparison.OrdinalIgnoreCase));

            if(user == null ||
               string.IsNullOrWhiteSpace(user.Username) ||
               string.IsNullOrWhiteSpace(user.Firstname) ||
               string.IsNullOrWhiteSpace(user.Lastname))
                return null;

            return new PublicUserProfileDTO
            {
                Username = user.Username!,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                DiscordId = user.DiscordId,
                SteamId = user.SteamId,
                RiotId = user.RiotId
            };
        }
    }

    public PublicTeamProfileDTO? GetPublicTeamByName(string teamName)
    {
        lock(_syncRoot)
        {
            var normalizedTeamName = teamName.Trim();
            if(string.IsNullOrWhiteSpace(normalizedTeamName))
                return null;

            var team = _document.Teams.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, normalizedTeamName, StringComparison.OrdinalIgnoreCase));

            if(team == null)
                return null;

            var members = team.Members
                .Where(member => !string.IsNullOrWhiteSpace(member.Username))
                .GroupBy(member => member.Username!, StringComparer.OrdinalIgnoreCase)
                .Select(group => new PublicTeamMemberDTO
                {
                    Username = group.First().Username!
                })
                .OrderBy(member => member.Username, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var captainUsername = team.Members
                .FirstOrDefault(member => member.Id == team.CaptainUserId)?.Username
                ?? _document.Users.FirstOrDefault(member => member.Id == team.CaptainUserId)?.Username;

            var tournaments = _document.Tournaments
                .Where(tournament => tournament.Teams.Any(candidate => candidate.Id == team.Id))
                .OrderBy(tournament => tournament.StartTime)
                .ThenBy(tournament => tournament.Name)
                .Select(tournament => new PublicTeamTournamentDTO
                {
                    TournamentId = tournament.Id,
                    Name = tournament.Name
                })
                .ToList();

            return new PublicTeamProfileDTO
            {
                TeamName = team.Name,
                CaptainUsername = captainUsername,
                LogoUrl = team.LogoUrl,
                Members = members,
                Tournaments = tournaments
            };
        }
    }

    public TournamentExtended? GetTournament(Guid id)
    {
        lock(_syncRoot)
        {
            return Clone(_document.Tournaments.FirstOrDefault(tournament => tournament.Id == id));
        }
    }

    public TournamentExtended CreateTournament(CreateTournamentDTO dto)
    {
        lock(_syncRoot)
        {
            var tournament = new TournamentExtended
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                StartTime = dto.PlannedStartTime,
                EndTime = dto.PlannedStartTime.AddHours(4),
                PlannedStartTime = dto.PlannedStartTime,
                AverageGameDurationMinutes = dto.AverageGameDurationMinutes,
                RoundBreakDurationMinutes = dto.RoundBreakDurationMinutes,
                EstimatedEndTime = null,
                ImageUrl = "/mock-data-local/generated-tournament.svg",
                Status = TournamentStatus.Scheduled,
                BracketType = dto.BracketType,
                Format = dto.Format,
                FinalsFormat = dto.FinalsFormat,
                ParticipationMode = dto.ParticipationMode.GetValueOrDefault(),
                TeamSize = dto.TeamSize
            };

            _document.Tournaments.Add(tournament);
            return Clone(tournament)!;
        }
    }

    public TournamentExtended UpdateTournament(Guid id, UpdateTournamentDTO dto)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(id);
            tournament.Name = dto.Name;
            tournament.Format = dto.Format;
            tournament.FinalsFormat = dto.FinalsFormat;
            tournament.BracketType = dto.BracketType;
            tournament.ParticipationMode = dto.ParticipationMode.GetValueOrDefault(tournament.ParticipationMode);
            tournament.TeamSize = dto.TeamSize;
            tournament.PlannedStartTime = dto.PlannedStartTime;
            tournament.AverageGameDurationMinutes = dto.AverageGameDurationMinutes;
            tournament.RoundBreakDurationMinutes = dto.RoundBreakDurationMinutes;
            tournament.EstimatedEndTime = tournament.Matches.Any() ? tournament.Matches.Max(match => match.EstimatedEndTime) : null;

            if(dto.Image != null)
                tournament.ImageUrl = "/mock-data-local/generated-tournament.svg";

            return Clone(tournament)!;
        }
    }

    public TournamentExtended ReplaceTournamentSponsors(Guid id, ReplaceTournamentSponsorsDTO dto)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(id);
            var placements = (dto.SponsorPlacements ?? []).Take(1).ToList();

            tournament.SponsorPlacement = placements
                .Select((placement, index) =>
                {
                    var sponsor = _document.Sponsors.Single(existing => existing.Id == placement.SponsorId);
                    return new TournamentSponsorPlacement
                    {
                        Id = index + 1,
                        SponsorId = sponsor.Id,
                        SponsorName = sponsor.Name,
                        SponsorTier = sponsor.SponsorTier,
                        SponsorLogoUrl = sponsor.LogoUrl,
                        SponsorInfoUrl = sponsor.InfoUrl,
                        SponsorDescription = sponsor.Description,
                        Context = placement.Context,
                        Headline = placement.Headline,
                        SupportLine = placement.SupportLine,
                        DisplayOrder = placement.DisplayOrder
                    };
                })
                .OrderBy(placement => placement.Context)
                .ThenBy(placement => placement.DisplayOrder)
                .ThenBy(placement => placement.SponsorName)
                .FirstOrDefault();

            return Clone(tournament)!;
        }
    }

    public void SetTournamentLifecycleState(Guid tournamentId, TournamentStatus state)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            tournament.Status = state;

            if(state == TournamentStatus.Scheduled)
            {
                tournament.Placements = [];
                foreach(var match in tournament.Matches)
                {
                    match.Participant1Score = null;
                    match.Participant2Score = null;
                    match.UserWinnerId = null;
                    match.UserLoserId = null;
                    match.TeamWinnerId = null;
                    match.TeamLoserId = null;
                }
            }
            else if(state == TournamentStatus.Completed && !tournament.Placements.Any())
            {
                tournament.Placements = BuildPlacements(tournament);
            }
        }
    }

    public void DeleteTournament(Guid tournamentId)
    {
        lock(_syncRoot)
        {
            _document.Tournaments.RemoveAll(tournament => tournament.Id == tournamentId);
            _registrationDetails.Remove(tournamentId);
        }
    }

    public Match UpdateMatch(Guid matchId, UpdateMatchDTO dto)
    {
        lock(_syncRoot)
        {
            var tournament = _document.Tournaments.FirstOrDefault(candidate => candidate.Matches.Any(match => match.Id == matchId))
                ?? throw new InvalidOperationException($"Mock match '{matchId}' was not found.");

            var match = tournament.Matches.First(existing => existing.Id == matchId);
            match.Participant1Score = dto.Participant1Score;
            match.Participant2Score = dto.Participant2Score;

            if(dto.Participant1Score == dto.Participant2Score)
            {
                match.UserWinnerId = null;
                match.UserLoserId = null;
                match.TeamWinnerId = null;
                match.TeamLoserId = null;
            }
            else if(match.ParticipationMode == ParticipationMode.Team)
            {
                var participant1Won = dto.Participant1Score > dto.Participant2Score;
                match.TeamWinnerId = participant1Won ? match.TeamParticipant1Id : match.TeamParticipant2Id;
                match.TeamLoserId = participant1Won ? match.TeamParticipant2Id : match.TeamParticipant1Id;
            }
            else
            {
                var participant1Won = dto.Participant1Score > dto.Participant2Score;
                match.UserWinnerId = participant1Won ? match.UserParticipant1Id : match.UserParticipant2Id;
                match.UserLoserId = participant1Won ? match.UserParticipant2Id : match.UserParticipant1Id;
            }

            return Clone(match)!;
        }
    }

    public Match GetMatch(Guid matchId)
    {
        lock(_syncRoot)
        {
            var match = _document.Tournaments
                .SelectMany(tournament => tournament.Matches)
                .FirstOrDefault(candidate => candidate.Id == matchId);

            return Clone(match)
                ?? throw new InvalidOperationException($"Mock match '{matchId}' was not found.");
        }
    }

    public CurrentUserTournamentRegistrationStateDTO GetCurrentUserTournamentRegistrationState(
        string persona,
        Guid tournamentId)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var currentUser = GetCurrentProfile(persona).User
                ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var registrations = GetRegistrationDetails(tournament);
            var individual = registrations.FirstOrDefault(registration =>
                registration.Kind == TournamentRegistrationKind.Individual &&
                registration.User?.Id == currentUser.Id);
            var pendingRoster = registrations
                .SelectMany(registration => registration.RosterMembers)
                .FirstOrDefault(member =>
                    member.User.Id == currentUser.Id &&
                    member.ConfirmationStatus == RosterMemberConfirmationStatus.Pending);
            var activeTeam = registrations.FirstOrDefault(registration =>
                registration.Kind == TournamentRegistrationKind.Team &&
                registration.Status == TournamentRegistrationStatus.Active &&
                registration.RosterMembers.Any(member => member.User.Id == currentUser.Id));
            var captainRegistrations = registrations
                .Where(registration =>
                    registration.Kind == TournamentRegistrationKind.Team &&
                    registration.Team?.CaptainUserId == currentUser.Id)
                .Select(Clone)
                .Where(registration => registration != null)
                .Cast<TournamentRegistrationDTO>()
                .ToList();

            var individualEligible = CheckIndividualEligibilityCore(tournament, registrations, currentUser.Id);

            return new CurrentUserTournamentRegistrationStateDTO
            {
                TournamentId = tournamentId,
                IndividualRegistration = Clone(individual),
                PendingRosterConfirmation = Clone(pendingRoster),
                ActiveTeamRegistration = Clone(activeTeam),
                CaptainManagedRegistrations = captainRegistrations,
                CanRegisterIndividual = individualEligible.Eligible && activeTeam == null && pendingRoster == null,
                CanConfirmRoster = pendingRoster != null,
                CanUnregister = individual != null || activeTeam != null || captainRegistrations.Count > 0
            };
        }
    }

    public EligibilityResponseDTO CheckIndividualTournamentRegistrationEligibility(
        string persona,
        Guid tournamentId)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var currentUser = GetCurrentProfile(persona).User
                ?? throw new InvalidOperationException("Mock profile does not have a user.");
            return CheckIndividualEligibilityCore(tournament, GetRegistrationDetails(tournament), currentUser.Id);
        }
    }

    public EligibilityResponseDTO CheckTeamTournamentRegistrationEligibility(
        string persona,
        Guid tournamentId,
        Guid teamId)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var currentUser = GetCurrentProfile(persona).User
                ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = GetRequiredTeam(teamId);
            var reasons = GetTeamEligibilityFailures(tournament, GetRegistrationDetails(tournament), team, currentUser.Id, excludedRegistrationId: null);
            return new EligibilityResponseDTO
            {
                Eligible = reasons.Count == 0,
                ReasonCodes = reasons
            };
        }
    }

    public RosterCandidateEligibilityResponseDTO CheckTeamRosterEligibility(
        string persona,
        Guid tournamentId,
        Guid teamId,
        SubmitTeamRosterDTO roster)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var currentUser = GetCurrentProfile(persona).User
                ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = GetRequiredTeam(teamId);
            var registrations = GetRegistrationDetails(tournament);
            var existing = registrations.FirstOrDefault(registration =>
                registration.Kind == TournamentRegistrationKind.Team &&
                registration.Team?.Id == teamId);
            var reasons = GetTeamEligibilityFailures(tournament, registrations, team, currentUser.Id, existing?.Id);
            var userIds = roster.UserIds ?? [];
            if(userIds.Count != userIds.Distinct().Count())
                reasons.Add("roster_user_ids_must_be_unique");
            if(tournament.TeamSize.HasValue && userIds.Distinct().Count() != tournament.TeamSize.Value)
                reasons.Add("exact_roster_size_required");

            var teamMemberIds = team.Members.Select(member => member.Id).ToHashSet();
            var candidates = userIds
                .Distinct()
                .Select(userId =>
                {
                    var user = _document.Users.FirstOrDefault(candidate => candidate.Id == userId && !candidate.IsDeleted);
                    var candidateReasons = new List<string>();
                    if(user == null)
                        candidateReasons.Add("user_not_found");
                    if(!teamMemberIds.Contains(userId))
                        candidateReasons.Add("user_not_team_member");
                    return new RosterCandidateEligibilityDTO
                    {
                        UserId = userId,
                        User = user == null ? null : ToPublicUser(user),
                        Eligible = candidateReasons.Count == 0,
                        ReasonCodes = candidateReasons
                    };
                })
                .ToList();

            reasons.AddRange(candidates.SelectMany(candidate => candidate.ReasonCodes));
            return new RosterCandidateEligibilityResponseDTO
            {
                Eligible = reasons.Count == 0,
                ReasonCodes = reasons.Distinct(StringComparer.Ordinal).ToList(),
                Candidates = candidates
            };
        }
    }

    public TournamentRegistrationDTO RegisterCurrentUserForTournament(string persona, Guid tournamentId)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var currentUser = GetCurrentProfile(persona).User
                ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var registrations = GetRegistrationDetails(tournament);
            var eligibility = CheckIndividualEligibilityCore(tournament, registrations, currentUser.Id);
            EnsureEligible(eligibility);

            var now = DateTime.UtcNow;
            var registration = new TournamentRegistrationDTO
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                Kind = TournamentRegistrationKind.Individual,
                Status = TournamentRegistrationStatus.Active,
                User = ToPublicUser(currentUser),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            registrations.Add(registration);
            UpdatePublicRegistrationProjection(tournament, registrations);
            return Clone(registration)!;
        }
    }

    public void DeleteCurrentUserTournamentRegistration(string persona, Guid tournamentId)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var currentUser = GetCurrentProfile(persona).User
                ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var registrations = GetRegistrationDetails(tournament);
            var registration = registrations.FirstOrDefault(candidate =>
                candidate.Kind == TournamentRegistrationKind.Individual &&
                candidate.User?.Id == currentUser.Id &&
                candidate.Status == TournamentRegistrationStatus.Active);

            if(registration == null)
                throw new InvalidOperationException("Individual registration not found.");

            registrations.Remove(registration);
            UpdatePublicRegistrationProjection(tournament, registrations);
        }
    }

    public TournamentRegistrationDTO SubmitTeamTournamentRoster(
        string persona,
        Guid tournamentId,
        Guid teamId,
        SubmitTeamRosterDTO roster)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var currentUser = GetCurrentProfile(persona).User
                ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = GetRequiredTeam(teamId);
            var registrations = GetRegistrationDetails(tournament);
            var existing = registrations.FirstOrDefault(registration =>
                registration.Kind == TournamentRegistrationKind.Team &&
                registration.Team?.Id == teamId);
            var eligibility = CheckTeamRosterEligibility(persona, tournamentId, teamId, roster);
            EnsureEligible(new EligibilityResponseDTO
            {
                Eligible = eligibility.Eligible,
                ReasonCodes = eligibility.ReasonCodes
            });

            if(existing != null)
                registrations.Remove(existing);

            var now = DateTime.UtcNow;
            var rosterMembers = roster.UserIds
                .Distinct()
                .Select(userId =>
                {
                    var user = GetRequiredUser(userId);
                    return new TournamentRosterMemberDTO
                    {
                        Id = Guid.NewGuid(),
                        User = ToPublicUser(user),
                        IsCaptain = userId == team.CaptainUserId,
                        ConfirmationStatus = userId == team.CaptainUserId
                            ? RosterMemberConfirmationStatus.AutoConfirmed
                            : RosterMemberConfirmationStatus.Pending
                    };
                })
                .ToList();
            var registration = new TournamentRegistrationDTO
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                Kind = TournamentRegistrationKind.Team,
                Status = rosterMembers.Any(member => member.ConfirmationStatus == RosterMemberConfirmationStatus.Pending)
                    ? TournamentRegistrationStatus.PendingConfirmation
                    : TournamentRegistrationStatus.Active,
                Team = new TeamParticipantDTO
                {
                    Id = team.Id,
                    Name = team.Name,
                    CaptainUserId = team.CaptainUserId,
                    LogoUrl = team.LogoUrl,
                    Members = Clone(team.Members.ToList())!
                },
                RosterMembers = rosterMembers,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            registrations.Add(registration);
            UpdatePublicRegistrationProjection(tournament, registrations);
            return Clone(registration)!;
        }
    }

    public void DeleteTeamTournamentRegistration(string persona, Guid tournamentId, Guid teamId)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var currentUser = GetCurrentProfile(persona).User
                ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var registrations = GetRegistrationDetails(tournament);
            var registration = registrations.FirstOrDefault(candidate =>
                candidate.Kind == TournamentRegistrationKind.Team &&
                candidate.Team?.Id == teamId &&
                (candidate.Team.CaptainUserId == currentUser.Id ||
                 candidate.RosterMembers.Any(member => member.User.Id == currentUser.Id)));

            if(registration == null)
                throw new InvalidOperationException("Team registration not found.");

            registrations.Remove(registration);
            UpdatePublicRegistrationProjection(tournament, registrations);
        }
    }

    public TournamentRegistrationDTO ConfirmTournamentRosterMember(
        string persona,
        Guid tournamentId,
        Guid rosterMemberId)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var currentUser = GetCurrentProfile(persona).User
                ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var registrations = GetRegistrationDetails(tournament);
            var registration = registrations.FirstOrDefault(candidate =>
                candidate.Kind == TournamentRegistrationKind.Team &&
                candidate.RosterMembers.Any(member => member.Id == rosterMemberId && member.User.Id == currentUser.Id));
            var member = registration?.RosterMembers.FirstOrDefault(candidate => candidate.Id == rosterMemberId);

            if(registration == null || member == null || member.ConfirmationStatus != RosterMemberConfirmationStatus.Pending)
                throw new InvalidOperationException("Pending roster confirmation not found.");

            var updatedRosterMembers = registration.RosterMembers
                .Select(candidate => candidate.Id == rosterMemberId
                    ? new TournamentRosterMemberDTO
                    {
                        Id = candidate.Id,
                        User = Clone(candidate.User)!,
                        IsCaptain = candidate.IsCaptain,
                        ConfirmationStatus = RosterMemberConfirmationStatus.Confirmed
                    }
                    : candidate)
                .ToList();
            var updatedRegistration = new TournamentRegistrationDTO
            {
                Id = registration.Id,
                TournamentId = registration.TournamentId,
                Kind = registration.Kind,
                Status = updatedRosterMembers.All(candidate =>
                    candidate.ConfirmationStatus != RosterMemberConfirmationStatus.Pending)
                    ? TournamentRegistrationStatus.Active
                    : TournamentRegistrationStatus.PendingConfirmation,
                User = Clone(registration.User),
                Team = Clone(registration.Team),
                RosterMembers = updatedRosterMembers,
                CreatedAtUtc = registration.CreatedAtUtc,
                UpdatedAtUtc = DateTime.UtcNow
            };
            var registrationIndex = registrations.IndexOf(registration);
            registrations[registrationIndex] = updatedRegistration;
            registration = updatedRegistration;
            UpdatePublicRegistrationProjection(tournament, registrations);
            return Clone(registration)!;
        }
    }

    public List<AdminTournamentRegistrationDTO> GetAdminTournamentRegistrations(
        Guid tournamentId,
        int? page,
        int? pageSize)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var registrations = GetRegistrationDetails(tournament)
                .OrderBy(registration => registration.Kind)
                .ThenBy(registration => registration.Status)
                .ThenBy(registration => registration.CreatedAtUtc)
                .ThenBy(registration => registration.Id)
                .Select(ToAdminRegistration)
                .AsEnumerable();

            if(pageSize is > 0)
            {
                var pageNumber = Math.Max(page ?? 1, 1);
                registrations = registrations.Skip((pageNumber - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return registrations.ToList();
        }
    }

    public void RemoveTournamentUserRegistrationAsAdmin(Guid tournamentId, Guid userId, string? reason)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var registrations = GetRegistrationDetails(tournament);
            var registration = registrations.FirstOrDefault(candidate =>
                candidate.Kind == TournamentRegistrationKind.Individual && candidate.User?.Id == userId);
            if(registration == null)
                throw new InvalidOperationException("Individual registration not found.");

            registrations.Remove(registration);
            UpdatePublicRegistrationProjection(tournament, registrations);
        }
    }

    public void RemoveTournamentTeamRegistrationAsAdmin(Guid tournamentId, Guid teamId, string? reason)
    {
        lock(_syncRoot)
        {
            var tournament = GetRequiredTournament(tournamentId);
            var registrations = GetRegistrationDetails(tournament);
            var registration = registrations.FirstOrDefault(candidate =>
                candidate.Kind == TournamentRegistrationKind.Team && candidate.Team?.Id == teamId);
            if(registration == null)
                throw new InvalidOperationException("Team registration not found.");

            registrations.Remove(registration);
            UpdatePublicRegistrationProjection(tournament, registrations);
        }
    }

    private void InitializeRegistrationDetails()
    {
        foreach(var tournament in _document.Tournaments)
            _ = GetRegistrationDetails(tournament);
    }

    private List<TournamentRegistrationDTO> GetRegistrationDetails(TournamentExtended tournament)
    {
        if(_registrationDetails.TryGetValue(tournament.Id, out var registrations))
            return registrations;

        registrations = tournament.Registrations
            .Where(registration => registration.Status == TournamentRegistrationStatus.Active)
            .Select(ToRegistrationDetails)
            .ToList();
        _registrationDetails[tournament.Id] = registrations;
        return registrations;
    }

    private static TournamentRegistrationDTO ToRegistrationDetails(PublicTournamentRegistrationDTO registration)
    {
        var now = DateTime.UtcNow;
        var rosterMembers = registration.RosterMembers
            .Select(member => new TournamentRosterMemberDTO
            {
                Id = Guid.NewGuid(),
                User = Clone(member.User)!,
                IsCaptain = member.IsCaptain,
                ConfirmationStatus = RosterMemberConfirmationStatus.AutoConfirmed
            })
            .ToList();

        return new TournamentRegistrationDTO
        {
            Id = registration.Id,
            TournamentId = registration.TournamentId,
            Kind = registration.Kind,
            Status = registration.Status,
            User = Clone(registration.User),
            Team = registration.Team == null
                ? null
                : new TeamParticipantDTO
                {
                    Id = registration.Team.Id,
                    Name = registration.Team.Name,
                    CaptainUserId = registration.Team.CaptainUserId,
                    LogoUrl = registration.Team.LogoUrl,
                    Members = []
                },
            RosterMembers = rosterMembers,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    private static PublicTournamentRegistrationDTO ToPublicRegistration(TournamentRegistrationDTO registration)
    {
        return new PublicTournamentRegistrationDTO
        {
            Id = registration.Id,
            TournamentId = registration.TournamentId,
            Kind = registration.Kind,
            Status = registration.Status,
            User = Clone(registration.User),
            Team = registration.Team == null
                ? null
                : new PublicTournamentTeamDTO
                {
                    Id = registration.Team.Id,
                    Name = registration.Team.Name,
                    CaptainUserId = registration.Team.CaptainUserId,
                    LogoUrl = registration.Team.LogoUrl,
                    Members = []
                },
            RosterMembers = registration.RosterMembers
                .OrderByDescending(member => member.IsCaptain)
                .ThenBy(member => member.User.Username, StringComparer.OrdinalIgnoreCase)
                .Select(member => new PublicTournamentRosterMemberDTO
                {
                    User = Clone(member.User)!,
                    IsCaptain = member.IsCaptain
                })
                .ToList()
        };
    }

    private static AdminTournamentRegistrationDTO ToAdminRegistration(TournamentRegistrationDTO registration)
    {
        return new AdminTournamentRegistrationDTO
        {
            Id = registration.Id,
            TournamentId = registration.TournamentId,
            Kind = registration.Kind,
            Status = registration.Status,
            User = Clone(registration.User),
            Team = registration.Team == null
                ? null
                : new TeamParticipantDTO
                {
                    Id = registration.Team.Id,
                    Name = registration.Team.Name,
                    CaptainUserId = registration.Team.CaptainUserId,
                    LogoUrl = registration.Team.LogoUrl,
                    Members = Clone(registration.Team.Members)!
                },
            RosterMembers = registration.RosterMembers.Select(Clone).Where(member => member != null).Cast<TournamentRosterMemberDTO>().ToList(),
            CreatedAtUtc = registration.CreatedAtUtc,
            UpdatedAtUtc = registration.UpdatedAtUtc
        };
    }

    private static EligibilityResponseDTO CheckIndividualEligibilityCore(
        TournamentExtended tournament,
        IReadOnlyCollection<TournamentRegistrationDTO> registrations,
        Guid userId)
    {
        var reasons = new List<string>();
        if(tournament.ParticipationMode != ParticipationMode.Individual)
            reasons.Add("not_individual_tournament");
        if(tournament.Status != TournamentStatus.Scheduled)
            reasons.Add("tournament_not_scheduled");
        if(registrations.Any(registration =>
               registration.User?.Id == userId ||
               registration.RosterMembers.Any(member => member.User.Id == userId)))
        {
            reasons.Add("duplicate_participation");
        }

        return new EligibilityResponseDTO
        {
            Eligible = reasons.Count == 0,
            ReasonCodes = reasons
        };
    }

    private static List<string> GetTeamEligibilityFailures(
        TournamentExtended tournament,
        IReadOnlyCollection<TournamentRegistrationDTO> registrations,
        Team team,
        Guid captainUserId,
        Guid? excludedRegistrationId)
    {
        var reasons = new List<string>();
        if(tournament.ParticipationMode != ParticipationMode.Team)
            reasons.Add("not_team_tournament");
        if(tournament.Status != TournamentStatus.Scheduled)
            reasons.Add("tournament_not_scheduled");
        if(!tournament.TeamSize.HasValue || tournament.TeamSize.Value <= 0)
            reasons.Add("team_size_required");
        if(registrations.Any(registration =>
               registration.Id != excludedRegistrationId &&
               registration.Kind == TournamentRegistrationKind.Team &&
               registration.Team?.Id == team.Id))
        {
            reasons.Add("team_already_registered");
        }
        if(registrations.Any(registration =>
               registration.Id != excludedRegistrationId &&
               (registration.User?.Id == captainUserId ||
                registration.RosterMembers.Any(member => member.User.Id == captainUserId))))
        {
            reasons.Add("captain_duplicate_participation");
        }

        return reasons;
    }

    private static void EnsureEligible(EligibilityResponseDTO eligibility)
    {
        if(!eligibility.Eligible)
        {
            var reason = eligibility.ReasonCodes.Count == 0
                ? "Registration is not eligible."
                : string.Join(", ", eligibility.ReasonCodes);
            throw new InvalidOperationException(reason);
        }
    }

    private static void UpdatePublicRegistrationProjection(
        TournamentExtended tournament,
        IEnumerable<TournamentRegistrationDTO> registrations)
    {
        tournament.Registrations = registrations
            .Where(registration => registration.Status == TournamentRegistrationStatus.Active)
            .Select(ToPublicRegistration)
            .ToList();
    }

    public List<Team> GetTeams(int page, int pageSize)
    {
        lock(_syncRoot)
        {
            if(page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");

            if(pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

            var offset = ((long)page - 1) * pageSize;
            if(offset > int.MaxValue)
                return [];

            var teams = _document.Teams
                .OrderBy(team => team.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(team => team.Id)
                .Skip((int)offset)
                .Take(pageSize)
                .ToList();

            return Clone(teams)!;
        }
    }

    public Team CreateTeam(CreateTeamDTO dto)
    {
        lock(_syncRoot)
        {
            var captain = GetRequiredUser(dto.CaptainUserId);
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                CaptainUserId = dto.CaptainUserId,
                Members = [PublicUserDTO.FromUser(captain)],
                TeamInvites = []
            };

            AddOrReplaceTeam(team);
            return Clone(team)!;
        }
    }

    public CurrentUserTeamSummaryDTO GetCurrentUserTeamSummary(string persona)
    {
        lock(_syncRoot)
        {
            var currentUser = GetCurrentProfile(persona).User ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var teams = EnsureCurrentUserTeamFixtures(currentUser);
            var captainedTeams = teams
                .Where(team => team.CaptainUserId == currentUser.Id)
                .OrderBy(team => team.Name, StringComparer.OrdinalIgnoreCase)
                .Select(ToManagementSummary)
                .ToList();
            var memberTeams = teams
                .Where(team => team.CaptainUserId != currentUser.Id && team.Members.Any(member => member.Id == currentUser.Id))
                .OrderBy(team => team.Name, StringComparer.OrdinalIgnoreCase)
                .Select(ToManagementSummary)
                .ToList();
            var invites = teams
                .SelectMany(team => team.TeamInvites.Select(invite => (team, invite)))
                .Where(entry => entry.invite.UserId == currentUser.Id && IsPending(entry.invite))
                .OrderBy(entry => entry.invite.CreatedAt)
                .Select(entry => ToInviteSummary(entry.team, entry.invite))
                .ToList();
            var sentInvites = teams
                .Where(team => team.CaptainUserId == currentUser.Id)
                .SelectMany(team => team.TeamInvites.Select(invite => (team, invite)))
                .Where(entry => IsPending(entry.invite))
                .OrderBy(entry => entry.invite.CreatedAt)
                .Select(entry => ToInviteSummary(entry.team, entry.invite))
                .ToList();

            return new CurrentUserTeamSummaryDTO
            {
                CaptainedTeams = captainedTeams,
                MemberTeams = memberTeams,
                ReceivedPendingInvites = invites,
                SentPendingInvites = sentInvites
            };
        }
    }

    public TeamManagementSummaryDTO CreateCurrentUserTeam(string persona, CreateTeamDTO dto)
    {
        lock(_syncRoot)
        {
            var currentUser = GetCurrentProfile(persona).User ?? throw new InvalidOperationException("Mock profile does not have a user.");
            EnsureCurrentUserTeamFixtures(currentUser);

            if(_document.Teams.Count(team => team.CaptainUserId == currentUser.Id) >= 3)
                throw new InvalidOperationException("You already captain the maximum number of mock teams.");

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                CaptainUserId = currentUser.Id,
                Members = [ToPublicUser(currentUser)],
                TeamInvites = []
            };

            AddOrReplaceTeam(team);
            return ToManagementSummary(team);
        }
    }

    public TeamInvite CreateTeamInvite(string persona, Guid teamId, Guid userId)
    {
        lock(_syncRoot)
        {
            var currentUser = GetCurrentProfile(persona).User ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = GetRequiredTeam(teamId);
            if(team.CaptainUserId != currentUser.Id)
                throw new InvalidOperationException("Only the team captain can send invites.");

            if(team.Members.Any(member => member.Id == userId))
                throw new InvalidOperationException("That player is already a team member.");

            if(team.TeamInvites.Any(invite => invite.UserId == userId && IsPending(invite)))
                throw new InvalidOperationException("That player already has a pending invite.");

            var createdAt = DateTime.UtcNow;
            var invite = new TeamInvite
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                UserId = userId,
                Status = "Pending",
                CreatedAt = createdAt,
                ExpiresAt = createdAt.AddDays(7)
            };
            team.TeamInvites = team.TeamInvites.Append(invite).ToList();
            AddOrReplaceTeam(team);
            return Clone(invite)!;
        }
    }

    public TeamInvite CancelTeamInvite(string persona, Guid teamId, Guid inviteId)
    {
        lock(_syncRoot)
        {
            var currentUser = GetCurrentProfile(persona).User ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = GetRequiredTeam(teamId);
            if(team.CaptainUserId != currentUser.Id)
                throw new InvalidOperationException("Only the team captain can cancel invites.");

            var invite = team.TeamInvites.First(candidate => candidate.Id == inviteId);
            EnsureInviteDates(invite);
            invite.Status = "Cancelled";
            invite.CancelledAt = DateTime.UtcNow;
            AddOrReplaceTeam(team);
            return Clone(invite)!;
        }
    }

    public TeamInvite RespondToTeamInvite(string persona, Guid inviteId, bool accept)
    {
        lock(_syncRoot)
        {
            var currentUser = GetCurrentProfile(persona).User ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = _document.Teams.First(team => team.TeamInvites.Any(invite => invite.Id == inviteId));
            var invite = team.TeamInvites.First(candidate => candidate.Id == inviteId);
            EnsureInviteDates(invite);
            if(invite.UserId != currentUser.Id)
                throw new InvalidOperationException("This invite belongs to another user.");

            invite.Status = accept ? "Accepted" : "Declined";
            invite.RespondedAt = DateTime.UtcNow;
            if(accept && team.Members.All(member => member.Id != currentUser.Id))
                team.Members = team.Members.Append(PublicUserDTO.FromUser(currentUser)).ToList();

            AddOrReplaceTeam(team);
            return Clone(invite)!;
        }
    }

    public TeamManagementSummaryDTO LeaveTeam(string persona, Guid teamId)
    {
        lock(_syncRoot)
        {
            var currentUser = GetCurrentProfile(persona).User ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = GetRequiredTeam(teamId);
            if(team.CaptainUserId == currentUser.Id)
                throw new InvalidOperationException("Transfer captainship before leaving a team you captain.");

            if(team.Name.Contains("Roster", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Mock roster lock: this team cannot be left during an active tournament.");

            team.Members = team.Members.Where(member => member.Id != currentUser.Id).ToList();
            AddOrReplaceTeam(team);
            return ToManagementSummary(team);
        }
    }

    public TeamManagementSummaryDTO RemoveTeamMember(string persona, Guid teamId, Guid userId)
    {
        lock(_syncRoot)
        {
            var currentUser = GetCurrentProfile(persona).User ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = GetRequiredTeam(teamId);
            if(team.CaptainUserId != currentUser.Id)
                throw new InvalidOperationException("Only the team captain can remove members.");

            if(team.CaptainUserId == userId)
                throw new InvalidOperationException("Transfer captainship before removing the captain.");

            if(team.Name.Contains("Roster", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Mock roster lock: this team cannot remove members during an active tournament.");

            if(team.Members.All(member => member.Id != userId))
                throw new InvalidOperationException("That player is not a member of this team.");

            team.Members = team.Members.Where(member => member.Id != userId).ToList();
            AddOrReplaceTeam(team);
            return ToManagementSummary(team);
        }
    }

    public TeamManagementSummaryDTO TransferCaptain(string persona, Guid teamId, Guid newCaptainUserId)
    {
        lock(_syncRoot)
        {
            var currentUser = GetCurrentProfile(persona).User ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = GetRequiredTeam(teamId);
            if(team.CaptainUserId != currentUser.Id)
                throw new InvalidOperationException("Only the current captain can transfer captainship.");

            if(team.Members.All(member => member.Id != newCaptainUserId))
                throw new InvalidOperationException("Captainship can only be transferred to a current member.");

            team.CaptainUserId = newCaptainUserId;
            AddOrReplaceTeam(team);
            return ToManagementSummary(team);
        }
    }

    public TeamLogoResponseDTO UploadTeamLogo(string persona, Guid teamId, string contentType, string fileName)
    {
        lock(_syncRoot)
        {
            var currentUser = GetCurrentProfile(persona).User ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = GetRequiredTeam(teamId);
            if(team.CaptainUserId != currentUser.Id)
                throw new InvalidOperationException("Only the team captain can update the team logo.");

            if(!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Mock validation: choose an image file for the team logo.");

            var extension = Path.GetExtension(fileName);
            team.LogoUrl = $"/mock-data-local/sponsors/mock-sponsor.svg?team={team.Id:N}{extension}";
            AddOrReplaceTeam(team);
            return new TeamLogoResponseDTO { TeamId = team.Id, LogoUrl = team.LogoUrl };
        }
    }

    public TeamLogoResponseDTO RemoveTeamLogo(string persona, Guid teamId)
    {
        lock(_syncRoot)
        {
            var currentUser = GetCurrentProfile(persona).User ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = GetRequiredTeam(teamId);
            if(team.CaptainUserId != currentUser.Id)
                throw new InvalidOperationException("Only the team captain can remove the team logo.");

            team.LogoUrl = null;
            AddOrReplaceTeam(team);
            return new TeamLogoResponseDTO { TeamId = team.Id, LogoUrl = null };
        }
    }

    public void DeleteCurrentUserTeam(string persona, Guid teamId)
    {
        lock(_syncRoot)
        {
            var currentUser = GetCurrentProfile(persona).User ?? throw new InvalidOperationException("Mock profile does not have a user.");
            var team = GetRequiredTeam(teamId);
            if(team.CaptainUserId != currentUser.Id)
                throw new InvalidOperationException("Only the team captain can delete the team.");

            DeleteTeam(teamId);
        }
    }

    public Team UpdateTeam(Guid id, UpdateTeamDTO dto)
    {
        lock(_syncRoot)
        {
            var team = GetRequiredTeam(id);
            team.Name = string.IsNullOrWhiteSpace(dto.Name) ? team.Name : dto.Name;
            team.CaptainUserId = dto.CaptainUserId ?? team.CaptainUserId;

            if(dto.CaptainUserId.HasValue && team.Members.All(member => member.Id != dto.CaptainUserId.Value))
                team.Members = team.Members.Append(PublicUserDTO.FromUser(GetRequiredUser(dto.CaptainUserId.Value))).ToList();

            AddOrReplaceTeam(team);
            return Clone(team)!;
        }
    }

    public void DeleteTeam(Guid id)
    {
        lock(_syncRoot)
        {
            _document.Teams.RemoveAll(team => team.Id == id);

            foreach(var tournament in _document.Tournaments)
            {
                tournament.Teams = tournament.Teams.Where(team => team.Id != id).ToList();
            }
        }
    }

    public List<Sponsor> GetSponsors()
    {
        lock(_syncRoot)
        {
            return Clone(_document.Sponsors.OrderBy(sponsor => sponsor.SponsorTier).ToList())!;
        }
    }

    public Sponsor GetSponsorById(int id)
    {
        lock(_syncRoot)
        {
            return Clone(_document.Sponsors.Single(sponsor => sponsor.Id == id))!;
        }
    }

    public Sponsor CreateSponsor(SponsorManagementDTO dto)
    {
        lock(_syncRoot)
        {
            var nextId = _document.Sponsors.Count == 0 ? 1 : _document.Sponsors.Max(sponsor => sponsor.Id) + 1;
            var sponsor = new Sponsor
            {
                Id = nextId,
                Name = dto.Name,
                SponsorTier = dto.SponsorTier,
                InfoUrl = dto.InfoUrl,
                Description = dto.Description,
                LogoUrl = "/mock-data-local/sponsors/mock-sponsor.svg"
            };

            _document.Sponsors.Add(sponsor);
            return Clone(sponsor)!;
        }
    }

    public Sponsor UpdateSponsor(int id, SponsorManagementDTO dto)
    {
        lock(_syncRoot)
        {
            var sponsor = _document.Sponsors.Single(existing => existing.Id == id);
            sponsor.Name = dto.Name;
            sponsor.SponsorTier = dto.SponsorTier;
            sponsor.InfoUrl = dto.InfoUrl;
            sponsor.Description = dto.Description;

            if(dto.Logo != null)
                sponsor.LogoUrl = "/mock-data-local/sponsors/mock-sponsor.svg";

            return Clone(sponsor)!;
        }
    }

    public void DeleteSponsor(int id)
    {
        lock(_syncRoot)
        {
            _document.Sponsors.RemoveAll(sponsor => sponsor.Id == id);

            foreach(var tournament in _document.Tournaments)
            {
                if(tournament.SponsorPlacement?.SponsorId == id)
                    tournament.SponsorPlacement = null;
            }
        }
    }

    public CurrentUserProfileResponse GetCurrentProfile(string persona)
    {
        lock(_syncRoot)
        {
            var normalizedPersona = NormalizePersona(persona);
            var profile = _document.Profiles.FirstOrDefault(candidate =>
                string.Equals(candidate.Persona, normalizedPersona, StringComparison.OrdinalIgnoreCase));

            if(profile == null)
                throw new InvalidOperationException($"Mock profile for persona '{normalizedPersona}' was not found.");

            return Clone(profile.Profile)!;
        }
    }

    public UserProfileDTO CompleteCurrentProfile(string persona, CompleteUserProfileRequest request)
    {
        lock(_syncRoot)
        {
            return UpdateCurrentProfileCore(persona, request.Username, request.Firstname, request.Lastname, request.DiscordId, request.SteamId, request.RiotId, isComplete: true);
        }
    }

    public UserProfileDTO UpdateCurrentProfile(string persona, UpdateUserProfileRequest request)
    {
        lock(_syncRoot)
        {
            return UpdateCurrentProfileCore(persona, request.Username, request.Firstname, request.Lastname, request.DiscordId, request.SteamId, request.RiotId, isComplete: true);
        }
    }

    public UsernameAvailabilityResponse CheckUsernameAvailability(string persona, string username)
    {
        lock(_syncRoot)
        {
            var normalized = username.Trim();
            var currentProfile = GetCurrentProfile(persona);
            var currentUserId = currentProfile.User?.Id;

            var existingUser = _document.Users.FirstOrDefault(user =>
                string.Equals(user.Username, normalized, StringComparison.OrdinalIgnoreCase) &&
                user.Id != currentUserId &&
                !user.IsDeleted);

            return new UsernameAvailabilityResponse
            {
                Username = username,
                NormalizedUsername = normalized,
                IsAvailable = existingUser == null,
                Reason = existingUser == null ? null : "Username is already in use."
            };
        }
    }

    public UserActionResponse DeleteCurrentUser(string persona)
    {
        lock(_syncRoot)
        {
            var normalizedPersona = NormalizePersona(persona);
            var profileRecord = _document.Profiles.First(candidate =>
                string.Equals(candidate.Persona, normalizedPersona, StringComparison.OrdinalIgnoreCase));

            if(profileRecord.Profile.User != null)
            {
                profileRecord.Profile.User.IsDeleted = true;
                var user = _document.Users.FirstOrDefault(existing => existing.Id == profileRecord.Profile.User.Id);
                if(user != null)
                    user.IsDeleted = true;
            }

            return new UserActionResponse("Mock account deleted.");
        }
    }

    public List<UserDTO> GetUsers()
    {
        lock(_syncRoot)
        {
            return Clone(_document.Users.Where(user => !user.IsDeleted).ToList())!;
        }
    }

    public UserDTO GetUserById(Guid id)
    {
        lock(_syncRoot)
        {
            return Clone(GetRequiredUser(id))!;
        }
    }

    public void DeleteUser(string username)
    {
        lock(_syncRoot)
        {
            var user = _document.Users.First(existing =>
                string.Equals(existing.Username, username, StringComparison.OrdinalIgnoreCase));
            user.IsDeleted = true;
        }
    }

    private static MockBackendDocument LoadDocument(string dataFilePath)
    {
        if(!File.Exists(dataFilePath))
        {
            throw new InvalidOperationException(
                $"Mock backend is enabled but the local fixture file was not found at '{dataFilePath}'.");
        }

        var json = File.ReadAllText(dataFilePath);
        var document = JsonSerializer.Deserialize<MockBackendDocument>(json, SerializerOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize mock backend fixture '{dataFilePath}'.");

        foreach(var user in document.Users)
        {
            user.DisplayName = string.IsNullOrWhiteSpace(user.DisplayName)
                ? BuildDisplayName(user.Firstname, user.Lastname, user.Username)
                : user.DisplayName;
        }

        foreach(var team in document.Teams)
        {
            foreach(var invite in team.TeamInvites)
                EnsureInviteDates(invite);

            var sourceTeam = document.Tournaments
                .SelectMany(tournament => tournament.Teams)
                .FirstOrDefault(candidate => candidate.Id == team.Id);

            if(sourceTeam != null)
            {
                team.Members = team.Members.Any() ? team.Members : Clone(sourceTeam.Members)!;
                team.TeamInvites = team.TeamInvites.Any() ? team.TeamInvites : Clone(sourceTeam.TeamInvites)!;
            }

            foreach(var invite in team.TeamInvites)
                EnsureInviteDates(invite);
        }

        foreach(var profile in document.Profiles.Where(profile => profile.Profile.User != null))
        {
            profile.Profile.User!.DisplayName = string.IsNullOrWhiteSpace(profile.Profile.User.DisplayName)
                ? BuildDisplayName(profile.Profile.User.Firstname, profile.Profile.User.Lastname, profile.Profile.User.Username)
                : profile.Profile.User.DisplayName;
        }

        foreach(var tournament in document.Tournaments)
        {
            EnsureScheduleFields(tournament);
            EnsureRegistrationProjection(tournament);
        }

        return document;
    }

    private static void EnsureScheduleFields(TournamentExtended tournament)
    {
        if(tournament.PlannedStartTime == default)
            tournament.PlannedStartTime = tournament.StartTime == default ? DateTime.UtcNow.AddDays(7) : tournament.StartTime;

        if(tournament.AverageGameDurationMinutes <= 0)
            tournament.AverageGameDurationMinutes = 30;

        if(tournament.RoundBreakDurationMinutes <= 0)
            tournament.RoundBreakDurationMinutes = 10;

        foreach(var match in tournament.Matches)
        {
            match.EstimatedStartTime ??= match.StartTime == default ? null : match.StartTime;
            match.EstimatedEndTime ??= match.EndTime == default ? null : match.EndTime;
        }

        tournament.EstimatedEndTime ??= tournament.Matches
            .Select(match => match.EstimatedEndTime)
            .Where(estimatedEnd => estimatedEnd.HasValue)
            .Max();
    }

    private void SeedFeaturedDoubleEliminationFixture()
    {
        var tournament = _document.Tournaments.FirstOrDefault(candidate => candidate.Id == FeaturedDoubleEliminationTournamentId);
        if(tournament == null)
            return;

        var teams = BuildFeaturedDoubleEliminationTeams();

        tournament.Name = "Valorant";
        tournament.StartTime = new DateTime(2026, 6, 14, 12, 0, 0, DateTimeKind.Utc);
        tournament.EndTime = new DateTime(2026, 6, 14, 23, 0, 0, DateTimeKind.Utc);
        tournament.PlannedStartTime = tournament.StartTime;
        tournament.AverageGameDurationMinutes = 30;
        tournament.RoundBreakDurationMinutes = 15;
        tournament.EstimatedEndTime = tournament.EndTime;
        tournament.Status = TournamentStatus.InProgress;
        tournament.BracketType = BracketType.DoubleElimination;
        tournament.Format = TournamentFormat.BestOf3;
        tournament.FinalsFormat = TournamentFormat.BestOf5;
        tournament.ParticipationMode = ParticipationMode.Team;
        tournament.TeamSize = 5;
        tournament.Placements = [];
        tournament.Users = [];
        tournament.Teams = Clone(teams)!;
        tournament.Matches = BuildFeaturedDoubleEliminationMatches(tournament.Id, teams);
        EnsureRegistrationProjection(tournament);

        foreach(var team in teams)
        {
            AddOrReplaceTeam(team);
        }
    }

    private static List<Team> BuildFeaturedDoubleEliminationTeams()
    {
        return
        [
            BuildFeaturedTeam("21111111-1111-1111-1111-111111111111", "Team Alpha", "41111111-1111-1111-1111-111111111111", "alpha1", "Alex", "Alder", "alex@example.test", "alpha#1111", "alpha#VAL"),
            BuildFeaturedTeam("21111111-1111-1111-1111-111111111112", "Binary Bandits", "41111111-1111-1111-1111-111111111113", "binary1", "Ben", "Binary", "ben@example.test", "binary#1111", "binary#VAL"),
            BuildFeaturedTeam("21111111-1111-1111-1111-111111111113", "Gamma Grid", "41111111-1111-1111-1111-111111111114", "gamma1", "Gina", "Grid", "gina@example.test", "gamma#1111", "gamma#VAL"),
            BuildFeaturedTeam("21111111-1111-1111-1111-111111111114", "Delta Drop", "41111111-1111-1111-1111-111111111115", "delta1", "Dana", "Drop", "dana@example.test", "delta#1111", "delta#VAL"),
            BuildFeaturedTeam("21111111-1111-1111-1111-111111111115", "Echo Unit", "41111111-1111-1111-1111-111111111116", "echo1", "Eli", "Echo", "eli@example.test", "echo#1111", "echo#VAL"),
            BuildFeaturedTeam("21111111-1111-1111-1111-111111111116", "Frame Perfect", "41111111-1111-1111-1111-111111111117", "frame1", "Finn", "Frame", "finn@example.test", "frame#1111", "frame#VAL"),
            BuildFeaturedTeam("22111111-1111-1111-1111-111111111121", "Pixel Pushers", "42111111-1111-1111-1111-111111111121", "pixel1", "Pia", "Pixel", "pia@example.test", "pixel#1111", "pixel#VAL"),
            BuildFeaturedTeam("22111111-1111-1111-1111-111111111122", "Quantum Queue", "42111111-1111-1111-1111-111111111122", "queue1", "Quinn", "Queue", "quinn@example.test", "queue#1111", "queue#VAL"),
            BuildFeaturedTeam("22111111-1111-1111-1111-111111111123", "Radiant Rift", "42111111-1111-1111-1111-111111111123", "radiant1", "Rhea", "Rift", "rhea@example.test", "rift#1111", "rift#VAL"),
            BuildFeaturedTeam("22111111-1111-1111-1111-111111111124", "Spike Syndicate", "42111111-1111-1111-1111-111111111124", "spike1", "Soren", "Spike", "soren@example.test", "spike#1111", "spike#VAL"),
            BuildFeaturedTeam("22111111-1111-1111-1111-111111111125", "Vector Vipers", "42111111-1111-1111-1111-111111111125", "vector1", "Vera", "Vector", "vera@example.test", "vector#1111", "vector#VAL"),
            BuildFeaturedTeam("22111111-1111-1111-1111-111111111126", "Neon Knights", "42111111-1111-1111-1111-111111111126", "neon1", "Nia", "Neon", "nia@example.test", "neon#1111", "neon#VAL"),
            BuildFeaturedTeam("22111111-1111-1111-1111-111111111127", "Orbital Ops", "42111111-1111-1111-1111-111111111127", "orbital1", "Owen", "Orbital", "owen@example.test", "orbital#1111", "orbital#VAL"),
            BuildFeaturedTeam("22111111-1111-1111-1111-111111111128", "Prism Protocol", "42111111-1111-1111-1111-111111111128", "prism1", "Priya", "Prism", "priya@example.test", "prism#1111", "prism#VAL"),
            BuildFeaturedTeam("22111111-1111-1111-1111-111111111129", "Haven Hackers", "42111111-1111-1111-1111-111111111129", "haven1", "Harper", "Haven", "harper@example.test", "haven#1111", "haven#VAL"),
            BuildFeaturedTeam("22111111-1111-1111-1111-111111111130", "Mid Control", "42111111-1111-1111-1111-111111111130", "mid1", "Milo", "Control", "milo@example.test", "mid#1111", "mid#VAL")
        ];
    }

    private static Team BuildFeaturedTeam(
        string teamId,
        string teamName,
        string captainUserId,
        string username,
        string firstname,
        string lastname,
        string email,
        string discordId,
        string riotId)
    {
        var captain = new PublicUserDTO
        {
            Id = Guid.Parse(captainUserId),
            Username = username,
            Firstname = firstname,
            Lastname = lastname,
            DiscordId = discordId,
            SteamId = $"steam-{username}",
            RiotId = riotId,
            DisplayName = username
        };

        var teammateUsername = username.EndsWith("1", StringComparison.Ordinal)
            ? $"{username[..^1]}2"
            : $"{username}2";
        var teammateIsUsernameOnly = string.Equals(teamName, "Team Alpha", StringComparison.Ordinal);
        var teammateFirstName = teammateIsUsernameOnly ? null : $"{firstname} Mate";
        var teammate = new PublicUserDTO
        {
            Id = Guid.Parse(captainUserId.Replace("-1111-1111-1111-", "-2222-2222-2222-", StringComparison.Ordinal)),
            Username = teammateUsername,
            Firstname = teammateFirstName,
            Lastname = teammateIsUsernameOnly ? null : lastname,
            DiscordId = teammateIsUsernameOnly ? null : discordId.Replace("#", "2#", StringComparison.Ordinal),
            SteamId = teammateIsUsernameOnly ? null : $"steam-{teammateUsername}",
            RiotId = teammateIsUsernameOnly ? null : riotId.Replace("#", "2#", StringComparison.Ordinal),
            DisplayName = teammateUsername
        };

        return new Team
        {
            Id = Guid.Parse(teamId),
            Name = teamName,
            CaptainUserId = captain.Id,
            Members = [captain, teammate],
            TeamInvites = []
        };
    }

    private static List<Match> BuildFeaturedDoubleEliminationMatches(Guid tournamentId, IReadOnlyList<Team> teams)
    {
        var teamIds = teams.ToDictionary(team => team.Name, team => team.Id);
        var startTime = new DateTime(2026, 6, 14, 12, 0, 0, DateTimeKind.Utc);

        const string ubRound1Match1Id = "31111111-1111-1111-1111-111111111001";
        const string ubRound1Match2Id = "31111111-1111-1111-1111-111111111002";
        const string ubRound1Match3Id = "31111111-1111-1111-1111-111111111003";
        const string ubRound1Match4Id = "31111111-1111-1111-1111-111111111004";
        const string ubRound1Match5Id = "31111111-1111-1111-1111-111111111005";
        const string ubRound1Match6Id = "31111111-1111-1111-1111-111111111006";
        const string ubRound1Match7Id = "31111111-1111-1111-1111-111111111007";
        const string ubRound1Match8Id = "31111111-1111-1111-1111-111111111008";
        const string ubRound2Match1Id = "31111111-1111-1111-1111-111111111009";
        const string ubRound2Match2Id = "31111111-1111-1111-1111-111111111010";
        const string ubRound2Match3Id = "31111111-1111-1111-1111-111111111011";
        const string ubRound2Match4Id = "31111111-1111-1111-1111-111111111012";
        const string ubRound3Match1Id = "31111111-1111-1111-1111-111111111013";
        const string ubRound3Match2Id = "31111111-1111-1111-1111-111111111014";
        const string ubFinalMatchId = "31111111-1111-1111-1111-111111111015";
        const string lbRound1Match1Id = "31111111-1111-1111-1111-111111111101";
        const string lbRound1Match2Id = "31111111-1111-1111-1111-111111111102";
        const string lbRound1Match3Id = "31111111-1111-1111-1111-111111111103";
        const string lbRound1Match4Id = "31111111-1111-1111-1111-111111111104";
        const string lbRound2Match1Id = "31111111-1111-1111-1111-111111111105";
        const string lbRound2Match2Id = "31111111-1111-1111-1111-111111111106";
        const string lbRound2Match3Id = "31111111-1111-1111-1111-111111111107";
        const string lbRound2Match4Id = "31111111-1111-1111-1111-111111111108";
        const string lbRound3Match1Id = "31111111-1111-1111-1111-111111111109";
        const string lbRound3Match2Id = "31111111-1111-1111-1111-111111111110";
        const string lbRound4Match1Id = "31111111-1111-1111-1111-111111111111";
        const string lbRound4Match2Id = "31111111-1111-1111-1111-111111111112";
        const string lbRound5MatchId = "31111111-1111-1111-1111-111111111113";
        const string lbRound6MatchId = "31111111-1111-1111-1111-111111111114";
        const string grandFinalMatchId = "31111111-1111-1111-1111-111111111115";

        return
        [
            BuildFeaturedMatch(ubRound1Match1Id, tournamentId, startTime, 1, 1, false, teamIds["Team Alpha"], teamIds["Mid Control"], 2, 0, teamIds["Team Alpha"], teamIds["Mid Control"], ubRound2Match1Id, lbRound1Match1Id),
            BuildFeaturedMatch(ubRound1Match2Id, tournamentId, startTime, 1, 2, false, teamIds["Quantum Queue"], teamIds["Orbital Ops"], 2, 1, teamIds["Quantum Queue"], teamIds["Orbital Ops"], ubRound2Match1Id, lbRound1Match1Id),
            BuildFeaturedMatch(ubRound1Match3Id, tournamentId, startTime, 1, 3, false, teamIds["Echo Unit"], teamIds["Neon Knights"], 2, 0, teamIds["Echo Unit"], teamIds["Neon Knights"], ubRound2Match2Id, lbRound1Match2Id),
            BuildFeaturedMatch(ubRound1Match4Id, tournamentId, startTime, 1, 4, false, teamIds["Delta Drop"], teamIds["Vector Vipers"], 2, 1, teamIds["Delta Drop"], teamIds["Vector Vipers"], ubRound2Match2Id, lbRound1Match2Id),
            BuildFeaturedMatch(ubRound1Match5Id, tournamentId, startTime, 1, 5, false, teamIds["Binary Bandits"], teamIds["Haven Hackers"], 2, 0, teamIds["Binary Bandits"], teamIds["Haven Hackers"], ubRound2Match3Id, lbRound1Match3Id),
            BuildFeaturedMatch(ubRound1Match6Id, tournamentId, startTime, 1, 6, false, teamIds["Radiant Rift"], teamIds["Prism Protocol"], 2, 1, teamIds["Radiant Rift"], teamIds["Prism Protocol"], ubRound2Match3Id, lbRound1Match3Id),
            BuildFeaturedMatch(ubRound1Match7Id, tournamentId, startTime, 1, 7, false, teamIds["Frame Perfect"], teamIds["Spike Syndicate"], 1, 2, teamIds["Spike Syndicate"], teamIds["Frame Perfect"], ubRound2Match4Id, lbRound1Match4Id),
            BuildFeaturedMatch(ubRound1Match8Id, tournamentId, startTime, 1, 8, false, teamIds["Gamma Grid"], teamIds["Pixel Pushers"], 2, 0, teamIds["Gamma Grid"], teamIds["Pixel Pushers"], ubRound2Match4Id, lbRound1Match4Id),

            BuildFeaturedMatch(lbRound1Match1Id, tournamentId, startTime.AddMinutes(90), 1, 1, true, teamIds["Mid Control"], teamIds["Orbital Ops"], 0, 2, teamIds["Orbital Ops"], teamIds["Mid Control"], lbRound2Match1Id, null),
            BuildFeaturedMatch(lbRound1Match2Id, tournamentId, startTime.AddMinutes(90), 1, 2, true, teamIds["Neon Knights"], teamIds["Vector Vipers"], 1, 2, teamIds["Vector Vipers"], teamIds["Neon Knights"], lbRound2Match2Id, null),
            BuildFeaturedMatch(lbRound1Match3Id, tournamentId, startTime.AddMinutes(90), 1, 3, true, teamIds["Haven Hackers"], teamIds["Prism Protocol"], 1, 2, teamIds["Prism Protocol"], teamIds["Haven Hackers"], lbRound2Match3Id, null),
            BuildFeaturedMatch(lbRound1Match4Id, tournamentId, startTime.AddMinutes(90), 1, 4, true, teamIds["Frame Perfect"], teamIds["Pixel Pushers"], 2, 0, teamIds["Frame Perfect"], teamIds["Pixel Pushers"], lbRound2Match4Id, null),

            BuildFeaturedMatch(ubRound2Match1Id, tournamentId, startTime.AddMinutes(180), 2, 1, false, teamIds["Team Alpha"], teamIds["Quantum Queue"], 2, 0, teamIds["Team Alpha"], teamIds["Quantum Queue"], ubRound3Match1Id, lbRound2Match1Id),
            BuildFeaturedMatch(ubRound2Match2Id, tournamentId, startTime.AddMinutes(180), 2, 2, false, teamIds["Echo Unit"], teamIds["Delta Drop"], 1, 2, teamIds["Delta Drop"], teamIds["Echo Unit"], ubRound3Match1Id, lbRound2Match2Id),
            BuildFeaturedMatch(ubRound2Match3Id, tournamentId, startTime.AddMinutes(180), 2, 3, false, teamIds["Binary Bandits"], teamIds["Radiant Rift"], 2, 1, teamIds["Binary Bandits"], teamIds["Radiant Rift"], ubRound3Match2Id, lbRound2Match3Id),
            BuildFeaturedMatch(ubRound2Match4Id, tournamentId, startTime.AddMinutes(180), 2, 4, false, teamIds["Spike Syndicate"], teamIds["Gamma Grid"], 0, 2, teamIds["Gamma Grid"], teamIds["Spike Syndicate"], ubRound3Match2Id, lbRound2Match4Id),

            BuildFeaturedMatch(lbRound2Match1Id, tournamentId, startTime.AddMinutes(270), 2, 1, true, teamIds["Orbital Ops"], teamIds["Quantum Queue"], 0, 2, teamIds["Quantum Queue"], teamIds["Orbital Ops"], lbRound3Match1Id, null),
            BuildFeaturedMatch(lbRound2Match2Id, tournamentId, startTime.AddMinutes(270), 2, 2, true, teamIds["Vector Vipers"], teamIds["Echo Unit"], 0, 2, teamIds["Echo Unit"], teamIds["Vector Vipers"], lbRound3Match1Id, null),
            BuildFeaturedMatch(lbRound2Match3Id, tournamentId, startTime.AddMinutes(270), 2, 3, true, teamIds["Prism Protocol"], teamIds["Radiant Rift"], 1, 2, teamIds["Radiant Rift"], teamIds["Prism Protocol"], lbRound3Match2Id, null),
            BuildFeaturedMatch(lbRound2Match4Id, tournamentId, startTime.AddMinutes(270), 2, 4, true, teamIds["Frame Perfect"], teamIds["Spike Syndicate"], 1, 2, teamIds["Spike Syndicate"], teamIds["Frame Perfect"], lbRound3Match2Id, null),

            BuildFeaturedMatch(ubRound3Match1Id, tournamentId, startTime.AddMinutes(360), 3, 1, false, teamIds["Team Alpha"], teamIds["Delta Drop"], 2, 1, teamIds["Team Alpha"], teamIds["Delta Drop"], ubFinalMatchId, lbRound4Match1Id),
            BuildFeaturedMatch(ubRound3Match2Id, tournamentId, startTime.AddMinutes(360), 3, 2, false, teamIds["Binary Bandits"], teamIds["Gamma Grid"], 2, 1, teamIds["Binary Bandits"], teamIds["Gamma Grid"], ubFinalMatchId, lbRound4Match2Id),

            BuildFeaturedMatch(lbRound3Match1Id, tournamentId, startTime.AddMinutes(450), 3, 1, true, teamIds["Quantum Queue"], teamIds["Echo Unit"], 1, 2, teamIds["Echo Unit"], teamIds["Quantum Queue"], lbRound4Match1Id, null),
            BuildFeaturedMatch(lbRound3Match2Id, tournamentId, startTime.AddMinutes(450), 3, 2, true, teamIds["Radiant Rift"], teamIds["Spike Syndicate"], 1, 2, teamIds["Spike Syndicate"], teamIds["Radiant Rift"], lbRound4Match2Id, null),

            BuildFeaturedMatch(lbRound4Match1Id, tournamentId, startTime.AddMinutes(540), 4, 1, true, teamIds["Echo Unit"], teamIds["Delta Drop"], 0, 2, teamIds["Delta Drop"], teamIds["Echo Unit"], lbRound5MatchId, null),
            BuildFeaturedMatch(lbRound4Match2Id, tournamentId, startTime.AddMinutes(540), 4, 2, true, teamIds["Spike Syndicate"], teamIds["Gamma Grid"], 0, 2, teamIds["Gamma Grid"], teamIds["Spike Syndicate"], lbRound5MatchId, null),
            BuildFeaturedMatch(ubFinalMatchId, tournamentId, startTime.AddMinutes(540), 4, 3, false, teamIds["Team Alpha"], teamIds["Binary Bandits"], 3, 1, teamIds["Team Alpha"], teamIds["Binary Bandits"], grandFinalMatchId, lbRound6MatchId, TournamentFormat.BestOf5),

            BuildFeaturedMatch(lbRound5MatchId, tournamentId, startTime.AddMinutes(630), 5, 1, true, teamIds["Delta Drop"], teamIds["Gamma Grid"], 1, 3, teamIds["Gamma Grid"], teamIds["Delta Drop"], lbRound6MatchId, null, TournamentFormat.BestOf5),
            BuildFeaturedMatch(lbRound6MatchId, tournamentId, startTime.AddMinutes(720), 6, 1, true, teamIds["Gamma Grid"], teamIds["Binary Bandits"], 3, 2, teamIds["Gamma Grid"], teamIds["Binary Bandits"], grandFinalMatchId, null, TournamentFormat.BestOf5),
            BuildFeaturedMatch(grandFinalMatchId, tournamentId, startTime.AddMinutes(810), 7, 1, false, teamIds["Team Alpha"], teamIds["Gamma Grid"], null, null, null, null, null, null, TournamentFormat.BestOf5)
        ];
    }

    private static Match BuildFeaturedMatch(
        string matchId,
        Guid tournamentId,
        DateTime startTime,
        int roundNumber,
        int matchNumber,
        bool isLowerBracketMatch,
        Guid? participant1Id,
        Guid? participant2Id,
        int? participant1Score,
        int? participant2Score,
        Guid? teamWinnerId,
        Guid? teamLoserId,
        string? winnerNextMatchId,
        string? loserNextMatchId,
        TournamentFormat? format = null)
    {
        return new Match
        {
            Id = Guid.Parse(matchId),
            StartTime = startTime,
            EndTime = startTime.AddMinutes(format == TournamentFormat.BestOf5 ? 75 : 60),
            EstimatedStartTime = startTime,
            EstimatedEndTime = startTime.AddMinutes(format == TournamentFormat.BestOf5 ? 75 : 60),
            BracketType = BracketType.DoubleElimination,
            Format = format ?? TournamentFormat.BestOf3,
            ParticipationMode = ParticipationMode.Team,
            RoundNumber = roundNumber,
            MatchNumber = matchNumber,
            IsLowerBracketMatch = isLowerBracketMatch,
            TournamentId = tournamentId,
            TeamParticipant1Id = participant1Id,
            TeamParticipant2Id = participant2Id,
            Participant1IsBYE = false,
            Participant2IsBYE = false,
            TeamWinnerId = teamWinnerId,
            TeamLoserId = teamLoserId,
            Participant1Score = participant1Score,
            Participant2Score = participant2Score,
            WinnerNextMatchId = string.IsNullOrWhiteSpace(winnerNextMatchId) ? null : Guid.Parse(winnerNextMatchId),
            LoserNextMatchId = string.IsNullOrWhiteSpace(loserNextMatchId) ? null : Guid.Parse(loserNextMatchId)
        };
    }

    private static List<Placement> BuildPlacements(TournamentExtended tournament)
    {
        if(tournament.ParticipationMode == ParticipationMode.Team)
        {
            return tournament.Teams.Take(4).Select((team, index) => new Placement
            {
                Place = index + 1,
                Teams = [Clone(team)!]
            }).ToList();
        }

        return tournament.Users.Take(4).Select((user, index) => new Placement
        {
            Place = index + 1,
            Users = [Clone(user)!]
        }).ToList();
    }

    private static void EnsureRegistrationProjection(TournamentExtended tournament)
    {
        if(tournament.Registrations.Any())
            return;

        var registrations = tournament.Users
            .Where(user => user.Id != Guid.Empty)
            .Select(user => new PublicTournamentRegistrationDTO
            {
                Id = user.Id,
                TournamentId = tournament.Id,
                Kind = TournamentRegistrationKind.Individual,
                Status = TournamentRegistrationStatus.Active,
                User = Clone(user)
            })
            .Concat(tournament.Teams
                .Where(team => team.Id != Guid.Empty)
                .Select(team => new PublicTournamentRegistrationDTO
                {
                    Id = team.Id,
                    TournamentId = tournament.Id,
                    Kind = TournamentRegistrationKind.Team,
                    Status = TournamentRegistrationStatus.Active,
                    Team = new PublicTournamentTeamDTO
                    {
                        Id = team.Id,
                        Name = team.Name,
                        CaptainUserId = team.CaptainUserId,
                        LogoUrl = team.LogoUrl,
                        Members = []
                    },
                    RosterMembers = team.Members
                        .Select(member => new PublicTournamentRosterMemberDTO
                        {
                            User = Clone(member)!,
                            IsCaptain = member.Id == team.CaptainUserId
                        })
                        .ToList()
                }))
            .ToList();

        tournament.Registrations = registrations;
    }

    private UserProfileDTO UpdateCurrentProfileCore(
        string persona,
        string username,
        string firstname,
        string lastname,
        string? discordId,
        string? steamId,
        string? riotId,
        bool isComplete)
    {
        var normalizedPersona = NormalizePersona(persona);
        var profileRecord = _document.Profiles.First(candidate =>
            string.Equals(candidate.Persona, normalizedPersona, StringComparison.OrdinalIgnoreCase));

        var existingUser = profileRecord.Profile.User
            ?? throw new InvalidOperationException($"Mock profile for persona '{normalizedPersona}' is missing its user.");

        existingUser.Username = username.Trim();
        existingUser.Firstname = firstname.Trim();
        existingUser.Lastname = lastname.Trim();
        existingUser.DiscordId = discordId;
        existingUser.SteamId = steamId;
        existingUser.RiotId = riotId;
        existingUser.DisplayName = BuildDisplayName(existingUser.Firstname, existingUser.Lastname, existingUser.Username);
        existingUser.UpdatedAtUtc = DateTime.UtcNow;

        var userIndex = _document.Users.FindIndex(user => user.Id == existingUser.Id);
        if(userIndex >= 0)
        {
            _document.Users[userIndex] = ToUserDto(existingUser);
        }

        profileRecord.Profile = new CurrentUserProfileResponse(
            isComplete,
            Clone(existingUser),
            profileRecord.Profile.Email,
            profileRecord.Profile.EmailVerified);

        return Clone(existingUser)!;
    }

    private TournamentExtended GetRequiredTournament(Guid id) =>
        _document.Tournaments.FirstOrDefault(tournament => tournament.Id == id)
        ?? throw new InvalidOperationException($"Mock tournament '{id}' was not found.");

    private UserDTO GetRequiredUser(Guid id) =>
        _document.Users.FirstOrDefault(user => user.Id == id)
        ?? throw new InvalidOperationException($"Mock user '{id}' was not found.");

    private Team GetRequiredTeam(Guid id)
    {
        var team = _document.Teams.FirstOrDefault(existing => existing.Id == id);
        return team ?? throw new InvalidOperationException($"Mock team '{id}' was not found.");
    }

    private void AddOrReplaceTeam(Team team)
    {
        var storeIndex = _document.Teams.FindIndex(existing => existing.Id == team.Id);
        if(storeIndex >= 0)
        {
            _document.Teams[storeIndex] = Clone(team)!;
        }
        else
        {
            _document.Teams.Add(Clone(team)!);
        }

        foreach(var member in team.Members)
        {
            UpsertUserPublicFields(member);
        }

        foreach(var tournament in _document.Tournaments)
        {
            var teams = tournament.Teams.ToList();
            var index = teams.FindIndex(existing => existing.Id == team.Id);
            if(index >= 0)
            {
                teams[index] = Clone(team)!;
                tournament.Teams = teams;
            }
        }
    }

    private void UpsertUserPublicFields(PublicUserDTO member)
    {
        var userIndex = _document.Users.FindIndex(existing => existing.Id == member.Id);
        if(userIndex >= 0)
        {
            var existingUser = _document.Users[userIndex];
            existingUser.Username = member.Username;
            existingUser.Firstname = member.Firstname ?? existingUser.Firstname;
            existingUser.Lastname = member.Lastname ?? existingUser.Lastname;
            existingUser.DiscordId = member.DiscordId;
            existingUser.SteamId = member.SteamId;
            existingUser.RiotId = member.RiotId;
            existingUser.DisplayName = string.IsNullOrWhiteSpace(member.DisplayName)
                ? BuildDisplayName(existingUser.Firstname, existingUser.Lastname, existingUser.Username)
                : member.DisplayName;
            existingUser.UpdatedAtUtc = FeaturedFixtureUpdatedAtUtc;
            return;
        }

        _document.Users.Add(new UserDTO
        {
            Id = member.Id,
            Username = member.Username,
            Firstname = member.Firstname,
            Lastname = member.Lastname,
            Email = string.Empty,
            EmailVerified = false,
            DiscordId = member.DiscordId,
            SteamId = member.SteamId,
            RiotId = member.RiotId,
            DisplayName = string.IsNullOrWhiteSpace(member.DisplayName)
                ? BuildDisplayName(member.Firstname, member.Lastname, member.Username)
                : member.DisplayName,
            IsDeleted = false,
            CreatedAtUtc = FeaturedFixtureCreatedAtUtc,
            UpdatedAtUtc = FeaturedFixtureUpdatedAtUtc
        });
    }

    private List<Team> EnsureCurrentUserTeamFixtures(UserProfileDTO currentUser)
    {
        var captainedId = Guid.Parse("23111111-1111-1111-1111-111111111120");
        var memberId = Guid.Parse("23111111-1111-1111-1111-111111111121");
        var inviteId = Guid.Parse("23111111-1111-1111-1111-111111111122");

        if(_document.Teams.All(team => team.Id != captainedId))
        {
            var teammate = _document.Users.First(user => user.Username == "track1");
            AddOrReplaceTeam(new Team
            {
                Id = captainedId,
                Name = "Mock Captains",
                CaptainUserId = currentUser.Id,
                LogoUrl = "/mock-data-local/sponsors/mock-sponsor.svg",
                Members = [ToPublicUser(currentUser), PublicUserDTO.FromUser(teammate)],
                TeamInvites =
                [
                    new TeamInvite
                    {
                        Id = Guid.Parse("24111111-1111-1111-1111-111111111120"),
                        TeamId = captainedId,
                        UserId = Guid.Parse("41111111-1111-1111-1111-111111111119"),
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    }
                ]
            });
        }

        if(_document.Teams.All(team => team.Id != memberId))
        {
            var captain = _document.Users.First(user => user.Username == "binary1");
            AddOrReplaceTeam(new Team
            {
                Id = memberId,
                Name = "Roster Lock",
                CaptainUserId = captain.Id,
                Members = [PublicUserDTO.FromUser(captain), ToPublicUser(currentUser)],
                TeamInvites = []
            });
        }

        if(_document.Teams.All(team => team.Id != inviteId))
        {
            var captain = _document.Users.First(user => user.Username == "gamma1");
            AddOrReplaceTeam(new Team
            {
                Id = inviteId,
                Name = "Pending Pixels",
                CaptainUserId = captain.Id,
                Members = [PublicUserDTO.FromUser(captain)],
                TeamInvites =
                [
                    new TeamInvite
                    {
                        Id = Guid.Parse("24111111-1111-1111-1111-111111111121"),
                        TeamId = inviteId,
                        UserId = currentUser.Id,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow.AddHours(-8)
                    }
                ]
            });
        }

        return _document.Teams;
    }

    private TeamManagementSummaryDTO ToManagementSummary(Team team)
    {
        var captainUsername = team.Members.FirstOrDefault(member => member.Id == team.CaptainUserId)?.Username
            ?? _document.Users.FirstOrDefault(user => user.Id == team.CaptainUserId)?.Username;

        return new TeamManagementSummaryDTO
        {
            Id = team.Id,
            Name = team.Name,
            CaptainUserId = team.CaptainUserId,
            CaptainUsername = captainUsername,
            LogoUrl = team.LogoUrl,
            Members = team.Members.ToList()
        };
    }

    private TeamInviteSummaryDTO ToInviteSummary(Team team, TeamInvite invite)
    {
        EnsureInviteDates(invite);
        var user = _document.Users.FirstOrDefault(candidate => candidate.Id == invite.UserId);

        return new TeamInviteSummaryDTO
        {
            Id = invite.Id,
            TeamId = team.Id,
            TeamName = team.Name,
            TeamLogoUrl = team.LogoUrl,
            UserId = invite.UserId,
            Username = user?.Username,
            Status = invite.Status,
            CreatedAt = invite.CreatedAt,
            ExpiresAt = invite.ExpiresAt!.Value
        };
    }

    private static void EnsureInviteDates(TeamInvite invite)
    {
        if(invite.CreatedAt == default)
            invite.CreatedAt = DateTime.UtcNow;

        invite.ExpiresAt ??= invite.CreatedAt.AddDays(7);
    }

    private static bool IsPending(TeamInvite invite)
    {
        return string.Equals(invite.Status, "Pending", StringComparison.OrdinalIgnoreCase);
    }

    private static Tournament ToTournament(TournamentExtended tournament)
    {
        return new Tournament
        {
            Id = tournament.Id,
            Name = tournament.Name,
            StartTime = tournament.StartTime,
            EndTime = tournament.EndTime,
            PlannedStartTime = tournament.PlannedStartTime,
            AverageGameDurationMinutes = tournament.AverageGameDurationMinutes,
            RoundBreakDurationMinutes = tournament.RoundBreakDurationMinutes,
            EstimatedEndTime = tournament.EstimatedEndTime,
            ImageUrl = tournament.ImageUrl,
            Status = tournament.Status,
            BracketType = tournament.BracketType,
            Format = tournament.Format,
            FinalsFormat = tournament.FinalsFormat,
            ParticipationMode = tournament.ParticipationMode,
            TeamSize = tournament.TeamSize
        };
    }

    private static string NormalizePersona(string? persona)
    {
        if(string.IsNullOrWhiteSpace(persona))
            return "user";

        return persona.Trim().ToLowerInvariant() switch
        {
            "admin" => "admin",
            "anonymous" => "anonymous",
            _ => "user"
        };
    }

    private static string BuildDisplayName(string? firstname, string? lastname, string? username)
    {
        var fullName = string.Join(" ", new[] { firstname, lastname }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim()));

        return string.IsNullOrWhiteSpace(fullName)
            ? username ?? "Mock User"
            : fullName;
    }

    private static PublicUserDTO ToPublicUser(UserProfileDTO user)
    {
        return PublicUserDTO.FromUser(user);
    }

    private static PublicUserDTO ToPublicUser(UserDTO user)
    {
        return PublicUserDTO.FromUser(user);
    }

    private static UserDTO ToUserDto(UserProfileDTO user)
    {
        return new UserDTO
        {
            Id = user.Id,
            Username = user.Username,
            Firstname = user.Firstname,
            Lastname = user.Lastname,
            Email = user.Email,
            EmailVerified = user.EmailVerified,
            DiscordId = user.DiscordId,
            SteamId = user.SteamId,
            RiotId = user.RiotId,
            DisplayName = user.DisplayName,
            IsDeleted = user.IsDeleted,
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc
        };
    }

    private static T? Clone<T>(T? value)
    {
        if(value == null)
            return default;

        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, SerializerOptions), SerializerOptions);
    }
}
