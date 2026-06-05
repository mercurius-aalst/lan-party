## ADDED Requirements

### Requirement: Public participant data excludes private account fields
The front-end SHALL model public game and team participant response data with DTOs that exclude private account/internal fields such as email, email verification state, Auth0 ID, roles, deletion state, and timestamps while allowing public profile fields such as first name, last name, Discord ID, Steam ID, and Riot ID when returned by the public API.

#### Scenario: Anonymous game response contains individual participants
- **WHEN** the front-end deserializes an anonymous public game detail response with individual participants
- **THEN** each participant is represented without email, email verification state, Auth0 ID, roles, deletion state, or timestamps
- **AND** the participant can provide an ID, username, first name, last name, display label, Discord ID, Steam ID, and Riot ID when those values are returned by the public API

#### Scenario: Anonymous team response contains members
- **WHEN** the front-end deserializes an anonymous public team or team participant response with members
- **THEN** each member is represented without email, email verification state, Auth0 ID, roles, deletion state, or timestamps
- **AND** member usernames, first names, last names, linked identities, and captain identity are represented when returned by the public API

### Requirement: Public participant data exposes returned public profile fields
The front-end SHALL treat first name, last name, Discord ID, Steam ID, and Riot ID in public participant data as public profile fields that may be rendered when the loaded public API response includes them.

#### Scenario: Public profile fields are present in public data
- **WHEN** a public game or team response includes first name, last name, Discord ID, Steam ID, or Riot ID fields for a participant
- **THEN** the front-end may render those returned public profile fields in participant displays
- **AND** the front-end does not infer or fetch missing public profile fields from current-user, admin-user, or full team endpoints

#### Scenario: Public profile fields are absent from public data
- **WHEN** a public participant response omits first name, last name, Discord ID, Steam ID, or Riot ID fields
- **THEN** the front-end omits those rows, labels, chips, or placeholders from public participant displays
- **AND** the front-end does not render copy implying that missing public profile fields are hidden or withheld

### Requirement: Authorized participant data remains separate from public participant data
The front-end SHALL keep authorized admin/current-user participant models and service flows separate from privacy-safe public participant models.

#### Scenario: Admin adds participants to a game
- **WHEN** an authorized admin opens an add-participant workflow
- **THEN** the workflow may use authorized user or team endpoints that return full details needed for administration
- **AND** those full details are not reused as the source model for anonymous public participant displays

#### Scenario: Current user manages private account data
- **WHEN** a signed-in user views or edits their own profile data
- **THEN** the front-end may use current-user DTOs with private fields
- **AND** those DTOs are not serialized into public game, bracket, placement, or team participant rendering state

### Requirement: Public participant lookup uses loaded response data efficiently
The front-end SHALL resolve public participant display labels and links from the already loaded game or team response data without additional per-participant API calls.

#### Scenario: Bracket renders participant names
- **WHEN** a public bracket renders match participant rows
- **THEN** participant labels are resolved from loaded game participant collections or match payload data
- **AND** no user, team, admin, current-user, or public profile endpoint is called once per participant to render the bracket

#### Scenario: Placements render participant names
- **WHEN** public placements or results render completed tournament rankings
- **THEN** participant labels are resolved from the loaded placement or game participant data
- **AND** missing optional fields are omitted cleanly without triggering enrichment calls
