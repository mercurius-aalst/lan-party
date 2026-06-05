## 1. Contract Alignment

- [x] 1.1 Inspect the local back-end checkout at `D:\Github Repositories\mercurius-aalst-back-end` on the current `main` branch to confirm public participant response property names for games, teams, matches, and placements.
- [x] 1.2 Document any remaining back-end contract assumptions in the implementation notes or PR description.
- [x] 1.3 Identify every front-end public participant display that currently depends on `UserDTO`, `Team.Members`, or full member fields from game/team responses.

## 2. Privacy-Safe DTOs and API Boundary

- [x] 2.1 Add privacy-safe public participant/member DTOs for individual participants, team participants, team members, and placement participants without private account fields.
- [x] 2.2 Update game detail, team, match, and placement response models used by `ILANClient` so public responses no longer deserialize anonymous participant data into `UserDTO`.
- [x] 2.3 Keep authorized admin/current-user DTOs and service methods separate from the public participant DTOs.
- [x] 2.4 Add mapping helpers or factories that create participant display models from public participant DTOs and authorized DTOs without mixing privacy scopes.

## 3. Game Detail Participant Rendering

- [x] 3.1 Update `ParticipantsTab` and shared participant cards to render username/display-label-only public participants cleanly.
- [x] 3.2 Remove public participant rows for email and other private account/internal fields from public game/team participant displays.
- [x] 3.3 Render first name, last name, Discord, Steam, and Riot values in public participant displays when the loaded public participant response includes them.
- [x] 3.4 Link usernames to `/users/{username}` and team names to `/teams/{teamName}` only when those route identifiers are present.
- [x] 3.5 Omit missing optional fields without `Not provided`, hidden-field, or private-field placeholder copy on public participant surfaces.

## 4. Bracket, Match Dialog, and Results

- [x] 4.1 Build efficient ID-keyed participant lookup state from the loaded game detail response for individual and team participants.
- [x] 4.2 Update schedule and bracket participant name resolution to use the loaded privacy-safe participant data without per-participant API calls.
- [x] 4.3 Update `MatchDetailsDialog` participant cards to resolve from loaded privacy-safe game data and avoid admin/current-user enrichment.
- [x] 4.4 Update `PlacementsTab` and placement models to render results from loaded privacy-safe placement or game participant data.
- [x] 4.5 Preserve BYE, TBD, empty, not-found, unauthorized, and error states while changing participant data sources.

## 5. Admin and Mock Mode

- [x] 5.1 Preserve admin add/remove participant workflows using authorized user/team list endpoints where required.
- [x] 5.2 Ensure refreshed game detail responses after admin registration/removal are mapped back into privacy-safe public display models.
- [x] 5.3 Update mock backend game/team/placement data to exercise username-only participants, teams with public member usernames, public first/last names, and optional linked identity cases.
- [x] 5.4 Remove mock-display reliance on private participant fields once the real back-end contract is available.

## 6. Verification

- [x] 6.1 Run `dotnet build src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj`.
- [x] 6.2 Verify public game detail participant list works with privacy-safe real or mocked API data.
- [x] 6.3 Verify bracket rows and match detail dialogs render names/cards without extra per-participant API calls.
- [x] 6.4 Verify placements/results render with privacy-safe participant data.
- [x] 6.5 Verify public participant displays do not expose email or other private account/internal fields, while rendering returned first name, last name, Discord, Steam, and Riot public fields without missing-field placeholders.
- [x] 6.6 Verify admin/current-user screens that need full user/team data still use authorized endpoints and remain functional.
