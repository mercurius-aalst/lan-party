## ADDED Requirements

### Requirement: Header navigation provides global search results
The site navigation SHALL include a global search input that renders matching users, teams, and games/tournaments in a dropdown without disrupting the existing primary, Info, Admin, User, and ticket navigation controls.

#### Scenario: Fewer than 3 characters show no results
- **WHEN** a visitor enters a search value whose trimmed length is less than 3 characters in the header search box
- **THEN** the navigation does not display user, team, or game result rows
- **AND** the navigation does not show a stale result dropdown from a previous query

#### Scenario: Search dropdown shows loading state
- **WHEN** a visitor enters a valid search query and the debounced request is in flight
- **THEN** the search dropdown shows a loading state

#### Scenario: Search dropdown shows empty state
- **WHEN** a visitor enters a valid search query and no users, teams, or games match
- **THEN** the search dropdown shows an empty state

#### Scenario: Search dropdown shows error state
- **WHEN** a visitor enters a valid search query and the search request fails
- **THEN** the search dropdown shows an error state
- **AND** the navigation remains usable

#### Scenario: Each search result type is visually identified
- **WHEN** the search dropdown renders user, team, and game results
- **THEN** each result row clearly identifies whether it is a user, team, or game with a visible icon or label
- **AND** the type marker is available to assistive technologies through text or accessible labeling

### Requirement: Header search selection navigates to public destinations
The site navigation SHALL route selected search results to the public destination for their result type and close temporary navigation surfaces after selection.

#### Scenario: User result selection navigates to user profile
- **WHEN** a visitor selects a user search result with username `sample-player`
- **THEN** the application navigates to `/users/sample-player`
- **AND** the search dropdown closes

#### Scenario: Team result selection navigates to team profile
- **WHEN** a visitor selects a team search result with team name `Sample Team`
- **THEN** the application navigates to `/teams/Sample%20Team`
- **AND** the search dropdown closes

#### Scenario: Game result selection navigates to game detail
- **WHEN** a visitor selects a game search result
- **THEN** the application navigates to `/games/{gameId}` for that result
- **AND** the search dropdown closes

### Requirement: Header search supports keyboard operation and close behavior
The site navigation SHALL support keyboard interaction for search results and close the search dropdown through the same temporary-surface behavior as other header menus.

#### Scenario: Arrow keys move active search result
- **WHEN** the search dropdown contains results and the visitor presses ArrowDown or ArrowUp from the search input
- **THEN** the active result moves through the available results without leaving the dropdown unusable

#### Scenario: Enter activates highlighted result
- **WHEN** the search dropdown contains a highlighted result and the visitor presses Enter
- **THEN** the highlighted result is selected
- **AND** the application navigates to the result destination

#### Scenario: Escape closes search dropdown
- **WHEN** the search dropdown is open and the visitor presses Escape
- **THEN** the search dropdown closes
- **AND** the current search text remains editable unless the visitor clears it

#### Scenario: Outside click closes search dropdown
- **WHEN** the search dropdown is open and the visitor clicks outside the navigation search surface
- **THEN** the search dropdown closes
- **AND** existing Info, Admin, and User menu outside-click behavior still works
- **AND** clicks outside the search input and results are detected reliably even when the sticky header creates its own stacking context

#### Scenario: Navigation closes search dropdown
- **WHEN** the search dropdown is open and the visitor navigates through any header navigation item or search result
- **THEN** the search dropdown closes
- **AND** the parent mobile navigation close callback is invoked when applicable

#### Scenario: Clearing input closes search dropdown
- **WHEN** the visitor clears the search input or changes it to fewer than 3 trimmed characters
- **THEN** search results are cleared
- **AND** the search dropdown closes
