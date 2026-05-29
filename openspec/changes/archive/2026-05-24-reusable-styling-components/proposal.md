## Why

Most of the site's visual styling still lives inside page-scoped CSS, so repeated surfaces such as non-homepage heroes, cards, buttons, and branded sections drift apart as pages evolve. This change is needed now to establish a reusable styling layer that keeps those patterns consistent across the app while still allowing each page to fine-tune layout, spacing, and positioning when needed.

## What Changes

- Introduce shared styling capabilities for recurring UI surfaces so sponsor cards, tournament cards, branded section shells, and the non-homepage hero treatment can be reused instead of redefined page by page.
- Define an override-friendly contract where pages keep the ability to adjust spacing, arrangement, and exact positioning through scoped CSS without forking the base visual treatment.
- Update the existing shared button styling contract so button variants are reused consistently across pages instead of diverging in local page styles.
- Align non-homepage lead or hero treatments, including the tournaments page lead, around a common reusable pattern rather than isolated one-off page CSS, while keeping the home page hero on its separate existing path.
- Keep the current user-facing UI visually unchanged so this change improves reuse and maintainability without introducing a deliberate redesign.

## Capabilities

### New Capabilities
- `shared-brand-surfaces`: Defines reusable styling primitives for sponsor display cards, tournament cards, and branded section shells, including supported page-level override hooks for layout and positioning adjustments.

### Modified Capabilities
- `button-visual-hierarchy`: Expands the shared button system so primary and supporting button treatments are reused consistently across pages while remaining safe to tune in scoped CSS for local layout needs.
- `page-hero-experience`: Refines non-homepage page-intro and hero treatments so pages can share the same visual foundation instead of maintaining separate lead-section styling implementations, without changing the distinct home page hero experience.

## Impact

- Affected code: shared frontend styles in `src/Mercurius.LAN.Web/wwwroot/app.css` and related shared component styling, plus page-scoped CSS and markup in surfaces such as `src/Mercurius.LAN.Web/Components/Pages/Games/GamesOverview.razor`, `src/Mercurius.LAN.Web/Components/Pages/Games/GamesOverview.razor.css`, `src/Mercurius.LAN.Web/Components/Pages/Sponsors.razor`, `src/Mercurius.LAN.Web/Components/Pages/Sponsors.razor.css`, and other non-home pages that currently restyle the same patterns independently.
- Affected UX: the user-facing UI should remain visually unchanged while non-homepage hero sections, sponsor cards, tournament cards, buttons, and branded content sections are moved onto a more reusable shared styling foundation, and the home page hero should remain distinct.
- Dependencies: existing Tailwind and shared CSS setup only; no new external dependencies expected.
