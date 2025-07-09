
using MercuriusAalstLanParty.Client.Models;

namespace MercuriusAalstLanParty.Client.Services;

public class GameService : IGameService
{
    private readonly List<GameExtended> _games = new()
    {
        new GameExtended
        {
            Id = 1,
            Name = "Counter-Strike 2",
            PictureUrl = "https://picsum.photos/seed/cs2/540/300",
            Status = "Scheduled",
            BracketType = "DoubleElimination",
            Format = "BestOf3",
            FinalsFormat = "BestOf5",
            ParticipantType = ParticipantType.Team,
            Matches = new List<Match> {
                new Match { Id = 1, RoundNumber = 1, MatchNumber = 1, Participant1Id = 1, Participant2Id = 2, Participant1Score = 16, Participant2Score = 10 },
                new Match { Id = 2, RoundNumber = 1, MatchNumber = 2, Participant1Id = 3, Participant2Id = 4, Participant1Score = 8, Participant2Score = 16 }
            },
            Participants = new List<Participant> {
                new Team { Id = 1, Name = "Team Alpha" },
                new Team { Id = 2, Name = "Team Bravo" },
                new Team { Id = 3, Name = "Team Charlie" },
                new Team { Id = 4, Name = "Team Delta" }
            },
            Placements = new List<Placement> {
                new Placement { Place = 1, Participants = new List<Participant> { new Team { Id = 1, Name = "Team Alpha" } } },
                new Placement { Place = 2, Participants = new List<Participant> { new Team { Id = 2, Name = "Team Bravo" } } },
                new Placement { Place = 3, Participants = new List<Participant> { new Team { Id = 3, Name = "Team Charlie" } } },
                new Placement { Place = 4, Participants = new List<Participant> { new Team { Id = 4, Name = "Team Delta" } } }
            }
        },
        new GameExtended
        {
            Id = 2,
            Name = "Rocket League",
            PictureUrl = "https://picsum.photos/seed/rl/540/300",
            Status = "InProgress",
            BracketType = "SingleElimination",
            Format = "BestOf1",
            FinalsFormat = "BestOf3",
            ParticipantType = ParticipantType.Player,
            Matches = new List<Match> {
                // Quarterfinals (Round 1)
                new Match { Id = 1, RoundNumber = 1, MatchNumber = 1, Participant1Id = 5, Participant2Id = 6, Participant1Score = 3, Participant2Score = 2 },
                new Match { Id = 2, RoundNumber = 1, MatchNumber = 2, Participant1Id = 7, Participant2Id = 8, Participant1Score = 1, Participant2Score = 4 },
                new Match { Id = 3, RoundNumber = 1, MatchNumber = 3, Participant1Id = 9, Participant2Id = 10, Participant1Score = 2, Participant2Score = 3 },
                new Match { Id = 4, RoundNumber = 1, MatchNumber = 4, Participant1Id = 11, Participant2Id = 12, Participant1Score = 0, Participant2Score = 2 },

                // Semifinals (Round 2)
                new Match { Id = 5, RoundNumber = 2, MatchNumber = 1, Participant1Id = 5, Participant2Id = 8, Participant1Score = 2, Participant2Score = 3 }, // Winners of M1 and M2
                new Match { Id = 6, RoundNumber = 2, MatchNumber = 2, Participant1Id = 10, Participant2Id = 12, Participant1Score = 1, Participant2Score = 2 }, // Winners of M3 and M4

                // Final (Round 3)
                new Match { Id = 7, RoundNumber = 3, MatchNumber = 1, Participant1Id = 8, Participant2Id = 12, Participant1Score = 2, Participant2Score = 3 } // Winners of M5 and M6
            },
            Participants = new List<Participant> {
                new Player { Id = 5, Username = "rocket1", Firstname = "Rick", Lastname = "Rocket", Email = "rick@email.com" },
                new Player { Id = 6, Username = "octane1", Firstname = "Olga", Lastname = "Octane", Email = "olga@email.com" },
                new Player { Id = 7, Username = "dominus1", Firstname = "Daan", Lastname = "Dominus", Email = "daan@email.com" },
                new Player { Id = 8, Username = "fennec1", Firstname = "Fien", Lastname = "Fennec", Email = "fien@email.com" },
                new Player { Id = 9, Username = "gizmo", Firstname = "Gert", Lastname = "Gizmo", Email = "gert@email.com" },
                new Player { Id = 10, Username = "turtle", Firstname = "Tina", Lastname = "Turtle", Email = "tina@email.com" },
                new Player { Id = 11, Username = "wolf", Firstname = "Wout", Lastname = "Wolf", Email = "wout@email.com" },
                new Player { Id = 12, Username = "lion", Firstname = "Lies", Lastname = "Lion", Email = "lies@email.com" }
            },
            Placements = new List<Placement> {
                new Placement { Place = 1, Participants = new List<Participant> { new Player { Id = 5, Username = "rocket1", Firstname = "Rick", Lastname = "Rocket", Email = "rick@email.com" } } },
                new Placement { Place = 2, Participants = new List<Participant> { new Player { Id = 6, Username = "octane1", Firstname = "Olga", Lastname = "Octane", Email = "olga@email.com" } } },
                new Placement { Place = 3, Participants = new List<Participant> { new Player { Id = 7, Username = "dominus1", Firstname = "Daan", Lastname = "Dominus", Email = "daan@email.com" } } },
                new Placement { Place = 4, Participants = new List<Participant> { new Player { Id = 8, Username = "fennec1", Firstname = "Fien", Lastname = "Fennec", Email = "fien@email.com" } } }
            }
        },
        new GameExtended
        {
            Id = 3,
            Name = "Valorant",
            PictureUrl = "https://picsum.photos/seed/valorant/540/300",
            Status = "Completed",
            BracketType = "SingleElimination",
            Format = "BestOf1",
            FinalsFormat = "BestOf3",
            ParticipantType = ParticipantType.Team,
            Matches = new List<Match>(),
            Participants = new List<Participant>(),
            Placements = new List<Placement>()
        }
    };


    public Task<List<Game>> GetGamesAsync()
    {
        return Task.FromResult(_games.Select(g => g as Game).ToList());
    }


    public Task<GameExtended?> GetGameByIdAsync(int id)
    {
        var game = _games.FirstOrDefault(g => g.Id == id);
        return Task.FromResult(game);
    }

    public Task<Game> RegisterForGameAsync() => throw new NotImplementedException();
}
