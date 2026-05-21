## Context

The current navigation menu in `NavMenu.razor` already uses Blazor `NavLink` for some top-level routes (`/`, `/games`, `/sponsors`) but still uses standard anchor tags for other menu items and dropdown destinations that are part of the same application. Those elements share the same visual classes, yet they participate in navigation differently: `NavLink` integrates with Blazor routing and active-state calculation, while plain anchors rely on standard browser navigation semantics.

This mixed approach makes the navigation harder to maintain because styling implies a shared behavior contract that the markup does not actually enforce. It also makes active-state behavior inconsistent across links that users perceive as belonging to the same navigation system.

## Goals / Non-Goals

**Goals:**
- Standardize route-based navigation items in the main menu around Blazor-native `NavLink` behavior.
- Keep the existing visual styling and menu interaction model, including sidebar closing and dropdown dismissal.
- Make active-state behavior predictable for internal application routes.
- Separate route navigation concerns from fragment jumps and action-style controls such as login and logout.

**Non-Goals:**
- Redesign the header, menu layout, or visual brand treatment.
- Change authentication flows, dropdown structure, or route structure.
- Introduce a broad navigation abstraction beyond what is needed for the current menu cleanup.
- Force fragment-only links such as home-page section jumps into the same active-state rules as page routes if Blazor routing does not model them cleanly.

## Decisions

### Use `NavLink` for internal route destinations

Internal destinations that correspond to routed Blazor pages will be rendered with `NavLink` rather than `<a>` so the framework owns active-state calculation and client-side navigation behavior.

Rationale:
- `NavLink` aligns markup with the intended SPA navigation behavior.
- It avoids subtle differences between visually identical menu items.
- It reduces the need for custom active-state logic in CSS or code-behind.

Alternatives considered:
- Keep mixed `<a>` and `NavLink` usage with shared classes. Rejected because it preserves the current inconsistency.
- Replace everything with plain anchors. Rejected because it would give up Blazor-native active-state behavior and route integration.

### Treat fragment jumps and non-navigation actions as a separate link category

Links that primarily jump to a document fragment such as `/#tickets` or trigger non-route behavior such as login/logout will remain separate from the route-based `NavLink` set unless their underlying destination model changes.

Rationale:
- Fragment navigation is not the same concern as route selection.
- Forcing section jumps into the same route-active model can produce misleading active states or awkward behavior when the user is already on the home page versus another route.
- Login and logout are action flows, not route-selection UI.

Alternatives considered:
- Convert all anchors in the navigation area to `NavLink`. Rejected because some items are not true page-route navigation targets.

### Preserve close-menu behavior through shared click handling

The existing `OnNavigationSelected` callback and dropdown-closing logic will remain the common mechanism for collapsing the mobile panel and open menus after a selection, regardless of whether the item is a `NavLink` or another supported control.

Rationale:
- It preserves current mobile behavior without coupling layout state to specific link types.
- It keeps the change focused on navigation semantics rather than broader layout refactoring.

Alternatives considered:
- Move all close behavior to `LocationChanged` only. Rejected because some menu interactions should close immediately even before route changes or when handling non-route actions.

## Risks / Trade-offs

- [Fragment links do not map cleanly to route-active behavior] -> Keep fragment-only items outside the route-based active-state contract unless requirements explicitly change.
- [Dropdown items may still behave inconsistently if some remain anchors] -> Apply the same route-vs-action distinction consistently across top-level and dropdown items.
- [Changing link components can affect event timing] -> Reuse the existing navigation click handler and verify mobile panel closing, dropdown dismissal, and auth-link flows after the markup change.

## Migration Plan

Update the navigation markup in `NavMenu.razor` so routed internal destinations use `NavLink`, keep fragment jumps and action flows in their appropriate control types, and verify that active classes and close-menu behavior still work in desktop and mobile layouts. Rollback is low risk because the change is isolated to navigation component markup and existing styling.

## Open Questions

- Should admin dropdown routes and profile routes also participate in the same active styling treatment, or is this change limited to the primary top-level navigation?
- Should any current fragment destinations become dedicated routed pages in the future, which would let them move into the `NavLink` category later?
