## Why

Visitors need a fast way to jump from the main navigation to people, teams, and tournaments without first knowing which page owns that data. Public profile routes also need privacy-safe DTOs so user and team discovery does not expose account/admin fields through existing broad models.

## What Changes

- Add a global search box to the main navigation for users, teams, and games/tournaments.
- Search only after at least 3 trimmed characters and match case-insensitive prefixes from that point onward.
- Show a debounced dropdown with loading, empty, and error states, plus keyboard support for ArrowUp, ArrowDown, Enter, and Escape.
- Identify every result type visually with an icon and label, then navigate selections to `/users/{username}`, `/teams/{teamname}`, or `/games/{gameId}`.
- Integrate the search dropdown with existing nav menu outside-click, navigation, Escape, and clearing-input close behavior without breaking Info, Admin, or User menus.
- Add privacy-safe public user profile pages where anonymous visitors see username only and authenticated visitors additionally see Discord, Steam, and Riot IDs.
- Add public team profile pages that show team name and members, exclude invites, and route member clicks to `/users/{username}`.
- Improve public user/team profile presentation with branded profile cards, focused content, inline team captain labels, and a team "Playing in" tournament section.
- Link usernames inside tournament participant popups to their public profiles and mark the team captain inline instead of rendering a separate captain entry.
- Add frontend DTOs, Refit client method declarations, and services for normalized search results and public profile responses instead of rendering raw `UserDTO`.
- Mark the required backend API paths clearly in the Refit clients as TODO backend endpoints; implementing those backend endpoints is out of scope for this change.
- Add mock service/store support for local mock mode so the frontend can be exercised before the backend API exists.

## Capabilities

### New Capabilities
- `global-search`: Normalized search result retrieval for users, teams, and games/tournaments, including query thresholds, prefix matching, response states, and destination metadata.
- `public-participant-profiles`: Privacy-safe public user and team profile pages, including authenticated-field visibility, team member links, invite exclusion, and not-found states.

### Modified Capabilities
- `site-navigation`: Adds global search UI behavior to the header navigation and extends temporary navigation surface closing behavior to include search dropdown interactions.

## Impact

- Frontend navigation: `src/Mercurius.LAN.Web/Components/Layout/NavMenu.razor`, code-behind, and scoped styles.
- Frontend API layer: new Refit client methods or client interfaces, DTOs, and services for search results and public user/team profiles.
- Public pages: new `/users/{username:string}` and `/teams/{teamname:string}` Blazor pages and styles using privacy-safe view models.
- Team services: `ITeamService`/`TeamService` may need public lookup/detail support by team name in addition to existing id-based team access.
- Tournament detail participant popups: member/user links should route to public user profiles, with captain status shown as an inline label.
- Mock backend mode: `MockBackendStore` and mock service implementations need matching search/profile behavior for local development.
- Backend API contract: expected normalized global search endpoint and privacy-safe public user/team profile endpoints under the existing `/lan` API surface, represented by clearly marked frontend Refit TODO methods only.
- Verification: build plus focused manual/browser checks for search threshold, debounce, keyboard navigation, dropdown closing behavior, privacy visibility, team member navigation, and branded not-found states.
