## MODIFIED Requirements

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
