## Context

The current frontend already has a shared styling layer in `src/Mercurius.LAN.Web/wwwroot/app.css`, but many higher-value visual surfaces still define most of their appearance inside page-scoped `.razor.css` files. That has produced repeated patterns with slightly different gradients, spacing, radii, and interaction states across pages such as:

- non-homepage hero or lead sections like the tournaments lead and info page hero
- sponsor display cards on the sponsors page
- tournament cards on the games overview page
- branded section shells and content sections
- buttons that are nominally shared but still get page-local drift

The user wants those surfaces to become reusable styling-wise across pages, but not frozen. Each page still needs room to tune exact spacing, layout, and positioning in scoped CSS without copying the whole visual treatment again. The home page hero is explicitly out of scope for reuse and should remain its own distinct experience.

This is a frontend-only change. No backend contracts, data models, or service boundaries need to change.

## Goals / Non-Goals

**Goals:**

- Move recurring visual treatments for non-homepage heroes, sponsor cards, tournament cards, branded sections, and shared buttons into a reusable shared styling layer
- Preserve the current user-facing UI so the change behaves as a styling reuse refactor rather than a visible redesign
- Preserve page-level control over composition details such as grid placement, alignment, spacing, and optional supporting blocks
- Reduce style duplication between page-scoped stylesheets without forcing all pages into identical markup
- Keep the shared system understandable in plain Razor and CSS, with minimal abstraction overhead
- Let pages adopt the shared surfaces incrementally instead of requiring a one-shot rewrite

**Non-Goals:**

- Reuse or redesign the home page hero
- Introduce intentional user-visible layout or styling changes to the affected pages
- Convert every repeated page surface into a Blazor component in this change
- Replace all scoped CSS with Tailwind utilities or a design-token system overhaul
- Normalize every page to identical copy structure, markup density, or supporting content
- Change tournament, sponsor, or registration behavior

## Decisions

### Decision 1: Use shared CSS surface classes in `app.css` as the primary reuse mechanism

The reusable contract should live first in shared CSS classes, not in page-specific copied selectors. `src/Mercurius.LAN.Web/wwwroot/app.css` is already the shared styling entry point for cards, buttons, and section primitives, so the new reusable surfaces should extend that layer with new base classes for:

- non-homepage hero surfaces
- sponsor-card surfaces
- tournament-card surfaces
- branded section shells
- button variants and state styles

Pages will adopt those classes in Razor markup and keep their scoped CSS focused on layout adjustments or local modifiers.

Alternative considered: extract each repeated surface into a dedicated Blazor component first.
Why not chosen: the user’s request is primarily about styling reuse and override flexibility. Starting with shared CSS avoids over-coupling markup and data flow when some surfaces are still page-specific in structure.

### Decision 2: Keep page markup page-owned and compose reusable visuals with base plus local classes

Pages should not lose ownership of their semantic structure. Instead of requiring a rigid shared component tree, each page will combine a shared base class with page-local classes, for example a shared hero class plus a page-specific layout class.

That means the reusable layer owns:
- background treatment
- border and radius system
- shadow language
- typography hierarchy for shared surface roles
- shared interactive states

And page-scoped CSS continues to own:
- exact grid template decisions
- local ordering and placement
- optional extra spacing rules
- page-only decorative adjustments

Alternative considered: enforce one canonical markup structure for every hero or card type.
Why not chosen: the sponsor page, tournaments page, and info page do not all need identical internal composition. Forcing identical markup would make reuse brittle and likely encourage future forks.

### Decision 3: Support overrides through stable custom properties and low-specificity base selectors

The reusable styling layer should be designed to be overridden intentionally rather than accidentally. Shared surface classes should expose a small set of CSS custom properties for values that are likely to vary by page, such as:

- outer padding
- internal gap
- accent glow or background intensity
- image or media height
- content width limits
- section spacing

Base selectors should remain low-specificity so page-scoped `.razor.css` files can override these values without `!important` or selector escalation. The preferred override path is:

1. page sets surface-scoped custom property values
2. page adds local layout rules around the shared class
3. page adds narrow local modifiers only when variables are not enough

