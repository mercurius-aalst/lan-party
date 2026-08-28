## Why

The front-end currently targets the pre-modular API contract (unversioned `/lan/games`
resources, legacy team actions, and a `POST` profile-completion route), while the current
back-end `main` at `e17d2b9` exposes only versioned `/v1/lan` tournament resources and
resource-oriented registration and team routes. Without a contract-first migration, live pages
will fail at routing or deserialization boundaries even though mock mode continues to work.

## What Changes

- **BREAKING** Retarget the live Refit boundary from aggregate-level `games`/`gameId` contracts to
  the back-end's canonical `tournaments`/`tournamentId` resources. The FE transport and domain
  model surface MUST use `Tournament*` names; only backend-defined field terminology such as
  `AverageGameDurationMinutes` is retained.
- Keep the shared `/v1` base-address strategy and make every live client call resolve to the
  back-end's versioned route groups.
- Replace legacy tournament lifecycle and participant mutations with the current lifecycle-state
  and tournament-registration resources, including eligibility, roster confirmation, and admin
  removal operations.
- Align tournament, match, placement, sponsor-placement, and schedule DTOs with the current
  response fields; remove the obsolete external registration URL and support `TeamSize`.
- Align team management with current leave, invite collection, invite-response, logo, and
  privacy-safe projection contracts, and align profile completion with `PUT /users/me`.
- Align global search with the current `user`, `team`, and `tournament` result records and
  `tournamentId` navigation field.
- Preserve existing public page routes and user-facing loading, empty, error, unauthorized, and
  not-found states while changing only the API-backed contract plumbing required by the live API.
- Keep mock backend services and fixtures behaviorally equivalent to the live contract, including
  version-independent local operation and representative registration/team states.

## Capabilities

### New Capabilities

- `live-backend-contract-integration`: Defines the versioned live API boundary, canonical
  tournament and registration routes, DTO/authorization/privacy expectations, and mock/live
  parity required for the front-end integration.

### Modified Capabilities

- `frontend-tournament-schedule-contract`: Tournament and match schedule models and multipart
  payloads use the current `tournamentId` contract and `TeamSize`/schedule fields.
- `global-search`: Search accepts the back-end's `tournament` result type and `TournamentId`
  navigation field while preserving bounded public search behavior.
- `game-detail-page-experience`: Public tournament detail keeps its existing page surface while
  consuming current tournament, registration, match, placement, and lifecycle response shapes.
- `user-owned-team-management`: Team management actions use current resource routes and
  privacy-safe response projections without changing the user-owned workflow semantics.
- `public-participant-profiles`: Public team tournament links and participant models use the
  current privacy-safe `TournamentId` projection.

## Impact

- Refit contracts in `src/Mercurius.LAN.Web/APIClients/` and live base-address configuration in
  `src/Mercurius.LAN.Web/Extensions/DependencyExtensions.cs`.
- Tournament, match, placement, participant, sponsor-placement, registration, and team DTOs and
  models under `src/Mercurius.LAN.Web/DTOs/` and `src/Mercurius.LAN.Web/Models/`; aggregate
  transport/domain types use `Tournament*` names.
- Service wrappers and page workflows under `src/Mercurius.LAN.Web/Services/` and
  `src/Mercurius.LAN.Web/Components/Pages/`.
- Mock contracts and fixtures under `src/Mercurius.LAN.Web/Mock/`.
- Auth0 role/profile completion handling and API error-state mapping.
- Verified dependency: sibling `D:\Github Repositories\mercurius-aalst-back-end`, clean `main`
  at `e17d2b9`; no new package dependency is expected.
