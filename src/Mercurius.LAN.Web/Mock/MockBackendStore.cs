using System.Text.Json;
using System.Text.Json.Serialization;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.DTOs.Search;
using Mercurius.LAN.Web.DTOs.Sponsors;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Games;
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
    private static readonly Guid FeaturedDoubleEliminationGameId = Guid.Parse("11111111-1111-1111-1111-111111111112");
    private static readonly DateTime FeaturedFixtureCreatedAtUtc = new(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FeaturedFixtureUpdatedAtUtc = new(2026, 5, 11, 12, 0, 0, DateTimeKind.Utc);

    private readonly object _syncRoot = new();
    private readonly string _dataFilePath;
    private MockBackendDocument _document;

    public MockBackendStore(IHostEnvironment environment, IOptions<MockBackendOptions> options)
    {
        _dataFilePath = Path.Combine(environment.ContentRootPath, options.Value.DataFilePath);
        _document = LoadDocument(_dataFilePath);
        SeedFeaturedDoubleEliminationFixture();
    }

    public List<Game> GetGames()
    {
        lock(_syncRoot)
        {
            return Clone(_document.Games.Select(ToGame).ToList())!;
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
                    SupportingText = "Player",
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

            var gameResults = _document.Games
                .Where(game =>
                    !string.IsNullOrWhiteSpace(game.Name) &&
                    game.Name.Contains(trimmedQuery, StringComparison.OrdinalIgnoreCase))
                .OrderBy(game => game.Name.StartsWith(trimmedQuery, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(game => game.Name, StringComparer.OrdinalIgnoreCase)
                .Select(game => new GlobalSearchResultDTO
                {
                    Type = GlobalSearchResultType.Game,
                    DisplayLabel = game.Name,
                    SupportingText = "Tournament",
                    GameId = game.Id
                });

            return userResults
                .Concat(teamResults)
                .Concat(gameResults)
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

            var tournaments = _document.Games
                .Where(game => game.Teams.Any(candidate => candidate.Id == team.Id))
                .OrderBy(game => game.StartTime)
                .ThenBy(game => game.Name)
                .Select(game => new PublicTeamTournamentDTO
                {
                    GameId = game.Id,
                    Name = game.Name
                })
                .ToList();

            return new PublicTeamProfileDTO
            {
                TeamName = team.Name,
                CaptainUsername = captainUsername,
                Members = members,
                Tournaments = tournaments
            };
        }
    }

    public GameExtended? GetGame(Guid id)
    {
        lock(_syncRoot)
        {
            return Clone(_document.Games.FirstOrDefault(game => game.Id == id));
        }
    }

    public GameExtended CreateGame(CreateGameDTO dto)
    {
        lock(_syncRoot)
        {
            var game = new GameExtended
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                StartTime = dto.PlannedStartTime,
                EndTime = dto.PlannedStartTime.AddHours(4),
                PlannedStartTime = dto.PlannedStartTime,
                AverageGameDurationMinutes = dto.AverageGameDurationMinutes,
                RoundBreakDurationMinutes = dto.RoundBreakDurationMinutes,
                EstimatedEndTime = null,
                ImageUrl = "/mock-data-local/generated-game.svg",
                Status = GameStatus.Scheduled,
                BracketType = dto.BracketType,
                Format = dto.Format,
                FinalsFormat = dto.FinalsFormat,
                ParticipationMode = dto.ParticipationMode,
                RegisterFormUrl = dto.RegisterFormUrl
            };

            _document.Games.Add(game);
            return Clone(game)!;
        }
    }

    public GameExtended UpdateGame(Guid id, UpdateGameDTO dto)
    {
        lock(_syncRoot)
        {
            var game = GetRequiredGame(id);
            game.Name = dto.Name;
            game.Format = dto.Format;
            game.FinalsFormat = dto.FinalsFormat;
            game.BracketType = dto.BracketType;
            game.ParticipationMode = dto.ParticipationMode;
            game.RegisterFormUrl = dto.RegisterFormUrl;
            game.PlannedStartTime = dto.PlannedStartTime;
            game.AverageGameDurationMinutes = dto.AverageGameDurationMinutes;
            game.RoundBreakDurationMinutes = dto.RoundBreakDurationMinutes;
            game.EstimatedEndTime = game.Matches.Any() ? game.Matches.Max(match => match.EstimatedEndTime) : null;

            if(dto.Image != null)
                game.ImageUrl = "/mock-data-local/generated-game.svg";

            return Clone(game)!;
        }
    }

    public GameExtended ReplaceGameSponsors(Guid id, ReplaceGameSponsorsDTO dto)
    {
        lock(_syncRoot)
        {
            var game = GetRequiredGame(id);
            var placements = (dto.SponsorPlacements ?? []).Take(1).ToList();

            game.SponsorPlacements = placements
                .Select((placement, index) =>
                {
                    var sponsor = _document.Sponsors.Single(existing => existing.Id == placement.SponsorId);
                    return new GameSponsorPlacement
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
                .ToList();

            return Clone(game)!;
        }
    }

    public GameExtended RegisterUser(Guid gameId, Guid userId)
    {
        lock(_syncRoot)
        {
            var game = GetRequiredGame(gameId);
            var user = GetRequiredUser(userId);

            if(game.Users.All(existing => existing.Id != userId))
                game.Users = game.Users.Append(PublicUserDTO.FromUser(user)).ToList();

            return Clone(game)!;
        }
    }

    public GameExtended UnregisterUser(Guid gameId, Guid userId)
    {
        lock(_syncRoot)
        {
            var game = GetRequiredGame(gameId);
            game.Users = game.Users.Where(user => user.Id != userId).ToList();
            return Clone(game)!;
        }
    }

    public GameExtended RegisterTeam(Guid gameId, Guid teamId)
    {
        lock(_syncRoot)
        {
            var game = GetRequiredGame(gameId);
            var team = GetRequiredTeam(teamId);

            if(game.Teams.All(existing => existing.Id != teamId))
                game.Teams = game.Teams.Append(Clone(team)!).ToList();

            return Clone(game)!;
        }
    }

    public GameExtended UnregisterTeam(Guid gameId, Guid teamId)
    {
        lock(_syncRoot)
        {
            var game = GetRequiredGame(gameId);
            game.Teams = game.Teams.Where(team => team.Id != teamId).ToList();
            return Clone(game)!;
        }
    }

    public void StartGame(Guid gameId)
    {
        lock(_syncRoot)
        {
            GetRequiredGame(gameId).Status = GameStatus.InProgress;
        }
    }

    public void CancelGame(Guid gameId)
    {
        lock(_syncRoot)
        {
            GetRequiredGame(gameId).Status = GameStatus.Canceled;
        }
    }

    public void ResetGame(Guid gameId)
    {
        lock(_syncRoot)
        {
            var game = GetRequiredGame(gameId);
            game.Status = GameStatus.Scheduled;
            game.Placements = [];

            foreach(var match in game.Matches)
            {
                match.Participant1Score = null;
                match.Participant2Score = null;
                match.UserWinnerId = null;
                match.UserLoserId = null;
                match.TeamWinnerId = null;
                match.TeamLoserId = null;
            }
        }
    }

    public void DeleteGame(Guid gameId)
    {
        lock(_syncRoot)
        {
            _document.Games.RemoveAll(game => game.Id == gameId);
        }
    }

    public void CompleteGame(Guid gameId)
    {
        lock(_syncRoot)
        {
            var game = GetRequiredGame(gameId);
            game.Status = GameStatus.Completed;

            if(game.Placements.Any())
                return;

            game.Placements = BuildPlacements(game);
        }
    }

    public Match UpdateMatch(Guid matchId, UpdateMatchDTO dto)
    {
        lock(_syncRoot)
        {
            var game = _document.Games.FirstOrDefault(candidate => candidate.Matches.Any(match => match.Id == matchId))
                ?? throw new InvalidOperationException($"Mock match '{matchId}' was not found.");

            var match = game.Matches.First(existing => existing.Id == matchId);
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

    public List<Team> GetTeams()
    {
        lock(_syncRoot)
        {
            return Clone(_document.Teams)!;
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

            foreach(var game in _document.Games)
            {
                game.Teams = game.Teams.Where(team => team.Id != id).ToList();
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

            foreach(var game in _document.Games)
            {
                game.SponsorPlacements = game.SponsorPlacements
                    .Where(placement => placement.SponsorId != id)
                    .ToList();
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
            var sourceTeam = document.Games
                .SelectMany(game => game.Teams)
                .FirstOrDefault(candidate => candidate.Id == team.Id);

            if(sourceTeam != null)
            {
                team.Members = team.Members.Any() ? team.Members : Clone(sourceTeam.Members)!;
                team.TeamInvites = team.TeamInvites.Any() ? team.TeamInvites : Clone(sourceTeam.TeamInvites)!;
            }
        }

        foreach(var profile in document.Profiles.Where(profile => profile.Profile.User != null))
        {
            profile.Profile.User!.DisplayName = string.IsNullOrWhiteSpace(profile.Profile.User.DisplayName)
                ? BuildDisplayName(profile.Profile.User.Firstname, profile.Profile.User.Lastname, profile.Profile.User.Username)
                : profile.Profile.User.DisplayName;
        }

        foreach(var game in document.Games)
        {
            EnsureScheduleFields(game);
        }

        return document;
    }

    private static void EnsureScheduleFields(GameExtended game)
    {
        game.PlannedStartTime ??= game.StartTime == default ? DateTime.UtcNow.AddDays(7) : game.StartTime;

        if(game.AverageGameDurationMinutes <= 0)
            game.AverageGameDurationMinutes = 30;

        if(game.RoundBreakDurationMinutes <= 0)
            game.RoundBreakDurationMinutes = 10;

        foreach(var match in game.Matches)
        {
            match.EstimatedStartTime ??= match.StartTime == default ? null : match.StartTime;
            match.EstimatedEndTime ??= match.EndTime == default ? null : match.EndTime;
        }

        game.EstimatedEndTime ??= game.Matches
            .Select(match => match.EstimatedEndTime)
            .Where(estimatedEnd => estimatedEnd.HasValue)
            .Max();
    }

    private void SeedFeaturedDoubleEliminationFixture()
    {
        var game = _document.Games.FirstOrDefault(candidate => candidate.Id == FeaturedDoubleEliminationGameId);
        if(game == null)
            return;

        var teams = BuildFeaturedDoubleEliminationTeams();

        game.Name = "Valorant";
        game.StartTime = new DateTime(2026, 6, 14, 12, 0, 0, DateTimeKind.Utc);
        game.EndTime = new DateTime(2026, 6, 14, 23, 0, 0, DateTimeKind.Utc);
        game.PlannedStartTime = game.StartTime;
        game.AverageGameDurationMinutes = 30;
        game.RoundBreakDurationMinutes = 15;
        game.EstimatedEndTime = game.EndTime;
        game.Status = GameStatus.InProgress;
        game.BracketType = BracketType.DoubleElimination;
        game.Format = GameFormat.BestOf3;
        game.FinalsFormat = GameFormat.BestOf5;
        game.ParticipationMode = ParticipationMode.Team;
        game.RegisterFormUrl = "https://example.test/register/valorant";
        game.Placements = [];
        game.Users = [];
        game.Teams = Clone(teams)!;
        game.Matches = BuildFeaturedDoubleEliminationMatches(game.Id, teams);

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

    private static List<Match> BuildFeaturedDoubleEliminationMatches(Guid gameId, IReadOnlyList<Team> teams)
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
            BuildFeaturedMatch(ubRound1Match1Id, gameId, startTime, 1, 1, false, teamIds["Team Alpha"], teamIds["Mid Control"], 2, 0, teamIds["Team Alpha"], teamIds["Mid Control"], ubRound2Match1Id, lbRound1Match1Id),
            BuildFeaturedMatch(ubRound1Match2Id, gameId, startTime, 1, 2, false, teamIds["Quantum Queue"], teamIds["Orbital Ops"], 2, 1, teamIds["Quantum Queue"], teamIds["Orbital Ops"], ubRound2Match1Id, lbRound1Match1Id),
            BuildFeaturedMatch(ubRound1Match3Id, gameId, startTime, 1, 3, false, teamIds["Echo Unit"], teamIds["Neon Knights"], 2, 0, teamIds["Echo Unit"], teamIds["Neon Knights"], ubRound2Match2Id, lbRound1Match2Id),
            BuildFeaturedMatch(ubRound1Match4Id, gameId, startTime, 1, 4, false, teamIds["Delta Drop"], teamIds["Vector Vipers"], 2, 1, teamIds["Delta Drop"], teamIds["Vector Vipers"], ubRound2Match2Id, lbRound1Match2Id),
            BuildFeaturedMatch(ubRound1Match5Id, gameId, startTime, 1, 5, false, teamIds["Binary Bandits"], teamIds["Haven Hackers"], 2, 0, teamIds["Binary Bandits"], teamIds["Haven Hackers"], ubRound2Match3Id, lbRound1Match3Id),
            BuildFeaturedMatch(ubRound1Match6Id, gameId, startTime, 1, 6, false, teamIds["Radiant Rift"], teamIds["Prism Protocol"], 2, 1, teamIds["Radiant Rift"], teamIds["Prism Protocol"], ubRound2Match3Id, lbRound1Match3Id),
            BuildFeaturedMatch(ubRound1Match7Id, gameId, startTime, 1, 7, false, teamIds["Frame Perfect"], teamIds["Spike Syndicate"], 1, 2, teamIds["Spike Syndicate"], teamIds["Frame Perfect"], ubRound2Match4Id, lbRound1Match4Id),
            BuildFeaturedMatch(ubRound1Match8Id, gameId, startTime, 1, 8, false, teamIds["Gamma Grid"], teamIds["Pixel Pushers"], 2, 0, teamIds["Gamma Grid"], teamIds["Pixel Pushers"], ubRound2Match4Id, lbRound1Match4Id),

            BuildFeaturedMatch(lbRound1Match1Id, gameId, startTime.AddMinutes(90), 1, 1, true, teamIds["Mid Control"], teamIds["Orbital Ops"], 0, 2, teamIds["Orbital Ops"], teamIds["Mid Control"], lbRound2Match1Id, null),
            BuildFeaturedMatch(lbRound1Match2Id, gameId, startTime.AddMinutes(90), 1, 2, true, teamIds["Neon Knights"], teamIds["Vector Vipers"], 1, 2, teamIds["Vector Vipers"], teamIds["Neon Knights"], lbRound2Match2Id, null),
            BuildFeaturedMatch(lbRound1Match3Id, gameId, startTime.AddMinutes(90), 1, 3, true, teamIds["Haven Hackers"], teamIds["Prism Protocol"], 1, 2, teamIds["Prism Protocol"], teamIds["Haven Hackers"], lbRound2Match3Id, null),
            BuildFeaturedMatch(lbRound1Match4Id, gameId, startTime.AddMinutes(90), 1, 4, true, teamIds["Frame Perfect"], teamIds["Pixel Pushers"], 2, 0, teamIds["Frame Perfect"], teamIds["Pixel Pushers"], lbRound2Match4Id, null),

            BuildFeaturedMatch(ubRound2Match1Id, gameId, startTime.AddMinutes(180), 2, 1, false, teamIds["Team Alpha"], teamIds["Quantum Queue"], 2, 0, teamIds["Team Alpha"], teamIds["Quantum Queue"], ubRound3Match1Id, lbRound2Match1Id),
            BuildFeaturedMatch(ubRound2Match2Id, gameId, startTime.AddMinutes(180), 2, 2, false, teamIds["Echo Unit"], teamIds["Delta Drop"], 1, 2, teamIds["Delta Drop"], teamIds["Echo Unit"], ubRound3Match1Id, lbRound2Match2Id),
            BuildFeaturedMatch(ubRound2Match3Id, gameId, startTime.AddMinutes(180), 2, 3, false, teamIds["Binary Bandits"], teamIds["Radiant Rift"], 2, 1, teamIds["Binary Bandits"], teamIds["Radiant Rift"], ubRound3Match2Id, lbRound2Match3Id),
            BuildFeaturedMatch(ubRound2Match4Id, gameId, startTime.AddMinutes(180), 2, 4, false, teamIds["Spike Syndicate"], teamIds["Gamma Grid"], 0, 2, teamIds["Gamma Grid"], teamIds["Spike Syndicate"], ubRound3Match2Id, lbRound2Match4Id),

            BuildFeaturedMatch(lbRound2Match1Id, gameId, startTime.AddMinutes(270), 2, 1, true, teamIds["Orbital Ops"], teamIds["Quantum Queue"], 0, 2, teamIds["Quantum Queue"], teamIds["Orbital Ops"], lbRound3Match1Id, null),
            BuildFeaturedMatch(lbRound2Match2Id, gameId, startTime.AddMinutes(270), 2, 2, true, teamIds["Vector Vipers"], teamIds["Echo Unit"], 0, 2, teamIds["Echo Unit"], teamIds["Vector Vipers"], lbRound3Match1Id, null),
            BuildFeaturedMatch(lbRound2Match3Id, gameId, startTime.AddMinutes(270), 2, 3, true, teamIds["Prism Protocol"], teamIds["Radiant Rift"], 1, 2, teamIds["Radiant Rift"], teamIds["Prism Protocol"], lbRound3Match2Id, null),
            BuildFeaturedMatch(lbRound2Match4Id, gameId, startTime.AddMinutes(270), 2, 4, true, teamIds["Frame Perfect"], teamIds["Spike Syndicate"], 1, 2, teamIds["Spike Syndicate"], teamIds["Frame Perfect"], lbRound3Match2Id, null),

            BuildFeaturedMatch(ubRound3Match1Id, gameId, startTime.AddMinutes(360), 3, 1, false, teamIds["Team Alpha"], teamIds["Delta Drop"], 2, 1, teamIds["Team Alpha"], teamIds["Delta Drop"], ubFinalMatchId, lbRound4Match1Id),
            BuildFeaturedMatch(ubRound3Match2Id, gameId, startTime.AddMinutes(360), 3, 2, false, teamIds["Binary Bandits"], teamIds["Gamma Grid"], 2, 1, teamIds["Binary Bandits"], teamIds["Gamma Grid"], ubFinalMatchId, lbRound4Match2Id),

            BuildFeaturedMatch(lbRound3Match1Id, gameId, startTime.AddMinutes(450), 3, 1, true, teamIds["Quantum Queue"], teamIds["Echo Unit"], 1, 2, teamIds["Echo Unit"], teamIds["Quantum Queue"], lbRound4Match1Id, null),
            BuildFeaturedMatch(lbRound3Match2Id, gameId, startTime.AddMinutes(450), 3, 2, true, teamIds["Radiant Rift"], teamIds["Spike Syndicate"], 1, 2, teamIds["Spike Syndicate"], teamIds["Radiant Rift"], lbRound4Match2Id, null),

            BuildFeaturedMatch(lbRound4Match1Id, gameId, startTime.AddMinutes(540), 4, 1, true, teamIds["Echo Unit"], teamIds["Delta Drop"], 0, 2, teamIds["Delta Drop"], teamIds["Echo Unit"], lbRound5MatchId, null),
            BuildFeaturedMatch(lbRound4Match2Id, gameId, startTime.AddMinutes(540), 4, 2, true, teamIds["Spike Syndicate"], teamIds["Gamma Grid"], 0, 2, teamIds["Gamma Grid"], teamIds["Spike Syndicate"], lbRound5MatchId, null),
            BuildFeaturedMatch(ubFinalMatchId, gameId, startTime.AddMinutes(540), 4, 3, false, teamIds["Team Alpha"], teamIds["Binary Bandits"], 3, 1, teamIds["Team Alpha"], teamIds["Binary Bandits"], grandFinalMatchId, lbRound6MatchId, GameFormat.BestOf5),

            BuildFeaturedMatch(lbRound5MatchId, gameId, startTime.AddMinutes(630), 5, 1, true, teamIds["Delta Drop"], teamIds["Gamma Grid"], 1, 3, teamIds["Gamma Grid"], teamIds["Delta Drop"], lbRound6MatchId, null, GameFormat.BestOf5),
            BuildFeaturedMatch(lbRound6MatchId, gameId, startTime.AddMinutes(720), 6, 1, true, teamIds["Gamma Grid"], teamIds["Binary Bandits"], 3, 2, teamIds["Gamma Grid"], teamIds["Binary Bandits"], grandFinalMatchId, null, GameFormat.BestOf5),
            BuildFeaturedMatch(grandFinalMatchId, gameId, startTime.AddMinutes(810), 7, 1, false, teamIds["Team Alpha"], teamIds["Gamma Grid"], null, null, null, null, null, null, GameFormat.BestOf5)
        ];
    }

    private static Match BuildFeaturedMatch(
        string matchId,
        Guid gameId,
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
        GameFormat? format = null)
    {
        return new Match
        {
            Id = Guid.Parse(matchId),
            StartTime = startTime,
            EndTime = startTime.AddMinutes(format == GameFormat.BestOf5 ? 75 : 60),
            EstimatedStartTime = startTime,
            EstimatedEndTime = startTime.AddMinutes(format == GameFormat.BestOf5 ? 75 : 60),
            BracketType = BracketType.DoubleElimination,
            Format = format ?? GameFormat.BestOf3,
            ParticipationMode = ParticipationMode.Team,
            RoundNumber = roundNumber,
            MatchNumber = matchNumber,
            IsLowerBracketMatch = isLowerBracketMatch,
            GameId = gameId,
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

    private static List<Placement> BuildPlacements(GameExtended game)
    {
        if(game.ParticipationMode == ParticipationMode.Team)
        {
            return game.Teams.Take(4).Select((team, index) => new Placement
            {
                Place = index + 1,
                Teams = [Clone(team)!]
            }).ToList();
        }

        return game.Users.Take(4).Select((user, index) => new Placement
        {
            Place = index + 1,
            Users = [Clone(user)!]
        }).ToList();
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

    private GameExtended GetRequiredGame(Guid id) =>
        _document.Games.FirstOrDefault(game => game.Id == id)
        ?? throw new InvalidOperationException($"Mock game '{id}' was not found.");

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

        foreach(var game in _document.Games)
        {
            var teams = game.Teams.ToList();
            var index = teams.FindIndex(existing => existing.Id == team.Id);
            if(index >= 0)
            {
                teams[index] = Clone(team)!;
                game.Teams = teams;
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

    private static Game ToGame(GameExtended game)
    {
        return new Game
        {
            Id = game.Id,
            Name = game.Name,
            StartTime = game.StartTime,
            EndTime = game.EndTime,
            PlannedStartTime = game.PlannedStartTime,
            AverageGameDurationMinutes = game.AverageGameDurationMinutes,
            RoundBreakDurationMinutes = game.RoundBreakDurationMinutes,
            EstimatedEndTime = game.EstimatedEndTime,
            ImageUrl = game.ImageUrl,
            Status = game.Status,
            BracketType = game.BracketType,
            Format = game.Format,
            FinalsFormat = game.FinalsFormat,
            ParticipationMode = game.ParticipationMode,
            RegisterFormUrl = game.RegisterFormUrl
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
