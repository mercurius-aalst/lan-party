## 1. Contracts and Services

- [x] 1.1 Add normalized global search DTOs, including result type, display label, optional supporting text, and route key fields for user username, team name, and game id.
- [x] 1.2 Add privacy-safe public profile DTOs for user profiles, team profiles, and public team members without email, legal names, roles, timestamps, deletion state, invites, or internal fields.
- [x] 1.3 Add frontend Refit search method declarations for `GET /lan/search?query={query}` with clear TODO comments that the backend endpoint still needs to be implemented.
- [x] 1.4 Add frontend Refit public profile method declarations for username and team-name lookups using privacy-safe DTOs, with clear TODO comments for the missing backend endpoints.
- [x] 1.5 Add frontend search and public profile service abstractions that call the Refit methods in production and enforce trimmed query threshold behavior where applicable.
- [x] 1.6 Ensure public/search UI does not render raw `UserDTO`, `UserProfileDTO`, or `Team`.
- [x] 1.7 Extend `ITeamService` and `TeamService` with public team detail support by team name while preserving existing admin/team-list methods.

## 2. Mock and Backend TODO Parity

- [x] 2.1 Extend `MockBackendStore` with case-insensitive starts-with global search across users, teams, and games for trimmed queries of at least 3 characters.
- [x] 2.2 Map mock search results to normalized safe records and exclude private user fields and team invites.
- [x] 2.3 Add mock public user profile lookup that returns username only for anonymous-style responses and linked Discord, Steam, and Riot identifiers for authenticated-style responses.
- [x] 2.4 Add mock public team profile lookup by team name with team name and member usernames only.
- [x] 2.5 Do not implement backend controllers, handlers, migrations, or persistence changes in this change.
- [x] 2.6 Add a short code comment near each new Refit TODO endpoint that identifies the backend API work needed later.

## 3. Navigation Search UI

- [x] 3.1 Add a global search input to `NavMenu.razor` that fits the existing desktop and mobile navigation layout.
- [x] 3.2 Implement debounced 250-400ms search input handling with cancellation or stale-result guards.
- [x] 3.3 Prevent lookup and clear visible results when the trimmed input is fewer than 3 characters.
- [x] 3.4 Render loading, empty, error, and result states in the search dropdown.
- [x] 3.5 Render every result row with a clear user, team, or game icon/label and accessible type text.
- [x] 3.6 Implement ArrowUp, ArrowDown, Enter, and Escape handling for the search input and result dropdown.
- [x] 3.7 Navigate selected results to `/users/{username}`, `/teams/{teamname}`, or `/games/{gameId}` with path segment encoding for names.
- [x] 3.8 Integrate search state into existing outside-click, navigation-selected, clearing-input, and mobile close behavior without regressing Info, Admin, or User dropdowns.
- [x] 3.9 Add scoped navigation styles for the search field, dropdown, states, highlighted result, and responsive layout.

## 4. Public Profile Pages

- [x] 4.1 Add a public `/users/{username:string}` page that loads a public user profile by username.
- [x] 4.2 Render anonymous user profile output as username only and render Discord, Steam, and Riot identifiers only when the response includes them for an authenticated visitor.
- [x] 4.3 Add a branded not-found state for unknown users without exposing raw API details.
- [x] 4.4 Add a public `/teams/{teamname:string}` page that loads a public team profile by team name.
- [x] 4.5 Render team name and public member usernames while excluding team invites.
- [x] 4.6 Navigate team member clicks to `/users/{username}` with path segment encoding.
- [x] 4.7 Add branded not-found state for unknown teams without exposing raw API details.
- [x] 4.8 Restyle public user and team pages as branded profile surfaces instead of plain full-width cards or duplicated titles.
- [x] 4.9 Keep public profile pages focused on profile content by removing discover/visibility-style sections.
- [x] 4.10 Highlight the team captain inline on the roster with an on-brand Captain label and no separate captain entry.
- [x] 4.11 Add a public team "Playing in" section that links to tournaments where the team participates.

## 5. Tournament Participant Popups

- [x] 5.1 Link participant popup usernames to `/users/{username}` when a public username is available.
- [x] 5.2 Render team captains in participant popups as an inline Captain label on the member chip rather than as a separate row or entry.
- [x] 5.3 Preserve existing participant popup details and empty-slot states while adding public profile links.

## 6. Verification

- [x] 6.1 Build the solution with `dotnet build Mercurius.LAN.sln -p:UseAppHost=false`.
- [x] 6.2 Verify fewer than 3 trimmed search characters show no results and trigger no visible stale dropdown.
- [x] 6.3 Verify 3 or more characters return case-insensitive prefix matches for users, teams, and games.
- [x] 6.4 Verify each search result type is visually identified and navigates to the expected route.
- [x] 6.5 Verify search closes on outside click, Escape, navigation selection, result selection, and clearing input.
- [x] 6.6 Verify existing Info, Admin, User, mobile navigation, login/logout, and ticket interactions still close and navigate correctly.
- [x] 6.7 Verify anonymous public user profile shows username only.
- [x] 6.8 Verify authenticated public user profile shows username plus Discord, Steam, and Riot identifiers when present.
- [x] 6.9 Verify public team page is accessible anonymously, excludes invites, and member clicks navigate to the member user profile.
- [x] 6.10 Verify public team page highlights the captain inline and lists participating tournaments.
- [x] 6.11 Verify participant popups link public usernames and label the captain inline without a separate captain row.
- [x] 6.12 Verify unknown user and team routes render branded not-found states.
