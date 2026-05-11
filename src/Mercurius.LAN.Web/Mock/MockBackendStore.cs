using System.Text.Json;
using System.Text.Json.Serialization;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
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

    private readonly object _syncRoot = new();
    private readonly string _dataFilePath;
    private MockBackendDocument _document;

    public MockBackendStore(IHostEnvironment environment, IOptions<MockBackendOptions> options)
    {
        _dataFilePath = Path.Combine(environment.ContentRootPath, options.Value.DataFilePath);
        _document = LoadDocument(_dataFilePath);
    }

    public List<Game> GetGames()
    {
        lock(_syncRoot)
        {
            return Clone(_document.Games.Select(ToGame).ToList())!;
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
                StartTime = DateTime.UtcNow.AddDays(7),
                EndTime = DateTime.UtcNow.AddDays(7).AddHours(4),
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

    public Game UpdateGame(Guid id, UpdateGameDTO dto)
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

            if(dto.Image != null)
                game.ImageUrl = "/mock-data-local/generated-game.svg";

            return Clone(ToGame(game))!;
        }
    }

    public GameExtended RegisterUser(Guid gameId, Guid userId)
    {
        lock(_syncRoot)
        {
            var game = GetRequiredGame(gameId);
            var user = GetRequiredUser(userId);

            if(game.Users.All(existing => existing.Id != userId))
                game.Users = game.Users.Append(Clone(user)!).ToList();

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
                Members = [Clone(captain)!],
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
                team.Members = team.Members.Append(Clone(GetRequiredUser(dto.CaptainUserId.Value))!).ToList();

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

        return document;
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

    private static Game ToGame(GameExtended game)
    {
        return new Game
        {
            Id = game.Id,
            Name = game.Name,
            StartTime = game.StartTime,
            EndTime = game.EndTime,
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
