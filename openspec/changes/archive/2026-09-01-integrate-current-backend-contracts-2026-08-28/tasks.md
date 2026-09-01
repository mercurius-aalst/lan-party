## 1. Lock the live contract boundary

- [x] 1.1 Record the backend contract baseline at commit `e17d2b9` and verify the versioned `/v1/lan` route map against `mercurius-aalst-back-end`; treat the sibling repository as read-only for this change.
- [x] 1.2 Update `src/Mercurius.LAN.Web/APIClients/ILANClient.cs` and `IUserClient.cs` to use the canonical tournament, match, team, public profile, search, sponsor, and identity routes with the exact current verbs and request bodies; the contracts MUST expose `Tournament*` aggregate types and MUST NOT expose legacy `Game*` aliases.
- [x] 1.3 Verify `src/Mercurius.LAN.Web/Extensions/DependencyExtensions.cs`, `Program.cs`, and API options normalize the host base and apply rooted `/v1/lan/...` routes exactly once, preserve Auth0 bearer handling, and keep the team-events hub at `/v1/lan/team-events` without duplication.

## 2. Align transport DTOs and domain mappings

- [x] 2.1 Update the tournament read/write transport models and `DTOs/Tournaments/*` to map `TournamentId`, `TeamSize`, planned/estimated timing, lifecycle status, and singular sponsor placement; remove `RegisterFormUrl` from live create/update payloads and use `Tournament*` model/DTO names throughout.
- [x] 2.2 Align `src/Mercurius.LAN.Web/Models/Matches/*`, registration DTOs, placement/participant mappings, and team/public DTOs with privacy-safe backend fields, including `Match.TournamentId` and active registration projections.
- [x] 2.3 Align `DTOs/Search/*` and the normalized search view model with the wrapped `results`/`nextCursor`/`hasMore` response and `user`/`team`/`tournament` result types, without retaining `GameId` as a wire requirement.

## 3. Migrate public tournament read surfaces

- [x] 3.1 Update `src/Mercurius.LAN.Web/Services/TournamentService.cs`, `ITournamentService.cs`, and overview/home consumers to load the paged tournament collection and preserve loading, empty, unauthorized, not-found, and error states.
- [x] 3.2 Update `Components/Pages/Tournaments/TournamentDetail.razor*`, `Tabs/*`, match detail, bracket, participant, and placement components to consume one canonical tournament detail response and avoid per-match or per-participant enrichment calls.
- [x] 3.3 Use `/tournaments` and `/tournaments/{id}` presentation routes and navigation links while ensuring every API request and model mapping uses tournament identifiers and the current schedule fields.

## 4. Migrate tournament administration and match actions

- [x] 4.1 Update `Components/Pages/Tournaments/AddTournamentDialog.razor*`, tournament DTOs, and `TournamentService` multipart construction for the current create/update fields, including optional image and team size, with no external registration URL field.
- [x] 4.2 Replace legacy game lifecycle and sponsor mutations with tournament lifecycle `PUT`, sponsor replacement `PUT`, and current admin delete contracts; preserve role checks and actionable validation/API errors.
- [x] 4.3 Update match editing and match DTO handling to use the current admin match update contract, score validation, and `TournamentId` relationship.

## 5. Implement backend-authoritative tournament registration

- [x] 5.1 Add service and DTO support for authenticated registration state, individual eligibility, team eligibility, roster eligibility, individual registration/removal, team roster submission/removal, and confirmed roster-member updates, plus backend-supported admin registration listing/removal.
- [x] 5.2 Refactor `Components/Pages/Tournaments/Tabs/TournamentParticipantsTab.razor*` and related lookup components so public pages render active registrations while authenticated users receive only the supported self/team roster actions; retain only backend-supported admin registration listing/removal and remove legacy admin add/replace/force-confirm workflows.
- [x] 5.3 Add clear loading, closed/ineligible, unauthorized, validation, conflict, and API-error states for registration actions, and refresh visible registration state only from confirmed backend responses.

