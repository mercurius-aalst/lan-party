# public-participant-profiles Specification

## Purpose
TBD - created by archiving change add-global-menu-search-public-profiles. Update Purpose after archive.
## Requirements
### Requirement: Public user profile exposes privacy-scoped fields
The system SHALL provide a public `/users/{username:string}` route that renders the back-end public user profile response and never renders raw user account DTOs.

#### Scenario: Anonymous user profile shows public identity fields
- **WHEN** an anonymous visitor opens `/users/{username}` for a known complete user
- **THEN** the page shows the user's first name
- **AND** the page shows the user's last name
- **AND** the page shows the user's username
- **AND** the page shows Discord ID, Steam ID, and Riot ID when those values are returned by the public API
- **AND** the page does not show email, email verification state, Auth0 ID, roles, timestamps, deletion state, or admin/internal fields

#### Scenario: Authenticated user profile uses the same public response shape
- **WHEN** an authenticated visitor opens `/users/{username}` for a known complete user
- **THEN** the page shows the same public fields as an anonymous visitor
- **AND** the page does not call current-user, admin-user, or other private account endpoints to enrich the profile
- **AND** the page does not show email, email verification state, Auth0 ID, roles, timestamps, deletion state, or admin/internal fields

#### Scenario: Missing linked identities are omitted cleanly
- **WHEN** a visitor opens `/users/{username}` for a known complete user whose public API response omits one or more linked IDs
- **THEN** the page does not render rows, cards, placeholders, or labels for the missing linked IDs
- **AND** the remaining profile content stays visible and well-formed

#### Scenario: User profile uses focused branded layout
- **WHEN** a visitor opens `/users/{username}` for a known user
- **THEN** the page presents the profile as a branded participant profile surface
- **AND** the page keeps username visible as the route and navigation identifier
- **AND** the page does not render duplicated title/subtitle blocks
- **AND** the page does not render unrelated discover or visibility sections

#### Scenario: Unknown user shows branded not-found state
- **WHEN** a visitor opens `/users/{username}` and no public user exists for that username
- **THEN** the page renders a branded not-found state
- **AND** the state gives the visitor a way to recover to a known site destination
- **AND** no private lookup error details are exposed

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
