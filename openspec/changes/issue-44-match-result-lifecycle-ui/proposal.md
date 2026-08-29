# Match result lifecycle UI

Issue #44 adds a match details experience for the controlled backend match-result lifecycle. Players and team captains can confirm that play has ended, submit a score, and explicitly forfeit. Admins can resolve disputes, force a result, and reverse an eligible result. The UI presents the current state, server deadlines, participant context, and actionable errors while refreshing from the authoritative backend after every mutation.

The existing bracket and anonymous match projection remain usable. Live and mock services share the same DTOs and state transitions so local development exercises the same states as production. Client timers are advisory and never decide a result.

## Non-goals

- No client-side bracket advancement or deadline decisions.
- No decline action is offered; the backend supports confirmation only.
- No browser/manual smoke testing is part of this change.
