## Context

The current styling issue spans both shared button primitives in `src/Mercurius.LAN.Web/wwwroot/app.css` and menu-specific utility controls in `src/Mercurius.LAN.Web/Components/Layout/NavMenu.razor.css`. Secondary and utility treatments rely on low-opacity green hover fills such as `rgba(34, 197, 94, 0.08)`, which makes the login and user buttons feel washed out in the header and produces the same impression on other supporting buttons across the site.

The menu also needs a stronger visual hierarchy than "all buttons look alike". `Get Tickets` is a primary conversion action and should remain more prominent than login, user, or admin controls, which are supporting actions within the navigation shell.

## Goals / Non-Goals

**Goals:**
- Replace translucent-feeling hover and selected states for shared secondary and utility buttons with more solid, intentional visual treatments.
- Align header login, user, and admin actions with the shared button system so they no longer feel like a separate, weaker styling pattern.
- Preserve a clear visual distinction between the primary `Get Tickets` CTA and lower-emphasis utility actions.
- Keep the change compatible with the existing Tailwind-plus-shared-CSS setup and avoid introducing a full button-system rewrite.

**Non-Goals:**
- Redesigning the site's brand palette, typography, or navigation layout structure.
- Reclassifying every existing button into a brand-new component taxonomy.
- Changing navigation behavior, routing, or dropdown interaction logic beyond any state styling needed for active/open presentation.

## Decisions

### Decision: Strengthen shared supporting-button states in `app.css`

Shared supporting button classes (`.button-secondary`, `.btn-secondary`, `.btn-outline-primary`, `.btn-outline-secondary`) will move away from faint translucent hover fills toward a more solid treatment using stronger border contrast, a tinted or elevated surface, and hover/active box-shadow feedback. This keeps the interaction readable on bright backgrounds and avoids the "transparent" feel the user described.

Alternative considered: Leave shared button primitives alone and patch only the menu buttons locally. Rejected because the same styling issue is already visible on other pages, and a menu-only fix would preserve the inconsistency elsewhere.

### Decision: Keep menu utility buttons as a distinct supporting-action layer

`NavMenu.razor.css` will continue to own the menu-specific utility button presentation, but its styling should be brought into line with the shared supporting-button language. The header controls can use a denser, navigation-friendly version of the same visual logic: solid surface, stronger border, and explicit hover/open states rather than translucent fills.

This keeps the menu free to tune spacing and compactness without forcing every page button to inherit header-specific styling.

Alternative considered: Replace `brand-utility-button` with `button-secondary` directly in markup. Rejected because the menu buttons have different spacing, layout, and dropdown-trigger needs, and the header still benefits from a dedicated class layer.

### Decision: Preserve CTA separation by keeping `Get Tickets` on the primary button path

The `Get Tickets` action should remain on the existing primary CTA styling path (`.button-primary.brand-ticket-cta`) while login, user, and admin controls remain on a supporting-action path. If shared primary styles are tuned as part of the same pass, they should reinforce the contrast rather than converge with secondary styles.

Alternative considered: Normalize all header actions to the same button treatment for visual consistency. Rejected because it would weaken the action hierarchy and make the navigation CTA less scannable.

### Decision: Define selected/open feedback explicitly for utility and dropdown actions

The current problem is most visible on hover, but the same weak treatment can affect selected or open states. The implementation should define explicit styling for:
- hovered supporting buttons
- open dropdown triggers such as user/admin
- active dropdown items
- focus-visible states that remain accessible and visually consistent

This avoids relying on incidental hover styles as the only interaction signal.

Alternative considered: Only change hover colors. Rejected because the complaint covers both hover and selected states, and open menu triggers need a stable selected appearance.

## Risks / Trade-offs

- [Supporting buttons become too visually heavy] -> Mitigation: keep the stronger treatment within the existing brand palette and use contrast, border, and subtle shadow before increasing saturation too aggressively.
- [Primary and secondary buttons drift too close together] -> Mitigation: preserve a stronger fill treatment and higher contrast for `.button-primary`, and verify header CTA prominence specifically in the menu context.
- [Header-specific tweaks diverge from shared button behavior again over time] -> Mitigation: base menu utility styling on the same state logic as shared supporting buttons, with only layout-density adjustments kept local.
- [Broad shared-class changes create unintended regressions on other pages] -> Mitigation: limit shared changes to supporting button state styling and verify high-visibility pages that already use `button-secondary` or Bootstrap-mapped secondary classes.

## Migration Plan

1. Update shared supporting button styles in `app.css` to establish stronger hover, active, and focus treatments.
2. Adjust `brand-utility-button` and related dropdown item states in `NavMenu.razor.css` to match the revised supporting-button language.
3. Verify that `Get Tickets` remains visually dominant in the header next to login, user, and admin controls.
4. Spot-check other pages that use shared secondary button classes to confirm the site-wide issue improves without broad regressions.

Rollback is straightforward because the change is CSS-only: revert the shared and menu stylesheet edits if the new button hierarchy proves too heavy or inconsistent in review.

## Open Questions

- Should open dropdown triggers in the menu use the same visual treatment as active dropdown items, or a lighter "selected" state that still reads as a trigger rather than a destination?
- Are there any other button-like controls outside the menu that should be intentionally excluded from the shared supporting-button update because they rely on current Bootstrap semantics?
