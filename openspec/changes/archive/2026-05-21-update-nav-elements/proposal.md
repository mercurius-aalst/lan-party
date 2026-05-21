## Why

The primary navigation currently mixes standard anchor elements with Blazor `NavLink` components that share the same styling but behave differently at runtime. This creates inconsistent active-state handling and navigation behavior in the main menu, which makes the navigation harder to reason about and maintain as the site UI evolves.

## What Changes

- Standardize the primary site navigation on Blazor-native navigation link behavior for internal menu destinations.
- Define consistent active-link behavior for navigation items that route within the application.
- Preserve the current visual styling of the navigation while removing behavior differences caused by mixing link types for comparable menu actions.
- Keep non-route actions and external-style targets scoped separately from route-based navigation behavior where needed.

## Capabilities

### New Capabilities
- `site-navigation`: Defines consistent behavior for route-based navigation items in the main site navigation, including active-state handling and Blazor-native navigation behavior.

### Modified Capabilities

## Impact

- Affected code: `src/Mercurius.LAN.Web/Components/Layout/NavMenu.razor`, related code-behind, and navigation styling.
- Affected UX: Primary navigation behavior, especially active-state indication and internal page transitions.
- Dependencies: Blazor routing and `NavLink` behavior already available in the web application; no new external dependencies expected.
