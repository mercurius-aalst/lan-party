## 1. Update Shared Button Hierarchy

- [x] 1.1 Refine the supporting button classes in `src/Mercurius.LAN.Web/wwwroot/app.css` so secondary and outline variants use stronger hover, active, and focus treatments without looking translucent.
- [x] 1.2 Verify the primary button treatment in `src/Mercurius.LAN.Web/wwwroot/app.css` still reads as the highest-emphasis shared CTA next to the updated supporting button styles.

## 2. Restyle Header Utility Actions

- [x] 2.1 Update `brand-utility-button` styling in `src/Mercurius.LAN.Web/Components/Layout/NavMenu.razor.css` so login, user, and admin controls match the stronger supporting-button visual language.
- [x] 2.2 Add explicit open/selected state styling for admin and user menu triggers in `src/Mercurius.LAN.Web/Components/Layout/NavMenu.razor.css` so open menus no longer rely on washed-out hover-like states.
- [x] 2.3 Adjust dropdown item state styling in `src/Mercurius.LAN.Web/Components/Layout/NavMenu.razor.css` as needed so supporting actions remain visually cohesive with the revised utility-button treatment.

## 3. Verify Menu CTA Hierarchy And Shared Reuse

- [x] 3.1 Verify in the header that `Get Tickets` remains visually more prominent than login, user, and admin actions in both signed-out and authenticated states.
- [x] 3.2 Spot-check other pages that use shared secondary or outline button classes to confirm the broader washed-out styling issue is improved without making supporting buttons look like primary CTAs.
