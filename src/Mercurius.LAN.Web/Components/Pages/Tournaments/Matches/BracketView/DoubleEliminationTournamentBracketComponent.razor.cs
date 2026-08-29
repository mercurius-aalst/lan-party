using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Models.Matches;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Matches.BracketView;

public partial class DoubleEliminationTournamentBracketComponent
{
    private enum BracketView
    {
        Upper,
        Lower,
        GrandFinal
    }

    private readonly record struct BracketViewOption(BracketView View, string Label, string Meta);

    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
    [Parameter] public EventCallback<Match> OnDataReload { get; set; }
    [Parameter] public EventCallback<Match> OnMatchRefreshed { get; set; }

    [Inject] private IJSRuntime JS { get; set; } = null!;

    private IEnumerable<Match> _uBMatches = Enumerable.Empty<Match>();
    private IEnumerable<Match> _lBMatches = Enumerable.Empty<Match>();
    private Match? _gFMatch;
    private TournamentExtended _upperBracketTournament = new();
    private IReadOnlyList<BracketViewOption> _viewOptions = [];
    private BracketView _activeView = BracketView.Upper;
    private BracketView? _lastInitializedDragView;
    private int LastRound => Tournament.Matches?.Max(m => m.RoundNumber) ?? 0;

    protected override void OnParametersSet()
    {
        if(Tournament.Matches.Any())
        {
            _gFMatch = Tournament.Matches.SingleOrDefault(m => m.RoundNumber == LastRound);
            _uBMatches = Tournament.Matches.Where(m => !m.IsLowerBracketMatch && m.RoundNumber < LastRound).ToList();
            _lBMatches = Tournament.Matches.Where(m => m.IsLowerBracketMatch && m.RoundNumber < LastRound).ToList();
            _upperBracketTournament = new TournamentExtended
            {
                Id = Tournament.Id,
                Name = Tournament.Name,
                StartTime = Tournament.StartTime,
                EndTime = Tournament.EndTime,
                ImageUrl = Tournament.ImageUrl,
                Status = Tournament.Status,
                BracketType = Tournament.BracketType,
                Format = Tournament.Format,
                FinalsFormat = Tournament.FinalsFormat,
                ParticipationMode = Tournament.ParticipationMode,
                TeamSize = Tournament.TeamSize,
                Matches = _uBMatches.ToList(),
                Registrations = Tournament.Registrations.ToList(),
                Users = Tournament.Users.ToList(),
                Teams = Tournament.Teams.ToList(),
                Placements = Tournament.Placements.ToList()
            };

            _viewOptions =
            [
                new(BracketView.Upper, "Upper bracket", $"{_uBMatches.Count()} matches"),
                new(BracketView.Lower, "Lower bracket", $"{_lBMatches.Count()} matches"),
                new(BracketView.GrandFinal, "Grand final", _gFMatch == null ? "Pending" : GetMatchFormatLabel(_gFMatch))
            ];

            if(_activeView == BracketView.Lower && !_lBMatches.Any())
                _activeView = BracketView.Upper;

            if(_activeView == BracketView.GrandFinal && _gFMatch == null)
                _activeView = _lBMatches.Any() ? BracketView.Lower : BracketView.Upper;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_activeView == BracketView.Lower && _lastInitializedDragView != BracketView.Lower)
        {
            await JS.InvokeVoidAsync("makeDraggable", "double-elimination-bracket-root");
            _lastInitializedDragView = BracketView.Lower;
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

    private void SelectView(BracketView view)
    {
        if(view != BracketView.Lower)
            _lastInitializedDragView = null;

        _activeView = view;
    }

    private string GetTabCssClass(BracketView view) =>
        view == _activeView ? "double-elimination-tab--active" : string.Empty;

    private static string GetMatchFormatLabel(Match match) =>
        match.Format switch
        {
            TournamentFormat.BestOf1 => "Best of 1",
            TournamentFormat.BestOf3 => "Best of 3",
            TournamentFormat.BestOf5 => "Best of 5",
            _ => "Final set"
        };
}
