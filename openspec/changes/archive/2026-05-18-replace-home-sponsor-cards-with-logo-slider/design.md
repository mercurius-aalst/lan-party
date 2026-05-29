## Context

The home page currently renders sponsors as tier-based card groups directly in `Home.razor`. There is an older `SponsorScroller` shared component in the codebase, but it is not suitable for the new homepage display and should not be reused.

## Goals / Non-Goals

**Goals:**

- Replace the homepage tiered sponsor card layout with a new animated infinite logo slider
- Keep the homepage sponsor section visually lighter and more compact
- Build the new slider specifically for the homepage experience
- Keep the homepage link to the full sponsors page

**Non-Goals:**

- Reuse or adapt the existing `SponsorScroller`
- Redesign the dedicated sponsors page
- Change sponsor data contracts or backend behavior
- Add tier-specific presentation back into the homepage sponsor area

## Decisions

### Decision 1: Build a new homepage-specific slider from scratch

The new sponsor display will be implemented specifically for the homepage rather than reusing the existing shared scroller. This avoids inheriting limitations from the old component and lets the new markup, animation, and responsiveness match the current homepage design.

### Decision 2: Use homepage-loaded sponsor data

The home page already loads sponsors, so the new slider should render from that existing data instead of issuing a second sponsor fetch. This keeps data flow simple and avoids redundant loading logic.

### Decision 3: Make the slider logo-first and continuously animated

The homepage sponsor area should focus on continuously animated clickable logos with accessible labeling. Tier-specific cards, descriptions, and detailed grouping will be removed from the homepage and remain available through the full sponsors page.
