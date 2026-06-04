## ADDED Requirements

### Requirement: Global search uses the live public search contract
The system SHALL search users, teams, and games/tournaments through the live public search contract when the trimmed query contains at least 3 characters, and matching eligibility and ordering SHALL follow the back-end response.

#### Scenario: Fewer than 3 characters do not search
- **WHEN** a visitor enters a search value whose trimmed length is less than 3 characters
- **THEN** the system does not perform a global search lookup
- **AND** no user, team, or game results are returned for that value

#### Scenario: Three or more characters return live matches
- **WHEN** a visitor enters a search value whose trimmed length is at least 3 characters
- **THEN** users, teams, and games returned by the live search response are eligible search results
- **AND** result ordering follows the deterministic relevance order returned by the live search response
- **AND** result matching is case-insensitive according to the live search contract

## MODIFIED Requirements

### Requirement: Global search returns normalized safe result records
The global search API SHALL return a response wrapper that contains bounded normalized search result records and pagination metadata, and each result SHALL identify the result type and provide only the data needed to render a result and navigate to its public destination.

#### Scenario: Search response includes wrapper metadata
- **WHEN** a valid global search request completes
- **THEN** the response includes a `results` collection
- **AND** the response includes `nextCursor`
- **AND** the response includes `hasMore`

#### Scenario: Search response includes every supported result type
- **WHEN** a search query matches at least one user, one team, and one game
- **THEN** the response includes result records whose type identifies `user`, `team`, or `game`
- **AND** each record includes a display label suitable for the dropdown
- **AND** each record includes supporting text suitable for the dropdown
- **AND** each user result includes the username needed for `/users/{username}`
- **AND** each team result includes the team name needed for `/teams/{teamname}`
- **AND** each game result includes the game id needed for `/games/{gameId}`

#### Scenario: Search response excludes private user and team fields
- **WHEN** the global search response includes user or team matches
- **THEN** user results do not include email, first name, last name, roles, timestamps, deletion state, Auth0 identifiers, platform identifiers, or admin/internal fields
- **AND** team results do not include team invites or private member/account fields

## REMOVED Requirements

### Requirement: Global search uses trimmed prefix matching
**Reason**: The live back-end search contract supports bounded public search with deterministic relevance ordering and pagination metadata, but it does not promise prefix-only matching.
**Migration**: Use `Global search uses the live public search contract` for minimum query length, matching eligibility, and ordering expectations.