Alternative considered: rely on more specific page selectors to restyle shared classes as needed.
Why not chosen: that would recreate the current drift problem and make reuse harder to reason about over time.

### Decision 4: Separate reusable surface categories instead of creating one generic “mega surface” abstraction

The design should create a few purposeful reusable categories rather than one generic class system that tries to style every card or section in the app. The shared categories should map to actual repeated UI types:

- non-homepage page hero
- sponsor display card
- tournament display card
- brand section or section shell
- button

This keeps the design legible and lets specs describe behavior at a capability level that matches the user’s request.

Alternative considered: create one generic card or panel abstraction and let every page derive its own patterns from that.
Why not chosen: the current problem is not a lack of a generic box primitive. It is drift in a handful of recognizable surface families that need stronger shared defaults.

### Decision 5: Reuse existing global primitives where possible and layer new surfaces on top of them

The new shared surfaces should build on existing global primitives such as `.brand-card`, `.brand-section`, `.brand-panel-dark`, `.button-primary`, and `.button-secondary` rather than replacing them wholesale. The change should evolve the current system by:

- promoting recurring page patterns into named shared surfaces
- tightening button state consistency
- adding optional modifier classes where a family has clear variants
- avoiding breaking older pages that still depend on today’s primitives

Alternative considered: rewrite the current shared CSS around a brand-new naming system and migrate all pages immediately.
Why not chosen: that would increase migration risk and expand the change beyond the user’s reusable styling goal.

### Decision 6: Migrate page-by-page, starting with the requested surfaces

Implementation should target the surfaces explicitly called out in the proposal:

- tournaments page lead or hero
- sponsor display cards
- tournament cards on the tournaments page
- shared buttons
- section styling

Other pages that already resemble those patterns, such as the info page’s hero and branded sections, can adopt the shared classes as part of the same change where it is low-risk, but they should not force a broader redesign.

The migration target is equivalence in the rendered UI. Shared classes may replace page-local styling definitions, but the resulting interface should remain materially the same to visitors unless a small technical adjustment is required to preserve consistency or responsiveness.

Alternative considered: postpone adoption and only add new shared classes without using them yet.
Why not chosen: the change would not prove the shared contract works until at least the target pages consume it.

## Risks / Trade-offs

- [Shared classes may become too opinionated for pages with different content density] -> Keep the base surfaces focused on visual language and expose page-level variables for spacing, media sizing, and layout
- [Low-specificity overrides can still become confusing if too many variables are exposed] -> Limit override hooks to a small, documented set per surface family and use local modifiers only for truly exceptional cases
- [Migrating markup to shared classes may create temporary mixed patterns during rollout] -> Migrate one surface family at a time and verify the target pages before removing old page-local styling blocks
- [Extending existing global classes could unintentionally affect older screens] -> Add new surface classes alongside current primitives first, then opt pages into them explicitly instead of broadening legacy selectors
- [Hero reuse could accidentally spill into the home page] -> Keep the shared hero naming and adoption limited to non-home pages and avoid attaching the new classes to `Home.razor`
- [Refactoring shared styling could introduce subtle visual regressions even without intended redesign] -> Compare affected pages before and after migration and treat visual parity as a validation requirement for the change

## Migration Plan

- Add shared surface classes and any supporting custom properties to `src/Mercurius.LAN.Web/wwwroot/app.css`
- Update the requested pages to use the new shared classes in markup while keeping page-local wrapper classes where layout tuning is needed
- Reduce duplicated visual rules in page-scoped CSS so those files keep only page-specific overrides and positioning logic
- Verify that the home page hero has not adopted the non-homepage hero surface accidentally
- Review responsive behavior, button states, card alignment, and visual parity across the affected pages before removing now-unused duplicated selectors

## Open Questions

- Should the shared non-homepage hero surface cover both simple intro sections and split-layout hero shells, or should those become separate variants within the same family?
- Do we want sponsor-card and tournament-card variants expressed as modifier classes, CSS variables, or both when size differences are substantial?
