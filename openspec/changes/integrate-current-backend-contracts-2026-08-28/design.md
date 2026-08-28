## Context

The front-end branch is a Blazor Server application whose live Refit clients currently model an
older API. Its pages and mock store still contain legacy aggregate names, while the sibling
back-end `main` at `e17d2b9` has completed the modular rename to `Tournament` and removed the old
`/v1/lan/games` routes. The current API also replaces legacy participant mutations with internal
tournament registration and exact-roster workflows, and replaces legacy team action routes with
resource-oriented routes.

The migration crosses the Refit boundary, DTOs/models, service wrappers, page workflows, Auth0
profile handling, and mock fixtures. The back-end is read-only scope for this change: its existing
routes, authorization, validation, privacy projections, pagination, and JSON shapes are the
source of truth.

## Goals / Non-Goals

**Goals:**

- Make every live Refit route and request/response type match the current `/v1/lan` API.
- Preserve backend-defined match-format and duration fields while using `TournamentId` and
  tournament resource names for aggregate-level data.
- Expose the current tournament registration, roster, eligibility, lifecycle, sponsor, team, and
  profile operations through explicit client contracts.
- Use canonical public Blazor URLs (`/tournaments`, `/tournaments/{id}`) while mapping them to
  the corresponding tournament API resources; map global-search and public-team links to those
  destinations.
- Keep public projections privacy-safe and keep loading, empty, error, unauthorized, and not-found
  states visible.
- Keep mock mode useful and semantically aligned with live data and mutation outcomes.

**Non-Goals:**

- Changing the sibling back-end, adding HTTP aliases, or retaining `/v1/lan/games` compatibility
  routes.
- Adding a second `/tournaments` page hierarchy or changing the site's visual redesign.
- Changing tournament business rules, search ranking, team authorization, privacy policy, or
  persistence behavior already implemented by the back-end.
- Introducing a new client package, generated OpenAPI client, or browser-side API layer.

## Decisions

### 1. Mirror the versioned back-end contract at the Refit boundary

`ILANClient` and `IUserClient` will use relative `lan/...` attributes (without a leading slash)
with the configured base address normalized to `/v1/` (including its trailing slash), so all
requests resolve to `/v1/lan/...`. Methods will use the exact current route verbs and resource
names. The clients will not preserve removed game routes or invent an HTTP compatibility fallback.
Transport and domain aggregate types MUST use `Tournament*` names; no route-free aliases are
permitted in the completed change.

**Alternative considered:** Keep old paths and add a second client for tournaments. This would
leave live calls broken and create two competing contracts, so it is rejected.

### 2. Keep the existing presentation URLs while canonicalizing aggregate API data

The API transport, domain model, service, and page component contract will use `Tournament` names
and current DTO fields. Public presentation routes and components will use `/tournaments` and
`/tournaments/{id}` so navigation names match the API resource. No aggregate `Game*` alias may
remain after migration, and no model may contain removed `RegisterFormUrl` or legacy `GameId`
fields. Any new transport DTOs will use canonical tournament names rather than adding parallel
compatibility DTOs.

**Alternative considered:** Keep `/games` pages and aggregate names as permanent compatibility
aliases. That would leave the front-end vocabulary inconsistent with the API and hide contract
drift, so the completed migration uses `/tournaments` names throughout.

### 3. Represent the detail response as the back-end returns it

Tournament detail transport models will include `TeamSize`, active public `Registrations`, match
`TournamentId`, and a nullable singular `SponsorPlacement`. Existing participant views that need
flat user/team collections will derive them once from loaded registrations in the service or page
model; they will not issue one API call per participant. Public detail rendering will only consume
the fields returned by the anonymous projection.

**Alternative considered:** Continue treating the response as a plural sponsor/participant legacy
shape and silently synthesize fields. That hides contract drift and can expose stale/private data,
so it is rejected.

### 4. Replace legacy registration UI with backend-authoritative registration state

The detail page will load current-user registration state only for authenticated users. Individual
registration uses the self `PUT`/`DELETE` resources; team registration uses eligibility, exact
roster submission, and roster-member confirmation resources; admin registration management is
limited to the back-end's removal endpoints. The former admin add-user/add-team flow and external
registration URL are removed from live and mock paths. The backend remains authoritative after
every eligibility check and mutation.

