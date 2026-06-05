## Why

The back-end public tournament and team responses are moving away from full user/member DTOs so anonymous pages cannot accidentally expose private account or internal fields such as email, Auth0 IDs, roles, verification state, or timestamps. The front-end needs an explicit privacy-safe participant contract now so game detail, brackets, placements, match dialogs, and team chips continue to work against the real API while still rendering public profile fields such as first name, last name, Discord, Steam, and Riot when those values are returned.

## What Changes

- Introduce distinct front-end participant data shapes for public participant responses and authorized admin/current-user responses with full private account details.
- Update public tournament participant rendering to use only the already loaded game/team response data, including participant lists, bracket entries, match detail dialogs, placements/results, and team member chips.
- Preserve useful public participant display with username, first name, last name, linked identities, display-label-only fallback data, links to `/users/{username}` and `/teams/{teamName}` when present, and clean omission of missing optional fields.
- Prevent public pages from displaying private account/internal fields such as email, email verification state, Auth0 ID, roles, deletion state, or timestamps from public participant responses.
- Preserve admin/current-user surfaces so they continue using authorized endpoints for full user/team data where those screens require private account fields.

## Capabilities

### New Capabilities
- `privacy-safe-participant-data`: Defines the participant data contracts and privacy boundaries used by public and authorized participant displays.

### Modified Capabilities
- `game-detail-page-experience`: Updates tournament participant, bracket, match dialog, and placement rendering requirements so public game detail pages consume privacy-safe participant data from the loaded response without per-participant enrichment calls.
- `public-participant-profiles`: Updates public team/user link behavior in participant surfaces so profile links remain available without implying hidden private fields.

## Impact

- API contract boundary: `src/Mercurius.LAN.Web/APIClients/ILANClient.cs`.
- Participant DTOs and view models under `src/Mercurius.LAN.Web/DTOs/`, `src/Mercurius.LAN.Web/Models/`, and feature component state.
- Game detail components under `src/Mercurius.LAN.Web/Components/Pages/Games/`, especially participant lists, bracket/match displays, match detail dialog, placements/results, and team member chips.
- Service wrappers and mock backend data that currently assume full user/member DTOs in public game/team responses.
- No new third-party packages are expected.
