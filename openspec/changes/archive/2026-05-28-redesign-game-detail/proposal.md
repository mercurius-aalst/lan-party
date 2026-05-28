## Why

The game detail page currently repeats the same tournament information across the hero, tabbed details area, preview cards, and sidebar, which makes the page feel crowded without helping visitors complete their next action. Important content like participants, matches, and results should stand on their own instead of being nested inside a generic tournament details shell, while redundant banner metrics, extra sponsor blocks, and other low-value summary content are removed.

## What Changes

- Remove the tabbed `Tournament information` / `Details` section entirely and replace it with clearer standalone page sections where participants, matches, and results each own their own content.
- Split match schedule information and bracket structure into separate standalone sections so visitors can inspect timing and tournament progression independently.
- Redesign the game detail page so tournament information is no longer duplicated between the hero, standalone sections, and sidebar surfaces.
- Remove dashboard-style summary elements that do not help visitors act on the page, including the stat strip between the hero and sections and the sidebar `At a glance` summary.
- Simplify tournament partner presentation so a tournament detail page highlights at most one sponsor or partner instead of rendering multiple sponsor sections.
- Restyle the game detail hero so it follows the same visual language as the other page hero surfaces on the site instead of using a one-off `Tournament spotlight` treatment.

## Capabilities

### New Capabilities
- `game-detail-page-experience`: Defines the public tournament detail page experience, including removal of the tabbed details shell, clear standalone ownership for overview, match schedule, bracket, participants, and results content, removal of redundant summary surfaces, simplified single-partner presentation, and a hero treatment aligned with the rest of the site's page heroes.

### Modified Capabilities
None.

## Impact

- Affected code: `src/Mercurius.LAN.Web/Components/Pages/Games/GameDetail.razor`, `src/Mercurius.LAN.Web/Components/Pages/Games/GameDetail.razor.css`, and the supporting tab components under `src/Mercurius.LAN.Web/Components/Pages/Games/Tabs/`.
- Affected UX: public visitors viewing tournament details, navigating to participants and bracket information, and scanning sponsor or partner context on a game detail page.
- Affected systems: shared hero styling and tournament sponsor/partner presentation, with possible follow-on changes to game sponsor placement management if the single-partner rule is enforced beyond presentation.
- Dependencies: existing Blazor page components, shared site styling, current game detail data models, and existing sponsor/game service flows; no new external dependencies expected.