**Alternative considered:** Keep the old add-participant controls and route them through new
endpoints. The new API intentionally does not expose admin additions or arbitrary roster swaps, so
this would misrepresent authorization and is rejected.

### 5. Adapt team and profile actions in one service-owned contract slice

Team service methods will call `DELETE .../members/me`, `POST .../{teamId}/invites` with a body,
`PATCH /team-invites/{inviteId}`, and current logo routes. Profile completion will call `PUT
/users/me`. Response models will omit assumptions about private fields while preserving existing
service-level error context and user-facing messages.

**Alternative considered:** Have pages construct raw `HttpRequestMessage` instances for new routes.
That bypasses the established Refit boundary and makes mock/live parity harder to test, so it is
rejected.

### 6. Keep mock behavior behind the same service interfaces

Mock services will continue to implement `ITournamentService`, `ITeamService`, and related interfaces so
local pages do not branch on transport mode. The mock store will expose tournament-shaped result
fields (`TournamentId`, `tournament`, registrations, team size) and representative blocked and
successful states for the same workflows. It will not need to emulate `/v1` URL construction.

## Risks / Trade-offs

- [Risk] The API rename touches nearly every game page and can accidentally rename genuine
  single-game concepts. → Classify each occurrence; keep backend-defined format and duration terminology,
  update only aggregate identifiers, and compile after each slice.
- [Risk] Current detail pages expect top-level `Users`/`Teams`, while the live detail projection
  nests participants under active registrations. → Add one bounded projection/derivation from the
  already loaded response and cover individual/team/empty cases; do not add per-participant calls.
- [Risk] Removing `RegisterFormUrl` changes the current hero action and create/edit forms. → Make
  registration state and authentication explicit, preserve disabled/closed states, and update mock
  fixtures before removing the old field from page code.
- [Risk] Team invite/leave route changes alter verbs, bodies, and response DTOs. → Keep all route
  declarations in `ILANClient`, test serialized request shapes, and refresh confirmed state after
  mutations.
- [Risk] An incomplete FE deploy cannot talk to a back-end that intentionally removed old routes.
  → Treat the FE migration as an atomic release against the pinned back-end commit; do not deploy
  a partially migrated client, and roll back the paired FE/back-end release together.
- [Risk] Mock fixtures can drift from live privacy and registration projections. → Keep fixture
  fields limited to live public DTOs, add representative pending/active roster states, and run
  both mock and live smoke checks before handoff.

## Migration Plan

1. Freeze the verified contract inventory in the Refit clients and transport DTOs, including route
   verbs, version prefix, auth requirements, multipart field names, enum values, and response
   wrappers.
2. Migrate canonical tournament/match/sponsor/search models and read-only home, browse, detail,
   public-profile, and navigation consumers. Use `/tournaments` presentation URLs.
3. Migrate admin tournament create/update, sponsor replacement, lifecycle-state, and match-score
   workflows; remove external registration URL fields and add `TeamSize` validation.
4. Migrate individual/team registration state, eligibility, roster submission/confirmation, and
   admin removal workflows. Remove old admin participant additions and game action calls.
5. Migrate team management and profile-completion route shapes, then update mock services/store and
   fixtures to exercise the same visible states.
6. Validate with `openspec validate`, the FE build, Refit request-shape tests or equivalent route
   assertions, and manual smoke checks against the current back-end and mock mode. Deploy only when
   all slices are complete.

Rollback is a paired release rollback: because the back-end deliberately has no old route aliases,
the pre-migration FE must not be run against the migrated API. Reverting only source files on this
branch is safe during development; production rollback requires restoring the previously compatible
FE and back-end versions together.

## Open Questions

- The migration deliberately uses `/tournaments` presentation URLs; a later release may decide
  whether any legacy `/games` redirect is needed, but it is outside this change.
- Which authenticated detail registration controls are required in the final UX for selecting an
  exact team roster? The API contract is known, but product copy and interaction details should be
  confirmed while implementing the team-registration slice.
- Is a dedicated FE API-contract test harness available or should route/JSON assertions use the
  existing build plus a small test project? No FE test project exists in the current repository;
  implementation should avoid introducing a package unless a focused test project is warranted.
