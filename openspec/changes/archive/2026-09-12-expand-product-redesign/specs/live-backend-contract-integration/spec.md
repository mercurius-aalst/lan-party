## MODIFIED Requirements

### Requirement: Live Refit calls use the versioned canonical API boundary

The reorganized front end MUST continue to send live API requests through the
existing Refit clients with a single `/v1` base path and the exact current
back-end route, verb, query, body, and multipart field names. Product-level
journey composition MUST NOT call removed aliases, construct a second ad-hoc
HTTP client, or invent a presentation-specific endpoint.

#### Scenario: Public tournament browse request

- **WHEN** the front end loads the tournament browse surface in live mode
- **THEN** it MUST send `GET /v1/lan/tournaments` with optional `page` and
  `pageSize` query values
- **AND** the journey composition MUST consume the existing response projection

#### Scenario: Public tournament detail request

- **WHEN** the front end loads a tournament detail page in live mode
- **THEN** it MUST send `GET /v1/lan/tournaments/{tournamentId}`
- **AND** it MUST not send `GET /v1/lan/games/{gameId}`

#### Scenario: Canonical GUID route uses the existing API identifier

- **WHEN** the canonical GUID tournament detail route is opened
- **THEN** the front end MUST use the route GUID as the existing tournament
  identifier before calling the detail API
- **AND** the live request MUST keep the existing `tournamentId` path value and
  MUST NOT require a name/slug route or lookup endpoint

#### Scenario: Removed game route is not used

- **WHEN** the live API client contract is inspected or exercised
- **THEN** no Refit method MUST target `/lan/games`
- **AND** no fallback request MUST be attempted when a canonical tournament
  call fails

#### Scenario: Reorganized journey reaches a live action

- **WHEN** a player or organizer selects an existing API-coupled action from a
  reorganized surface
- **THEN** the front end MUST call the same existing Refit/service operation
  with its current contract
- **AND** no journey-only endpoint or DTO field MAY be required for success

## ADDED Requirements

### Requirement: Mock mode mirrors reorganized journey semantics

Mock mode MUST continue to implement the same front-end service interfaces and
representative public/authenticated response states used by the reorganized
player, team, profile, tournament, sponsor, and organizer journeys.

#### Scenario: Mock action is selected from a new entry point

- **WHEN** a mock user reaches an existing registration, team, profile, sponsor,
  or organizer action through the reorganized UI
- **THEN** the mock service MUST receive the same logical operation and return
  the same success, blocked, unauthorized, conflict, or error semantics
- **AND** the UI MUST not require live-only fields or endpoints
