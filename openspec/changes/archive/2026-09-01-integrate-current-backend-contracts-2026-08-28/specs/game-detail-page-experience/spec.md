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
