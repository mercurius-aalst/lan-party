using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Mercurius.LAN.Web.Components.Pages.Games.Matches.BracketView;

public partial class DoubleEliminationBracketComponent
{
    [Parameter] public GameExtended Game { get; set; } = null!;
    [Parameter] public EventCallback OnDataReload { get; set; }

    [Inject] private IJSRuntime JS { get; set; } = null!;

    private IEnumerable<Match> _uBMatches = Enumerable.Empty<Match>();
    private IEnumerable<Match> _lBMatches = Enumerable.Empty<Match>();
    private Match? _gFMatch;
    private GameExtended _upperBracketGame = new();
    private int LastRound => Game.Matches?.Max(m => m.RoundNumber) ?? 0;

    protected override void OnParametersSet()
    {
        if(Game.Matches.Any())
        {
            _gFMatch = Game.Matches.SingleOrDefault(m => m.RoundNumber == LastRound);
            _uBMatches = Game.Matches.Where(m => !m.IsLowerBracketMatch && m.RoundNumber < LastRound).ToList();
            _lBMatches = Game.Matches.Where(m => m.IsLowerBracketMatch && m.RoundNumber < LastRound).ToList();
            _upperBracketGame = new GameExtended
            {
                Id = Game.Id,
                Name = Game.Name,
                StartTime = Game.StartTime,
                EndTime = Game.EndTime,
                ImageUrl = Game.ImageUrl,
                Status = Game.Status,
                BracketType = Game.BracketType,
                Format = Game.Format,
                FinalsFormat = Game.FinalsFormat,
                ParticipationMode = Game.ParticipationMode,
                RegisterFormUrl = Game.RegisterFormUrl,
                Matches = _uBMatches.ToList(),
                Users = Game.Users.ToList(),
                Teams = Game.Teams.ToList(),
                Placements = Game.Placements.ToList()
            };
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            await JS.InvokeVoidAsync("makeDraggable", "double-elimination-bracket-root");
        }
    }

    private struct LowerBracketMatchData
    {
        public int SvgWidth { get; set; }
        public int SvgHeight { get; set; }
        public int MatchHeight { get; set; }
        public int MatchWidth { get; set; }
        public int ColumnWidth { get; set; }
        public Dictionary<Guid, (int left, int y)> MatchPositions { get; set; }
        public List<string> SvgElements { get; set; }
    }

    private LowerBracketMatchData CalculateLowerBracketLayout(List<IGrouping<int, Match>> rounds, IEnumerable<Match> uBMatches)
    {
        var matchData = new LowerBracketMatchData
        {
            MatchHeight = 60,
            MatchWidth = 220,
            ColumnWidth = 260,
            MatchPositions = new Dictionary<Guid, (int left, int y)>(),
            SvgElements = new List<string>()
        };

        int verticalGap = 32;
        int numRounds = rounds.Count;
        int reservedHeight = matchData.MatchHeight + verticalGap;

        var lbMatchesReceivingLosersFromUB = new HashSet<Guid>(uBMatches.Where(m => m.LoserNextMatchId.HasValue).Select(m => m.LoserNextMatchId!.Value));

        for(int r = 0; r < rounds.Count; r++)
        {
            var currentRoundMatches = rounds[r].OrderBy(m => m.MatchNumber).ToList();
            int colX = r * matchData.ColumnWidth + (matchData.ColumnWidth - matchData.MatchWidth) / 2;

            if(r == 0)
            {
                for(int i = 0; i < currentRoundMatches.Count; i++)
                {
                    var match = currentRoundMatches[i];
                    int y = i * reservedHeight;
                    matchData.MatchPositions.Add(match.Id, (colX, y));
                }
            }
            else
            {
                var previousRoundMatches = rounds[r - 1].ToList();

                for(int i = 0; i < currentRoundMatches.Count; i++)
                {
                    var currentMatch = currentRoundMatches[i];

                    var sourceMatches = previousRoundMatches
                        .Where(m => m.WinnerNextMatchId == currentMatch.Id)
                        .ToList();

                    if(sourceMatches.Any())
                    {
                        int firstSourceY = matchData.MatchPositions[sourceMatches.First().Id].y;
                        int lastSourceY = matchData.MatchPositions[sourceMatches.Last().Id].y;
                        int y = (int)Math.Round((double)(firstSourceY + lastSourceY) / 2);
                        matchData.MatchPositions.Add(currentMatch.Id, (colX, y));
                    }
                    else
                    {
                        int y = i * reservedHeight;
                        matchData.MatchPositions.Add(currentMatch.Id, (colX, y));
                    }
                }
            }
        }

        foreach(var match in _lBMatches)
        {
            if(matchData.MatchPositions.TryGetValue(match.Id, out var sourcePos))
            {
                if(match.WinnerNextMatchId.HasValue && matchData.MatchPositions.TryGetValue(match.WinnerNextMatchId.Value, out var destPos))
                {
                    int x0 = sourcePos.left + matchData.MatchWidth;
                    int y0 = sourcePos.y + (int)Math.Round((double)matchData.MatchHeight / 2);
                    int x1 = destPos.left;
                    int y1 = destPos.y + (int)Math.Round((double)matchData.MatchHeight / 2);
                    int elbowX = x0 + (matchData.ColumnWidth - matchData.MatchWidth) / 2;
                    matchData.SvgElements.Add($"<polyline points='{x0},{y0} {elbowX},{y0} {elbowX},{y1} {x1},{y1}' fill='none' stroke='#4caf50' stroke-width='3' />");
                }

                if(lbMatchesReceivingLosersFromUB.Contains(match.Id))
                {
                    int yConnect = sourcePos.y + (int)Math.Round(matchData.MatchHeight * 0.25);
                    int x1 = sourcePos.left - 10;
                    int y1 = sourcePos.y - 10;
                    int x2 = sourcePos.left - 10;
                    int y2 = yConnect;
                    int x3 = sourcePos.left;
                    int y3 = yConnect;
                    matchData.SvgElements.Add($"<polyline points='{x1},{y1} {x2},{y2} {x3},{y3}' fill='none' stroke='#4caf50' stroke-width='3' stroke-dasharray='5,5' />");
                }
            }
        }

        matchData.SvgWidth = numRounds * matchData.ColumnWidth;
        matchData.SvgHeight = matchData.MatchPositions.Any() ? matchData.MatchPositions.Values.Max(pos => pos.y + matchData.MatchHeight) : 0;

        return matchData;
    }
}
