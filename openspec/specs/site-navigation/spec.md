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
