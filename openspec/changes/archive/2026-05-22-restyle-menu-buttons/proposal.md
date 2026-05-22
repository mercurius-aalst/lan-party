## Why

The menu's login and user buttons currently use hover and selected treatments that feel semi-transparent and visually weak compared with the rest of the site's branding. The same styling pattern appears on other non-primary buttons, so the change should tighten the broader button system while preserving a clearly stronger call-to-action treatment for `Get Tickets`.

## What Changes

- Refine the shared button styling system so utility and secondary buttons use more solid hover and selected treatments instead of washed-out translucent states.
- Define clearer visual hierarchy between primary calls to action and lower-emphasis utility actions.
- Update the header menu's login, user, and related utility controls to align with the shared button treatment and feel intentional next to the navigation links.
- Preserve a distinct, higher-emphasis `Get Tickets` treatment so it remains visually recognizable as the primary CTA in the menu.

## Capabilities

### New Capabilities
- `button-visual-hierarchy`: Defines shared visual treatments for primary, secondary, and utility-style buttons, including stronger hover and selected states and preserved CTA emphasis.

### Modified Capabilities
- `site-navigation`: Refines header action styling so login and user controls read as supporting navigation actions while the tickets action remains a distinct primary CTA.

## Impact

- Affected code: shared button styles in `src/Mercurius.LAN.Web/wwwroot/app.css`, header/menu styling in `src/Mercurius.LAN.Web/Components/Layout/NavMenu.razor.css`, and any Razor components that consume the shared button classes.
- Affected UX: button hover, selected, and emphasis hierarchy across the menu and other pages that use the same shared button styles.
- Dependencies: existing Tailwind and shared CSS setup only; no new external dependencies expected.
