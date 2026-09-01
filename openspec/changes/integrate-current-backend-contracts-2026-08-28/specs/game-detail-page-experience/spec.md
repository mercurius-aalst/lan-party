## ADDED Requirements

### Requirement: Tournament detail consumes the canonical tournament detail contract
The tournament detail page SHALL load its public data from the versioned tournament detail resource and
map the response without inventing legacy game fields or follow-up enrichment requests.

#### Scenario: Tournament detail is loaded
- **WHEN** a visitor opens `/tournaments/{tournamentId}`
- **THEN** the front-end requests `/v1/lan/tournaments/{tournamentId}`
- **AND** the loaded response maps the backend tournament identifier, schedule, status, placements,
  matches, sponsor placement, and public registration projection
- **AND** the page preserves its loading, empty, not-found, unauthorized, and error states

#### Scenario: Detail response uses current public fields
- **WHEN** the tournament detail response contains an optional team size or singular sponsor
  placement
- **THEN** the page renders those values when present
- **AND** a missing sponsor placement does not create an empty sponsor surface
- **AND** the page does not require `RegisterFormUrl`, a legacy `GameId`, or a separate users or
  teams lookup to render the loaded public participant data

### Requirement: Tournament detail maps canonical match and registration identifiers
The tournament detail page SHALL use identifiers and active registration data from the current backend
response when rendering schedule, bracket, participant, and registration surfaces.

#### Scenario: Match data is rendered from tournament detail
- **WHEN** the detail response contains matches
- **THEN** each match is associated with the loaded tournament through `TournamentId`
- **AND** schedule and bracket surfaces reuse the loaded match data without issuing one API call per
  match solely for participant or timing enrichment

#### Scenario: Registration projection is rendered
- **WHEN** the detail response includes active public registrations
- **THEN** the participant surface renders the returned privacy-safe users, teams, and rosters
- **AND** inactive or private registrations are not presented as active public participants

### Requirement: Tournament detail loads only in the interactive circuit
The tournament detail page MUST avoid executing its tournament or sponsor data load during
prerender and MUST keep lifecycle and sponsor action busy state independent when operations overlap.

#### Scenario: Direct detail navigation is prerendered
- **WHEN** a visitor requests a tournament detail URL directly
- **THEN** the page does not issue tournament or sponsor data requests during prerender
- **AND** the interactive circuit performs the detail load once

#### Scenario: Lifecycle and sponsor actions overlap
- **WHEN** a lifecycle mutation and a sponsor mutation overlap or one fails while the other succeeds
- **THEN** each operation clears the busy state it owns
- **AND** a stale operation cannot disable the unrelated action surface

### Requirement: Sponsor administration is restricted to administrators
The detail page MUST load the sponsor administration collection only for an authenticated
administrator and MUST keep non-administrator detail loads free of that request.

#### Scenario: Non-administrator opens tournament detail
- **WHEN** an anonymous or non-administrator visitor loads a tournament detail
- **THEN** the page renders the public detail response without requesting the sponsor collection

#### Scenario: Administrator opens tournament detail
- **WHEN** an administrator loads a tournament detail with sponsor administration available
- **THEN** the page requests the sponsor collection for the administration selector
- **AND** a sponsor request failure does not hide the public tournament detail
