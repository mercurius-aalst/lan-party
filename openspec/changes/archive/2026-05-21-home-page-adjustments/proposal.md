## Why

The home page hero has a few visual details that feel separate from the rest of the composition: the event date display sits above the logo instead of with the supporting information, and the bottom viewport fade ends too statically and abruptly. Tightening these elements will make the first viewport feel more cohesive and polished.

## What Changes

- Move the event date display widget into the grouped information area instead of presenting it above the logo.
- Restyle the event date display so it feels consistent with the surrounding home page information elements.
- Remove the small event description while keeping the event title text.
- Add a subtle breathing effect to the bottom viewport fade.
- Soften the transition from the animated fade into the hard white page background.
- Make the "Explore the event" prompt feel like a clear, synchronized scroll invitation.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `page-hero-experience`: Refine the home page hero's event information grouping and the visual handoff between the first viewport and following content.

## Impact

- Affects the home page hero layout and styling in the web frontend.
- May require updates to component-scoped CSS, Tailwind utility usage, or animation styles for the hero fade treatment.
- Should preserve reduced-motion compatibility for the breathing fade effect.
