# global-search Specification

## Purpose
TBD - created by archiving change add-global-menu-search-public-profiles. Update Purpose after archive.
## Requirements
### Requirement: Global search uses trimmed prefix matching
The system SHALL only search users, teams, and games/tournaments when the trimmed query contains at least 3 characters, and matching SHALL be case-insensitive starts-with matching from the beginning of each searchable display name.

#### Scenario: Fewer than 3 characters do not search
- **WHEN** a visitor enters a search value whose trimmed length is less than 3 characters
- **THEN** the system does not perform a global search lookup
- **AND** no user, team, or game results are returned for that value

#### Scenario: Three or more characters return prefix matches
- **WHEN** a visitor enters a search value whose trimmed length is at least 3 characters
- **THEN** users with usernames starting with the query are eligible search results regardless of case
- **AND** teams with names starting with the query are eligible search results regardless of case
- **AND** games or tournaments with names starting with the query are eligible search results regardless of case
- **AND** users, teams, and games whose searchable names do not start with the query are not returned

### Requirement: Global search returns normalized safe result records
The global search API SHALL return normalized search result records that identify the result type and provide only the data needed to render a result and navigate to its public destination.

#### Scenario: Search response includes every supported result type
- **WHEN** a search query matches at least one user, one team, and one game
- **THEN** the response includes result records whose type identifies `user`, `team`, or `game`
- **AND** each record includes a display label suitable for the dropdown
- **AND** each user result includes the username needed for `/users/{username}`
- **AND** each team result includes the team name needed for `/teams/{teamname}`
- **AND** each game result includes the game id needed for `/games/{gameId}`

#### Scenario: Search response excludes private user and team fields
- **WHEN** the global search response includes user or team matches
- **THEN** user results do not include email, first name, last name, roles, timestamps, deletion state, or admin/internal fields
- **AND** team results do not include team invites or private member/account fields

### Requirement: Global search failures are representable to consumers
The global search service SHALL make loading, empty, and error outcomes distinguishable so the navigation dropdown can render the correct state.

#### Scenario: Search is pending
- **WHEN** a valid debounced search query is in flight
- **THEN** the consumer can identify that results are loading

#### Scenario: Search completes with no matches
- **WHEN** a valid search query completes without matching users, teams, or games
- **THEN** the consumer can identify the empty result state

#### Scenario: Search fails
- **WHEN** a valid search query fails because the search service or API cannot complete the request
- **THEN** the consumer can identify the error state without treating stale previous results as current matches
