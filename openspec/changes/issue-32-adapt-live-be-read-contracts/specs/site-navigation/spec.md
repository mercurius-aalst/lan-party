## MODIFIED Requirements

### Requirement: Header navigation provides global search results
The site navigation SHALL include a global search input that renders matching users, teams, and games/tournaments from the live search response `results` collection in a dropdown without disrupting the existing primary, Info, Admin, User, and ticket navigation controls.

#### Scenario: Fewer than 3 characters show no results
- **WHEN** a visitor enters a search value whose trimmed length is less than 3 characters in the header search box
- **THEN** the navigation does not display user, team, or game result rows
- **AND** the navigation does not show a stale result dropdown from a previous query

#### Scenario: Search dropdown shows loading state
- **WHEN** a visitor enters a valid search query and the debounced request is in flight
- **THEN** the search dropdown shows a loading state

#### Scenario: Search dropdown shows empty state
- **WHEN** a visitor enters a valid search query and the live search response contains no `results`
- **THEN** the search dropdown shows an empty state

#### Scenario: Search dropdown shows error state
- **WHEN** a visitor enters a valid search query and the search request fails
- **THEN** the search dropdown shows an error state
- **AND** the navigation remains usable

#### Scenario: Each search result type is visually identified
- **WHEN** the search dropdown renders user, team, and game results from the live response
- **THEN** each result row clearly identifies whether it is a user, team, or game with a visible icon or label
- **AND** the type marker is available to assistive technologies through text or accessible labeling

#### Scenario: Wrapped search metadata does not disrupt compact dropdown rendering
- **WHEN** the live search response includes `nextCursor` or indicates more results are available
- **THEN** the header dropdown renders the bounded current `results` collection
- **AND** the dropdown remains usable without requiring visitors to page through additional results
