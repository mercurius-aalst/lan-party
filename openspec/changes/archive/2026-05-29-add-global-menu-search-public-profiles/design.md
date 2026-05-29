## Context

The Blazor web app currently keeps primary header behavior in `NavMenu.razor` and `NavMenu.razor.cs`, with separate booleans for Info, Admin, and User dropdowns plus a fixed outside-click overlay. The frontend already uses Refit clients and service abstractions for `/lan/games`, `/lan/teams`, and `/lan/users`, while mock mode provides local equivalents through `MockBackendStore`.

Existing user and team models are not safe for public rendering as-is. `UserDTO`/`UserProfileDTO` include email, first/last name, timestamps, deletion state, and other account fields. `Team` includes `TeamInvites`, which must not appear on a public team page. This change therefore needs dedicated response DTOs, not reuse of broad admin/profile models.

## Goals / Non-Goals

**Goals:**
- Provide global header search across users, teams, and games/tournaments.
- Keep search threshold, debounce, prefix matching, keyboard interaction, and close behavior predictable.
- Add public user and team profile routes that expose only privacy-approved fields.
- Preserve existing Info, Admin, User, mobile navigation, and ticket interactions.
- Keep frontend code using service/client abstractions and add mock-mode parity.
- Add clearly marked Refit method declarations for backend TODO endpoints so the future API work is easy to identify.

**Non-Goals:**
- Full-text, fuzzy, contains, typo-tolerant, or ranking-based search.
- Admin search or management workflows.
- Editing user/team data from public pages.
- Changing the existing `/profile` private account page behavior.
- Exposing team invites, email, legal names, roles, audit timestamps, deletion state, or other internal fields.
- Implementing backend API controllers, backend endpoint handlers, migrations, or persistence changes.

## Decisions

1. Use a normalized backend global search contract.

   The frontend should declare and call a dedicated search method such as `GET /lan/search?query={query}` through a new Refit client/service. The Refit method should include a clear TODO comment that the backend endpoint still needs to be implemented. The response should be a flat list of safe result DTOs with type, display label, optional supporting text, and route key data. The frontend can construct routes from those safe fields:
   - user: username -> `/users/{escaped username}`
   - team: team name -> `/teams/{escaped team name}`
   - game: game id -> `/games/{gameId}`

   Alternative considered: have the header fetch all users, teams, and games and filter locally. That would reuse existing calls but pulls private/broad models into navigation, duplicates filtering logic, and scales poorly.

2. Keep the matching rule server-owned and frontend-guarded.

   The nav input should not call the service until `query.Trim().Length >= 3`. The future backend search endpoint should apply the same threshold semantics and perform case-insensitive starts-with matching so API behavior remains correct if called directly. The frontend uses debounce, cancellation, and stale-result guards to avoid flashing obsolete results.

   Alternative considered: enforce the threshold only in the UI. That is simpler but leaves inconsistent endpoint behavior and makes tests weaker.

3. Add dedicated public profile DTOs.

   Public user responses should contain `Username` and, only for authenticated callers, `DiscordId`, `SteamId`, and `RiotId`. Public team responses should contain team name, public member summaries, captain username, and tournament summaries, with no `TeamInvites` collection. Frontend pages render only these public DTOs.

   Alternative considered: use existing `UserDTO` and `Team` and hide fields in Razor. That leaves accidental exposure risk through future markup, logging, serialization, or debug views.

4. Treat search as another temporary navigation surface.

   `NavMenu` should add explicit search open/close state and route it through the same central close method used by existing dropdowns. Opening search results should close Info/Admin/User dropdowns, outside-click should close all temporary surfaces, Escape should close search, selecting a result should close search and invoke `OnNavigationSelected`, and clearing the input should clear results and close the dropdown. Because the sticky blurred header creates a stacking context, search-specific outside-click handling may use document-level pointer detection instead of relying only on a CSS overlay.

   Alternative considered: implement the search dropdown as a self-contained child with no shared state. That risks competing overlays and inconsistent mobile menu closing.

5. Render public not-found states inside the page routes.

   Unknown user/team responses should render the shared branded `StatusPage` treatment from the public page instead of leaking raw API errors or relying only on router-level not-found. This lets `/users/{unknown}` and `/teams/{unknown}` remain branded, recoverable pages.

6. Keep tournament participant popups connected to public profiles.

   Participant popups in tournament detail contexts may still render richer tournament/admin-visible participant details, but public usernames should link to `/users/{username}` when available. For team participants, the captain should be indicated as an inline label on the relevant member chip rather than a separate captain row, keeping the roster compact and consistent with the public team page.

## Risks / Trade-offs

- Backend contract mismatch -> Define DTOs and Refit TODO methods first, keep mock-mode implementation aligned, and make backend expectations explicit without implementing backend handlers in this change.
- Private data exposure through reused models -> Do not bind `UserDTO`, `UserProfileDTO`, or `Team` directly in public/search UI; map to public DTOs before rendering.
- Race conditions during debounced search -> Use a cancellation token/version counter and discard stale responses.
- Header interaction regressions -> Centralize close behavior and manually verify Info/Admin/User dropdowns, mobile close callback, outside-click, Escape, clearing, and selection.
- Route encoding differences for names -> Escape username and team name path segments during navigation and decode route parameters before lookup.
- Case-insensitive prefix behavior differs by data source collation -> Normalize comparisons explicitly in backend/mock search logic.
