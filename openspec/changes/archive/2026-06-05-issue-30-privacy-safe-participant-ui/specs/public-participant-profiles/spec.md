## ADDED Requirements

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
