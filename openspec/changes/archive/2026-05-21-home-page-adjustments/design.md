## Context

The current home page hero renders the event window as a standalone chip above the LAN logo, followed by the logo, event title, event description, calls to action, stat cards, and a scroll cue. The bottom of the hero uses a static `::before` gradient overlay to bridge the dark full-viewport hero into the white homepage content.

This change keeps the existing hero architecture and visual direction intact while refining two areas: the event information hierarchy and the viewport handoff. The implementation should remain scoped to the home page Razor markup and component-scoped CSS.

## Goals / Non-Goals

**Goals:**

- Group the event date display with the rest of the hero information instead of placing it above the logo.
- Restyle the date display so it reads as part of the same visual system as the hero stat cards and supporting information.
- Remove the small event description while keeping the event title visible.
- Replace the static bottom fade with a subtle breathing treatment.
- Make the transition from the fade into the white content area softer.
- Animate the scroll prompt as a synchronized cue so the label, arrow, and highlight feel intentionally connected.
- Preserve responsive behavior and reduced-motion compatibility.

**Non-Goals:**

- Redesign the full home page or change the downstream sponsor, feature, games, schedule, ticket, or contact sections.
- Change how event dates are calculated from loaded games.
- Introduce new data models, services, routes, or external dependencies.
- Change the primary hero imagery system beyond what is needed for the fade handoff.

## Decisions

1. Keep the change in the existing home hero component and CSS.

   The requested behavior is presentational and already lives in `Home.razor` and `Home.razor.css`. Keeping the work there avoids new shared abstractions for a single-page visual refinement. Alternative considered: extracting a dedicated event summary component, but that would add structure without clear reuse.

2. Move the event date into the hero information cluster near the existing stat cards.

   The date should no longer precede the logo as an isolated chip. It should sit with related event information so visitors read the logo and event title first, then see date and event facts together. Alternative considered: placing the date beside the primary actions, but that competes with ticket and navigation actions instead of grouping it with informational content.

3. Treat the date display as a sibling style to the existing stat cards.

   The date should share the hero's glassy, high-contrast information styling while remaining readable for a longer text value such as a date range. This can be done with a dedicated class or modifier rather than forcing the date into the same fixed three-column stat grid. Alternative considered: reusing the chip style in a lower position, but the pill treatment would still feel separate from the information group.

4. Remove only the event description paragraph from the hero copy.

   The event title remains the main text statement. Removing the smaller supporting paragraph reduces clutter after the date moves into the information group, while keeping the hero's primary message intact. Alternative considered: shortening the description, but the requested adjustment is removal.

5. Animate the bottom fade through opacity and gradient-position or transform-like values that do not affect layout.

   The breathing effect should be atmospheric and slow, without moving the content or changing the hero's measured height. The existing pseudo-element is the right hook for this because it already owns the bottom viewport fade. Alternative considered: adding a new DOM element for the fade, but the pseudo-element keeps markup simpler.

6. Respect reduced-motion preferences.

   The breathing fade animation should be disabled under `prefers-reduced-motion: reduce`, alongside the existing hero image, glow, scroll, and sponsor marquee animations. The static softened gradient should remain in that mode.

7. Keep the scroll prompt animation synchronized as one cue.

   The "Explore the event" label, arrow, and halo highlight should share timing and visual center alignment. The parent prompt should own the vertical movement so the label and arrow travel together, while the arrow only varies emphasis. Alternative considered: independently animating the arrow, but that made the cue feel misaligned.

## Risks / Trade-offs

- Date range text may be wider than the existing stat card values on small screens -> Use responsive layout rules that allow the date treatment to span or stack without squeezing text.
- Removing the event description reduces explanatory context -> Keep the event title prominent and preserve clear calls to action immediately below it.
- Breathing fade could become distracting or make the handoff feel unstable -> Use a slow, low-amplitude animation and verify that the white transition remains soft at both animation endpoints.
- Pseudo-element animation support varies by property -> Prefer animating broadly supported properties such as opacity or background-position, and keep a good static gradient as the baseline.
- More visual blending at the bottom may overlap the first content band -> Check desktop and mobile viewport boundaries so the sponsor section remains legible and intentionally revealed.

## Migration Plan

No data or deployment migration is required. Implement as markup and CSS changes in the home page component, then verify desktop and mobile rendering plus reduced-motion behavior.

Rollback is limited to reverting the home page markup and CSS changes for the hero information grouping and fade overlay.

## Open Questions

None.
