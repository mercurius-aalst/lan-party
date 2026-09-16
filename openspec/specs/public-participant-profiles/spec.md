# public-participant-profiles Specification

## Purpose
TBD - created by archiving change add-global-menu-search-public-profiles. Update Purpose after archive.
## Requirements
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

### Requirement: Public team profile exposes team name, members, captain label, and tournaments
The system SHALL provide a public `/teams/{teamname:string}` route that renders a privacy-safe team profile response with team name, members, captain identity, and participating tournaments while excluding invites.

#### Scenario: Team page is public and excludes invites
- **WHEN** an anonymous visitor opens `/teams/{teamname}` for a known team
- **THEN** the page shows the team name
- **AND** the page shows team members by public username
- **AND** the page does not show team invites
- **AND** the page does not require authentication

#### Scenario: Team page labels captain inline
- **WHEN** a visitor opens `/teams/{teamname}` for a known team that has a captain
- **THEN** the captain appears in the roster as a normal member
- **AND** the captain member includes a clean Captain label
- **AND** the page does not render the captain as a separate roster row or standalone captain entry

#### Scenario: Team page lists participating tournaments
- **WHEN** a visitor opens `/teams/{teamname}` for a known team that is participating in tournaments
- **THEN** the page shows a "Playing in" section
- **AND** each listed tournament links to its game detail page

#### Scenario: Team member click navigates to public user profile
- **WHEN** a visitor selects a team member on a public team page
- **THEN** the application navigates to `/users/{username}` for that member

#### Scenario: Team profile uses focused branded layout
- **WHEN** a visitor opens `/teams/{teamname}` for a known team
- **THEN** the page presents the profile as branded roster and tournament surfaces
- **AND** the page does not render duplicated title/subtitle blocks
- **AND** the page does not render unrelated discover or visibility sections

#### Scenario: Unknown team shows branded not-found state
- **WHEN** a visitor opens `/teams/{teamname}` and no public team exists for that team name
- **THEN** the page renders a branded not-found state
- **AND** the state gives the visitor a way to recover to a known site destination
- **AND** no private lookup error details are exposed

### Requirement: Tournament participant popups link to public profiles
The system SHALL make tournament participant popup usernames navigable to their public user profiles when a public username is available.

#### Scenario: Individual participant username opens public profile
- **WHEN** a visitor opens a participant popup for an individual participant with a public username
- **THEN** the username is rendered as a link to `/users/{username}`

#### Scenario: Team participant member usernames open public profiles
- **WHEN** a visitor opens a participant popup for a team participant
- **THEN** each member with a public username is rendered as a link to `/users/{username}`

#### Scenario: Team captain is labeled inline in participant popup
- **WHEN** a visitor opens a participant popup for a team participant that has a captain
- **THEN** the captain appears in the member list with a Captain label
- **AND** no separate captain row or duplicate captain entry is rendered

### Requirement: Participant surfaces link only returned public identifiers
Tournament participant surfaces SHALL link to public user and team profile routes only when the loaded public participant data includes the required public route identifier.

#### Scenario: Public username is present
- **WHEN** a public participant surface renders an individual participant or team member with a returned username
- **THEN** that username links to `/users/{username}`
- **AND** the surface does not call a profile, current-user, or admin endpoint to discover a missing username

#### Scenario: Public username is missing
- **WHEN** a public participant surface renders an individual participant or team member without a returned username
- **THEN** the surface renders the available public display label without a user profile link
- **AND** the surface does not show private-field placeholders or hidden-field copy

#### Scenario: Public team name is present
- **WHEN** a public participant surface renders a team participant with a returned team name
- **THEN** the team name links to `/teams/{teamName}`
- **AND** the team member list continues to use only the member data returned in the loaded response

#### Scenario: Public team name is missing
- **WHEN** a public participant surface renders a team participant without a returned team route name
- **THEN** the surface renders the available public team display label without a team profile link
- **AND** the surface does not call a team profile or admin team endpoint to discover a missing route name

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
