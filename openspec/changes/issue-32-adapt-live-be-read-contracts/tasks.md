## 1. API Version Routing

- [x] 1.1 Normalize the configured Mercurius API base address so the live Refit clients compose requests under `/v1/`.
- [x] 1.2 Convert `ILANClient` Refit route attributes from leading-slash paths to relative `lan/...` paths.
- [x] 1.3 Convert `IUserClient` Refit route attributes from leading-slash paths to relative `lan/...` paths.
- [x] 1.4 Verify the resulting live request URIs preserve `/v1/lan/...` for games, sponsors, search, teams, public profiles, users, and matches.

## 2. Complete Refit Endpoint Coverage

- [x] 2.1 Inventory the local back-end route groups for games, matches, search, sponsors, teams, public profiles, current-user, and admin-user endpoints.
- [x] 2.2 Add missing `ILANClient` methods for search cursor/page-size parameters and team membership/invite endpoints.
- [x] 2.3 Add missing `IUserClient` methods for admin user create, update, delete-by-id, delete-by-username, and legacy username account deletion routes.
- [x] 2.4 Add or align front-end DTOs needed by the new Refit methods.

## 3. Global Search Contract

- [x] 3.1 Add a front-end search response DTO with `Results`, `NextCursor`, and `HasMore` properties matching the live back-end response.
- [x] 3.2 Update `ILANClient.SearchAsync` to return the response wrapper from `GET /v1/lan/search`.
- [x] 3.3 Ensure search result type deserialization handles live `user`, `team`, and `game` values.
- [x] 3.4 Update `GlobalSearchService` to keep the minimum three-character guard and return the wrapper `Results` collection to consumers.
- [x] 3.5 Keep navigation search loading, empty, error, keyboard, selection, and close behavior unchanged while rendering wrapped live results.
- [x] 3.6 Align mock global search behavior with the live contract where needed while preserving useful local results.

## 4. Game DTO Contract Alignment

- [x] 4.1 Update the front-end game and match models to include live schedule estimation fields used by the back-end DTOs.
- [x] 4.2 Ensure create and update game form data sends live required schedule fields.
- [x] 4.3 Update the front-end game detail model to represent the live nullable singular `SponsorPlacement` response.
- [x] 4.4 Update game detail featured partner selection and selected sponsor synchronization to use the singular sponsor placement.
- [x] 4.5 Keep sponsor replacement writes using the existing list-based request DTO while sending no more than one placement.
- [x] 4.6 Update mock game sponsor replacement and fixture projection to expose no more than one sponsor placement for game detail responses.

## 5. Verification

## 5. Admin Tournament Creation Visibility

- [x] 5.1 Keep the games overview add tournament card visible to admin users when no tournaments match the current filters or search.
- [x] 5.2 Align the front-end default Auth0 role claim type with the live back-end admin role claim type.

## 6. Verification

- [x] 6.1 Build the Blazor project with `dotnet build src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj`.
- [x] 6.2 Run the back-end smoke/contract check with `dotnet test LAN.API.sln` from `D:\Github Repositories\mercurius-aalst-back-end`.
- [ ] 6.3 Manually verify or smoke-test home, games overview, game detail, sponsors, and nav search against the live back-end.
- [ ] 6.4 Manually verify mock backend mode still supports local UI development for nav search and game detail sponsor rendering.
- [x] 6.5 Update this checklist as implementation tasks are completed and note any unresolved back-end contract or configuration assumptions.

## Notes

- Backend endpoint inventory covered `GameEndpoints`, `MatchEndpoints`, `SearchEndpoints`, `SponsorEndpoints`, `TeamEndpoints`, and `UserEndpoints`.
- Automated checks passed: front-end `dotnet build src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj`; back-end `dotnet test LAN.API.sln`.
- Front-end build was rerun after the admin add tournament card visibility fix.
- Manual browser smoke checks for live and mock modes remain pending.
