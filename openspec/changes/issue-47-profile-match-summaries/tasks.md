## 1. Contract and backend projection

- [x] 1.1 Add public match-summary contract records/enums and response DTOs with only safe fields.
- [x] 1.2 Add a tournament-module projection that resolves active user/team registrations and
      selects one previous/upcoming match per tournament using lifecycle-aware deterministic order.
- [x] 1.3 Resolve opponent labels in bounded set-based work from public registration snapshots or
      public identity/team projections; avoid N+1 calls and exclude BYE/canceled/unresolved cases.
- [x] 1.4 Wire anonymous versioned user/team summary endpoints and 404/empty semantics.

## 2. Frontend profiles and mock parity

- [x] 2.1 Align FE DTOs, Refit contracts, and public profile services with the summary response.
- [x] 2.2 Load profile and summaries with cancellation/generation guards so rapid route changes do
      not allow stale results or errors to overwrite the current profile.
- [x] 2.3 Render previous/upcoming sections with opponent, status/result, timing, and stable links,
      plus separate loading, empty, error, not-found, and TBD states.
- [x] 2.4 Extend mock fixtures/store/services with lifecycle-aware public summaries and parity
      behavior without exposing private fields.

## 3. Tests and validation

- [x] 3.1 Add backend projection tests for individual/team, single/double bracket, active
      registration, deterministic tie-break, BYE/TBD, canceled, forfeited, reversed, and
      unresolved states.
- [x] 3.2 Add backend route/serialization/privacy/query-count regressions.
- [x] 3.3 Add frontend contract/service/component regressions for both profile types, links,
      loading/empty/error/not-found, mock parity, and stale-route protection.
- [x] 3.4 Run OpenSpec strict, backend/frontend builds and relevant tests; record remaining
      format-baseline exceptions if any.
