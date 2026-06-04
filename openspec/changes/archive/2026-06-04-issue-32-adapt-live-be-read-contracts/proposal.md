## Why

The redesigned Blazor front-end still models several read-only API responses from its mock-era assumptions, while the live back-end now exposes versioned `/v1/lan/...` endpoints with wrapper and singular DTO shapes. Aligning these contracts now prevents home, games, game detail, sponsors, and navigation search from silently calling unversioned routes or failing to deserialize live responses.

## What Changes

- Update front-end API client routing so all live Refit calls preserve the `/v1` base path for `/lan/...` endpoints.
- Add missing Refit methods for the live games, matches, search, sponsors, teams, public profiles, current-user, and admin-user API surfaces exposed by the back-end.
- Align request and response DTOs used by those Refit methods with the live back-end contracts.
- Consume the live global search response wrapper and render the bounded `results` collection in the navigation dropdown.
- Align global search expectations with the live back-end contract, including pagination metadata and supported result fields.
- Align game detail sponsor rendering with the live singular `sponsorPlacement` response while still presenting at most one featured tournament partner.
- Preserve admin-only tournament creation affordances on the games overview when the current result set is empty.
- Align the front-end default Auth0 role claim type with the live back-end admin role claim type.
- Preserve mock backend usefulness for local UI development while matching the live read contracts closely enough for integration.
- Keep existing loading, empty, error, not-found, and unauthorized states visible.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `global-search`: Search consumers must handle the live response wrapper and public-safe normalized result records returned by the back-end.
- `site-navigation`: Header search must render search results from the wrapped live response without breaking loading, empty, error, keyboard, or navigation behavior.
- `game-detail-page-experience`: Tournament detail sponsor context must consume and render at most one singular sponsor placement from the live game response.
- `games-overview-browse-experience`: Admin users must retain access to the add tournament card even when no tournaments match the current overview state.

## Impact

- Front-end Refit contracts in `src/Mercurius.LAN.Web/APIClients/`.
- Front-end request/response DTOs needed by the live API client boundary.
- Search DTOs and services in `src/Mercurius.LAN.Web/DTOs/Search/` and `src/Mercurius.LAN.Web/Services/GlobalSearchService.cs`.
- Game DTO/model and sponsor rendering in `src/Mercurius.LAN.Web/Models/Games/GameExtended.cs`, `src/Mercurius.LAN.Web/Services/GameService.cs`, and `src/Mercurius.LAN.Web/Components/Pages/Games/GameDetail.*`.
- Games overview admin rendering in `src/Mercurius.LAN.Web/Components/Pages/Games/GamesOverview.razor`.
- Auth0 role claim defaults in `src/Mercurius.LAN.Web/Options/Auth0Options.cs`.
- Mock backend search and sponsor placement behavior in `src/Mercurius.LAN.Web/Mock/`.
- Live back-end contract assumptions verified against `D:\Github Repositories\mercurius-aalst-back-end`.
- No new package dependencies are expected.
