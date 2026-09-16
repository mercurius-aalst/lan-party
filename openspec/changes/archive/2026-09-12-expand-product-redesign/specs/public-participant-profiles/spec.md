## MODIFIED Requirements

### Requirement: Public user profile exposes privacy-scoped fields

The system SHALL provide a public `/users/{username:string}` route that renders
the back-end public user profile response, offers clear links to related public
team/tournament context when returned by existing projections, and never renders
raw user account DTOs.

#### Scenario: Anonymous user profile shows public identity fields

- **WHEN** an anonymous visitor opens `/users/{username}` for a known complete
  user
- **THEN** the page MUST show the user's first name, last name, and username
- **AND** it MUST show Discord ID, Steam ID, and Riot ID when those values are
  returned by the public API
- **AND** it MUST not show email, email verification state, Auth0 ID, roles,
  timestamps, deletion state, or admin/internal fields

#### Scenario: Authenticated user profile uses the same public response shape

- **WHEN** an authenticated visitor opens `/users/{username}` for a known
  complete user
- **THEN** the page MUST show the same public fields as an anonymous visitor
- **AND** it MUST not call current-user, admin-user, or other private account
  endpoints to enrich the profile
- **AND** it MUST not show email, email verification state, Auth0 ID, roles,
  timestamps, deletion state, or admin/internal fields

#### Scenario: Missing linked identities are omitted cleanly

- **WHEN** a visitor opens `/users/{username}` for a known complete user whose
  public API response omits one or more linked IDs
- **THEN** the page MUST not render rows, cards, placeholders, or labels for the
  missing linked IDs
- **AND** the remaining profile content MUST stay visible and well-formed

#### Scenario: User profile uses focused branded layout

- **WHEN** a visitor opens `/users/{username}` for a known user
- **THEN** the page MUST present the profile as a branded participant profile
  surface with a clear identity header and available next public context
- **AND** the page MUST keep username visible as the route and navigation
  identifier
- **AND** the page MUST not render duplicated title/subtitle blocks or unrelated
  discover/visibility sections

#### Scenario: Unknown user shows branded not-found state

- **WHEN** a visitor opens `/users/{username}` and no public user exists for that
  username
- **THEN** the page MUST render a branded not-found state
- **AND** the state MUST give the visitor a way to recover to a known site
  destination
- **AND** no private lookup error details MUST be exposed

## ADDED Requirements

### Requirement: Public profile journeys connect returned context

Public user and team profiles SHALL expose related tournament, team, and match
summary context only when the current public response provides it, using the
canonical existing routes and preserving omission behavior for unavailable data.

#### Scenario: Public profile has related match context

- **WHEN** a public profile response includes a previous or upcoming match
  summary
- **THEN** the page MUST identify the tournament and match context and link to
  the canonical tournament detail destination when available
- **AND** it MUST not request private registration or account data to complete
  the summary

#### Scenario: Public profile has no related context

- **WHEN** the public response contains no related teams, tournaments, or match
  summaries
- **THEN** the profile MUST render a clear empty or omitted state
- **AND** the identity and recovery navigation MUST remain usable

### Requirement: Contextual user identities open privacy-safe information popups

User identities shown in contextual lists MUST open an information popup rather
than forcing navigation to a public profile route. This includes team
management, tournament participants, and tournament team lineups.

#### Scenario: User selects a contextual identity

- **WHEN** a visitor or authenticated player selects a user in one of those
  contextual lists
- **THEN** the popup MUST show returned first name, last name, username, and
  available public linked IDs only
- **AND** missing linked IDs MUST be omitted without placeholder text
- **AND** email, Auth0 ID, roles, timestamps, and other private fields MUST not
  be rendered
- **AND** the popup MUST not issue a private enrichment call or change the
  current route
