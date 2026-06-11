# site-navigation Specification

## Purpose
Defines the application's primary site-navigation behavior, including how routed header items participate in Blazor-native navigation, how active-state feedback is presented, and how temporary navigation surfaces close after selection.
## Requirements
### Requirement: Route-based site navigation uses Blazor-native navigation behavior
The site SHALL render internal application destinations in the header navigation with Blazor-native route navigation behavior instead of mixing comparable route items across different link mechanisms.

#### Scenario: Visitor selects a top-level routed menu item
- **WHEN** a visitor selects a primary navigation item that targets a routed page such as Home, Games, or Sponsors
- **THEN** the application navigates using Blazor route navigation behavior
- **AND** the selected item behaves consistently with other routed items in the same navigation group

#### Scenario: Visitor selects a routed destination from a navigation menu
- **WHEN** a visitor selects an internal routed destination from a header-associated menu such as an admin or profile menu
- **THEN** that destination uses the same Blazor-native navigation behavior as other internal routed navigation items
- **AND** the navigation item does not rely on plain anchor navigation semantics for equivalent routed destinations

### Requirement: Route-based navigation items expose consistent active-state feedback
The site SHALL apply consistent active-state feedback to route-based header navigation items based on the current routed page.

#### Scenario: Current page matches a top-level navigation route
- **WHEN** the visitor is on a routed page represented by a primary navigation item
- **THEN** that navigation item is shown with the active visual treatment
- **AND** sibling routed navigation items that do not match the current route are not shown as active

#### Scenario: Non-route items are not represented as active page routes
- **WHEN** the visitor uses a fragment link or action-style control in the header navigation
- **THEN** that item is not treated as the active routed page selection
- **AND** route-based active styling continues to represent the current page route rather than the fragment or action control

### Requirement: Header action buttons preserve clear action hierarchy
The site SHALL style supporting header actions such as login, user, and admin controls as lower-emphasis utility actions while preserving the tickets action as the visually dominant header call to action.

#### Scenario: Visitor views signed-out header actions
- **WHEN** a visitor views the header while signed out
- **THEN** the login action is presented as a supporting utility control
- **AND** the tickets action remains more visually prominent than the login action

#### Scenario: Visitor views authenticated header actions
- **WHEN** an authenticated visitor views the header with the user menu trigger and tickets action
- **THEN** the user menu trigger is presented as a supporting utility control
- **AND** the tickets action remains the most visually prominent call to action in the header

### Requirement: Open header menu triggers use stable selected styling
The site SHALL apply explicit selected styling to open admin and user menu triggers without making them appear faded, washed out, or inactive.

#### Scenario: Visitor opens the user menu
- **WHEN** the user dropdown menu is open
- **THEN** the user menu trigger shows a selected treatment that is more prominent than its resting state
- **AND** the trigger does not rely on a faint translucent-only fill to indicate selection
- **AND** the selected state remains visually distinct from the tickets CTA treatment

#### Scenario: Visitor opens the admin menu
- **WHEN** the admin dropdown menu is open
- **THEN** the admin menu trigger uses the same selected-state language as other supporting header actions
- **AND** the open state remains visually distinct from the tickets CTA treatment

### Requirement: Navigation selection preserves current menu-closing behavior
The site SHALL preserve the current behavior that closes temporary navigation surfaces after a navigation selection or action is made from the header navigation.

#### Scenario: Visitor selects an item while the mobile navigation panel is open
- **WHEN** a visitor selects a navigation item from the open mobile navigation panel
- **THEN** the panel closes after the selection is handled
- **AND** the visitor is not left with the navigation panel obscuring the destination page

#### Scenario: Visitor selects an item from an open dropdown menu
- **WHEN** a visitor selects a routed navigation item or supported action from an open admin or user dropdown
- **THEN** the dropdown closes after the selection is handled
- **AND** other temporary navigation overlays associated with that menu are dismissed

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

### Requirement: Authenticated user menu shows username
The site navigation SHALL identify an authenticated user-menu trigger by the user's Mercurius username when a current profile username is available.

#### Scenario: Current profile has username
- **WHEN** an authenticated visitor has a current user profile with username `sampleplayer`
- **THEN** the header user-menu trigger displays `sampleplayer`
- **AND** the trigger does not display the user's first and last name instead

#### Scenario: Username is unavailable
- **WHEN** the current user profile username cannot be loaded or is empty
- **THEN** the header user-menu trigger falls back to username-like identity claims when present
- **AND** otherwise keeps a non-blocking account label so navigation remains usable

### Requirement: Authenticated navigation exposes team management and user notifications
The site navigation SHALL provide authenticated users with a discoverable route to their team management area and SHALL surface user-level notifications, including team invites and team-related updates, through a clear branded notification control with a count.

#### Scenario: Authenticated user has team management access
- **WHEN** an authenticated user opens the header or mobile navigation
- **THEN** navigation provides access to the team management area
- **AND** the team management destination uses Blazor-native route navigation behavior
- **AND** selecting it closes temporary navigation surfaces consistently with other routed destinations

#### Scenario: Authenticated user has notifications
- **WHEN** an authenticated user has one or more unread or actionable user notifications
- **THEN** navigation presents a notification bell action within the authenticated user widget
- **AND** the count is visible as a compact badge that does not obscure the bell icon in desktop and mobile navigation contexts where the notification control is shown
- **AND** opening the notification bell shows a compact list of user notifications separately from the profile, teams, and logout menu
- **AND** notification entries for team invites can navigate to the team management area or received invites section
- **AND** the notification control does not expose private notification details in public or anonymous navigation

#### Scenario: Notification actions fit inside the notification menu
- **WHEN** an authenticated user opens the notification menu with entries that have action buttons
- **THEN** the notification list is wide enough for the notification title, message, and available action buttons
- **AND** action buttons MUST NOT overlap notification title or message text
- **AND** long notification text wraps within the notification row instead of pushing controls out of the menu

#### Scenario: Account and notification menus close on outside click
- **WHEN** an authenticated user has the profile menu or notification menu open
- **AND** the user clicks outside the authenticated user widget and its open dropdown
- **THEN** the open profile or notification menu MUST close
- **AND** clicks inside the authenticated user widget or open dropdown MUST NOT close the menu before its selected action can run

#### Scenario: Admin navigation is rendered
- **WHEN** an authenticated admin opens admin navigation
- **THEN** admin team management is not shown as an admin navigation item
- **AND** removing the admin team item does not disrupt remaining admin navigation destinations

#### Scenario: Notification state changes
- **WHEN** an authenticated user accepts, declines, receives, reads, or otherwise resolves a user notification during the session
- **THEN** the notification control updates to reflect the current confirmed notification state
- **AND** stale notification counts or labels are not left visible after the notification state refreshes

#### Scenario: Team hub invite notification arrives
- **WHEN** an authenticated user receives a team SignalR hub notification for a pending team invite
- **THEN** the authenticated navigation updates its notification control after refreshing confirmed notification and invite state
- **AND** the notification does not expose private invite details to anonymous visitors or public navigation surfaces

