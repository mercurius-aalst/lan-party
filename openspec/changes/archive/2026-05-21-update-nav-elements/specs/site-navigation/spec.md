## ADDED Requirements

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