## 6. Migrate user-owned team management

- [x] 6.1 Update `Services/TeamService.cs`, `ITeamService.cs`, `DTOs/Participants/Teams/*`, and `APIClients/ILANClient.cs` for current team routes: delete-members-me leave, invite body, team-invite response body, captain transfer, member removal, team deletion, and logo PUT/DELETE.
- [x] 6.2 Update `Components/Pages/Teams/ManageTeams.razor*`, create/invite dialogs, and team management models to map current summary/invite responses, preserve confirmed-state refreshes, and retain recoverable mutation errors.
- [x] 6.3 Keep team management privacy-safe and authenticated: no admin-only team lifecycle UI, no private fields in lookup or invite rows, and no anonymous calls to current-user team resources.

## 7. Migrate identity and public profile flows

- [x] 7.1 Update complete-profile, profile redirect, current-profile, username availability, and account action consumers to use the current identity routes, especially `PUT /v1/lan/users/me` for completion.
- [x] 7.2 Update `Services/PublicProfileService.cs`, `IPublicProfileService.cs`, public user/team pages, and profile DTOs for `/public/users/{username}` and `/public/teams/{teamName}`, including `TournamentId` links and privacy-safe fields only.
- [x] 7.3 Verify `Components/Auth/*`, `Middleware/AccessTokenHandler.cs`, and error handling preserve anonymous/public access, Auth0 authorization, unauthorized recovery, and backend response diagnostics without leaking private data.

## 8. Migrate global search and shared navigation

- [x] 8.1 Update `Services/GlobalSearchService.cs`, `IGlobalSearchService.cs`, `Services/UserSearchService.cs`, and search DTOs for the bounded public wrapper, minimum query length, cursor metadata, and canonical result types.
- [x] 8.2 Update `Components/Layout/NavMenu*`, `Components/Shared/GlobalSearchResultItem.razor*`, and invite-user search consumers so user, team, and tournament destinations use `username`, `teamName`, and `TournamentId` without stale `GameId` handling.
- [x] 8.3 Preserve loading, empty, failure, and stale-result behavior for both global navigation search and authenticated invite lookup.

## 9. Bring mock mode to contract parity

- [x] 9.1 Update `Mock/MockBackendDocument.cs`, `MockBackendStore.cs`, `MockServices.cs`, and `src/Mercurius.LAN.Web/MockData.Local/*` to represent canonical tournaments, match `TournamentId`, team size, singular sponsor placement, active registrations, and wrapped search responses.
- [x] 9.2 Implement mock transitions for supported registration, roster, team, identity, lifecycle, sponsor, logo, and search actions, including representative blocked/error outcomes and confirmed-state refresh behavior.
- [x] 9.3 Verify mock and live service interfaces expose the same route-independent semantics so local UI development does not depend on removed legacy game fields or actions.

## 10. Validate the integrated contract

- [x] 10.1 Search the frontend for removed `/lan/games`, legacy lifecycle/registration routes, `RegisterFormUrl`, required `GameId` wire mappings, and aggregate `Game*` types; resolve every remaining live-contract reference or document only genuine backend field terminology such as `AverageGameDurationMinutes`. The only residual match is the historical documentation mention at `CHANGELOG.md:47`; gamer/game-ID prose and `AverageGameDurationMinutes` are legitimate terminology, not live-contract references.
- [x] 10.2 Run `openspec validate integrate-current-backend-contracts-2026-08-28` and update this checklist as each implementation slice is completed.
- [ ] 10.3 Build `src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj`, rebuild Tailwind only if styling changes, and smoke-test anonymous, authenticated, admin, mock, not-found, and API-error paths against the backend contract baseline. FE build, contract tests, and OpenSpec validation are automated checks; manual/browser smoke for these paths was not run in this workflow and remains required before release.
