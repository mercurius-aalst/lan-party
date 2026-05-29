## Context

The current games overview page already has working search, sort, and filter logic, but the entry experience is split across three separate surfaces: a page intro block, a search/sort toolbar card, and a stacked filter section. That structure makes the top of the page feel heavier than necessary and weakens the visual cohesion of the browsing controls.

The page also ends with two separate sponsor-related treatments: a `Need more context?` callout and the legacy shared `SponsorScroller` component. Meanwhile, the home page already contains a richer sponsor marquee implementation with better panel structure, logo framing, masking, and link behavior, but that implementation lives directly inside `Home.razor` instead of in a reusable component.

This change touches multiple frontend surfaces:
- `GamesOverview.razor` and its CSS for the browse layout and hero/callout removal
- `Home.razor` and its CSS for sponsor marquee extraction only
- `Components/Shared/SponsorScroller*` for the reusable sponsor marquee implementation

No backend contracts, filtering rules, or game data models need to change.

## Goals / Non-Goals

**Goals:**

- Remove the separate games overview hero/intro treatment without losing a clear page heading
- Present search, sort, and filter controls as one cohesive browse surface
- Remove the extra games overview context callout
- Replace the legacy games sponsor scroller with the richer home-page marquee treatment
- Extract the home sponsor marquee into a reusable shared component that can be used by both pages
- Keep the change frontend-only and preserve the existing filter/search behavior

**Non-Goals:**

- Change how games are filtered, sorted, or registered for
- Redesign the rest of the home page or remove the home hero
- Introduce new sponsor or game APIs
- Create a large cross-page “hero” abstraction
- Change the detail page, bracket views, or admin editing flows

## Decisions

### Decision 1: Replace the games hero, toolbar, and standalone filter stack with a single browse shell

The games page should stop opening with a separate intro/hero section. Instead, the page will begin with one top-level browse shell that contains:
- the page heading
- short supporting copy if needed
- search and sort controls
- status and participation filter chips

This keeps the page identifiable as the games overview while removing the sense that visitors must scroll past a decorative top block before they can interact with the useful controls.

Alternative considered: keep the intro section and only restyle the filter controls.
Why not chosen: that would improve surface styling but would not address the user’s request to remove the hero or the underlying fragmentation at the top of the page.

### Decision 2: Extract only the sponsor marquee treatment into a reusable shared component

The reusable component should encapsulate the home-page marquee behavior itself: repeated logo loops, masked viewport, linked logo cards, animation, and responsive styling. The surrounding page-specific copy and section framing should remain owned by each page.

This keeps the reusable part aligned with the user request to reuse the home-page sponsor scroller, while avoiding an over-configured component that also tries to own every page’s heading, CTA, and panel composition.

Alternative considered: move the entire home sponsor section, including header copy and CTA, into one configurable shared section component.
Why not chosen: the reusable need is the marquee treatment, not necessarily identical page copy structure. Keeping the wrapper content page-owned makes the component easier to reuse without forcing both pages into the exact same sponsor panel layout.

### Decision 3: Make the shared sponsor marquee presentational and let pages own sponsor data loading

The reusable marquee component should receive a sponsor list as input rather than fetching sponsor data internally. `Home.razor` already loads sponsors for its existing event stats and sponsor section, so keeping data ownership at the page level avoids duplicate fetches. `GamesOverview.razor` should add sponsor loading alongside game loading and render the marquee only when sponsor data is available.

This also removes toast-driven side effects from the shared sponsor component and makes its rendering behavior easier to reason about and test.

Alternative considered: keep sponsor fetching inside the shared component and upgrade the current `SponsorScroller`.
Why not chosen: that approach would either duplicate sponsor requests on the home page or keep the shared component coupled to service injection, async lifecycle behavior, and toast notifications. For a reusable UI surface, a presentational contract is cleaner.

### Decision 4: Remove the extra games context callout rather than replacing it with another supporting card

The `Need more context?` section should be deleted outright. Any truly necessary orientation should live in the top browse shell or in the existing results/process sections. This keeps the page focused on tournament discovery and prevents sponsor-related messaging from being split across multiple areas.

Alternative considered: replace the context callout with a smaller informational strip.
Why not chosen: the user explicitly called out that the section should be removed, and the page already has enough supporting structure without inserting another transition block.

## Risks / Trade-offs

- [Removing the separate intro could make the games page feel abrupt] -> Keep the page heading and a concise line of supporting copy inside the new browse shell so visitors still get orientation without a hero block
- [Adding sponsor loading to the games page increases first-render work] -> Fetch games and sponsors in parallel and render the sponsor marquee independently from the main game grid
- [The shared sponsor component could become too home-specific] -> Limit the shared abstraction to marquee mechanics and logo presentation, while page-level wrappers keep ownership of copy and section framing
- [Visual cohesion could regress on smaller screens when controls are merged] -> Design the browse shell mobile-first so search, sort, and chip groups stack intentionally inside one card rather than relying on desktop-only alignment

## Migration Plan

- Refactor the richer sponsor marquee markup and styles into the shared sponsor component
- Update the home page to consume the shared marquee component while keeping its existing sponsor section wrapper and home hero intact
- Update the games overview page to load sponsors, remove the hero/intro and context callout, and replace the legacy scroller with the shared marquee treatment
- Remove now-unused legacy sponsor scroller logic and any games overview properties or styles that only existed for the removed hero/callout layout
- Verify responsive behavior, reduced-motion behavior, and graceful rendering when no sponsors are available

## Open Questions

None at the design level. The user direction is specific enough to proceed into specs without additional product decisions.
