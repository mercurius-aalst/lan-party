## Why

Public player and team profiles currently show identity, roster, and tournament links but do not
give visitors a concise view of the participant's match history or next scheduled match. Loading
tournament details or individual matches from the browser would be both privacy-sensitive and an
N+1 query pattern.

## What Changes

- Add a dedicated public-safe match-summary contract for player and team profiles.
- Show at most one previous and one upcoming summary per participating tournament.
- Use the existing public profile routes and stable `/tournaments/{tournamentId}` navigation,
  with match links only where the existing match route supports them.
- Preserve explicit loading, empty, error, and not-found states and keep mock mode aligned with the
  live contract.
- Stack on the approved FE match lifecycle UI and BE match lifecycle API changes so Completed,
  Forfeited, and Reversed states are represented authoritatively.

## Non-goals

- No private match reports, moderator/admin data, authentication identifiers, or new mutation route.
- No client-side tournament/match fan-out or per-summary API enrichment.
- No change to tournament registration or match lifecycle rules.

## Dependencies

- FE PR #55 (`codex/issue-44-match-result-lifecycle-ui`, `decf673`) supplies the public lifecycle
  enum and match state semantics.
- BE PR #121 (`codex/issue-84-match-result-lifecycle`, `ca23a96`) supplies persisted lifecycle,
  result, forfeit, and reversal state. This change targets that branch and should merge after it.
- BE PR #122 (`Add public profile match summaries`) supplies the anonymous user/team summary
  endpoints and privacy-safe response contract.
