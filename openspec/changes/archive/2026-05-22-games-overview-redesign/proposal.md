## Why

The games overview page is functionally useful, but its search and filtering controls still feel visually fragmented because the controls stack as isolated pieces instead of reading as one cohesive browsing surface. At the same time, the current games overview hero/intro block, extra context callout, and duplicate sponsor scroller add visual weight without improving how visitors find tournaments or orient themselves on the page.

## What Changes

- Redesign the games overview browsing controls so search, sort, and filter actions feel like one cohesive interface instead of separate stacked elements.
- Remove the games overview hero/intro treatment so the page opens directly with useful browsing controls and tournament content.
- Remove the `Need more context?` section from the games overview so the page stays focused on tournaments and supporting sponsor content.
- Replace the games overview sponsor scroller with the same reusable sponsor marquee treatment used on the home page so sponsor presentation is visually consistent across public pages.

## Capabilities

### New Capabilities
- `games-overview-browse-experience`: Defines the public games overview browsing experience, including cohesive search and filter presentation, removal of the top hero/intro treatment and redundant context callouts, and consistent sponsor showcase treatment.

### Modified Capabilities
- `page-hero-experience`: Updates the games overview entry experience so the page no longer depends on a dedicated hero/intro block ahead of the browsing interface.

## Impact

- Affected code: `src/Mercurius.LAN.Web/Components/Pages/Games/GamesOverview.razor`, `src/Mercurius.LAN.Web/Components/Pages/Games/GamesOverview.razor.css`, and shared sponsor scroller components under `src/Mercurius.LAN.Web/Components/Shared/`, plus any home page sponsor markup refactored into the reusable scroller component.
- Affected UX: public visitors browsing tournaments, sponsor visibility on the games overview, and the way the games page opens before visitors reach the tournament grid.
- Dependencies: existing Blazor components, shared CSS/Tailwind utility usage, and existing sponsor/game data services only; no new external dependencies expected.
