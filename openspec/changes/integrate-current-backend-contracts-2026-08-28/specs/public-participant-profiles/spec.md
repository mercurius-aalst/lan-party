## ADDED Requirements

### Requirement: Public profile services use the current public resource contracts
Public participant pages SHALL load public user and team profiles through the versioned public
resources and SHALL keep private current-user and admin resources out of anonymous profile flows.

#### Scenario: Public user profile is loaded
- **WHEN** a visitor opens `/users/{username}`
- **THEN** the front-end requests `/v1/lan/public/users/{username}`
- **AND** it renders only the privacy-safe public user fields returned by that resource

#### Scenario: Public team profile is loaded
- **WHEN** a visitor opens `/teams/{teamName}`
- **THEN** the front-end requests `/v1/lan/public/teams/{teamName}`
- **AND** it renders only the returned team name, public members, captain identity, logo, and
  participating tournament records
- **AND** it does not call authenticated team summary, invite, or admin endpoints to enrich the page

### Requirement: Public team tournament links use canonical tournament identifiers
Public team profile surfaces SHALL map participating tournament records through `TournamentId` and
use the canonical tournament-detail presentation route.

#### Scenario: Team lists a participating tournament
- **WHEN** a public team response contains a tournament record
- **THEN** the UI displays its returned tournament name
- **AND** the link targets `/tournaments/{TournamentId}`
- **AND** the UI does not expect a legacy `GameId` field or issue a tournament lookup per row

#### Scenario: Tournament identifier is unavailable
- **WHEN** a participating tournament record has no usable public tournament identifier
- **THEN** the UI omits the broken link while retaining any safe display label
- **AND** it does not substitute a private or inferred identifier
