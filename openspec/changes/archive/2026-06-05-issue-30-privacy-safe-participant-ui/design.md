## Context

Issue #30 depends on back-end issue mercurius-aalst/mercurius-aalst-back-end#63, where anonymous public responses will stop returning full user/member DTOs. The current front-end public game detail flow still models `GameExtended.Users`, `Team.Members`, and `Placement.Users` as `UserDTO`, and shared participant rendering can display private account fields such as email whenever those values are present. First name, last name, Discord, Steam, and Riot are public profile fields, so the privacy-safe participant contract should allow them when returned by public game/team responses without reusing full account DTOs.

The affected surfaces are cross-cutting: game detail participants, bracket participant rows, match detail dialogs, placements/results, team member chips, mock backend data, and admin registration/removal flows. Public pages must keep rendering from the loaded game/team response data, while admin/current-user screens may continue using authorized endpoints for full account details.

## Goals / Non-Goals

**Goals:**
- Represent public tournament participants with privacy-safe DTOs that do not depend on full `UserDTO` fields.
- Keep public participant rendering useful with username, first name, last name, linked identities, display label, team name, captain labels, and profile links when those values are present.
- Render public profile fields from the loaded public API response without enriching participant cards from authorized endpoints.
- Preserve admin/current-user flows that legitimately need full user/team DTOs from authorized endpoints.
- Avoid additional per-participant API calls on public game detail, bracket, match detail, or placement rendering.
- Keep mock backend mode aligned enough to develop and verify privacy-safe participant displays locally.

**Non-Goals:**
- Implement the back-end privacy-safe participant endpoints.
- Change the public `/users/{username}` or `/teams/{teamName}` profile response contract beyond link behavior from participant surfaces.
- Add new packages or client-side caching infrastructure.
- Redesign the game detail page layout.

## Decisions

1. Introduce explicit participant response models at the front-end API boundary.

   Public game/team responses should deserialize into DTO/model types that expose participant identity fields intentionally: stable participant ID, optional username, first name, last name, display label, optional Discord/Steam/Riot IDs, optional team name, and optional captain/member labels when the public API returns them. These types should not reuse `UserDTO` or `UserProfileDTO` for anonymous game/team participant data because those models include private account fields.

   Alternative considered: keep `UserDTO` and hide private rows in components. Rejected because private fields would still be deserialized and remain available for accidental rendering, client-side serialization, or future component reuse.

2. Normalize participant display through a privacy-scoped view model.

   Shared participant components should consume a display model that can be created from public individual participants, public teams, and authorized/admin entities. The model should carry only fields allowed for the rendering context. Public constructors/factories should not accept full user DTOs, while admin/current-user factories may continue to do so in authorized surfaces.

   Alternative considered: update every component independently. Rejected because bracket rows, dialogs, participant cards, and placements would drift and reintroduce private-field assumptions.

3. Resolve participant names from loaded response collections with efficient lookup state.

   Game detail component state should build ID-keyed lookup dictionaries from the loaded game response for individuals and teams. Bracket rows, schedule labels, match details, and placements should resolve participant labels from those dictionaries or the placement payload without calling user/team lookup endpoints per participant.

   Alternative considered: fetch profile/team data on demand for missing fields. Rejected because issue #30 explicitly forbids enriching public participant cards from admin/current-user endpoints and forbids per-participant calls.

4. Keep admin registration/removal data paths separate from public display data.

   Admin controls such as adding or removing participants can continue calling authorized list/register endpoints and can use full authorized DTOs inside those workflows. Once a game detail response is refreshed, the public display portion should map the returned privacy-safe participant data rather than carrying forward full user records into public display components.

   Alternative considered: make all game detail DTOs privacy-safe and move admin controls to a separate page immediately. Rejected as larger than necessary for issue #30 and likely disruptive to existing admin workflows.

5. Treat missing optional fields as absence, not secrecy copy.

   Public rendering should omit missing optional public profile fields and private account fields entirely. It should not show `Not provided`, `Hidden`, blank labeled rows, or placeholders that imply a value exists but is being withheld.

   Alternative considered: show generic unavailable rows for consistency. Rejected because the issue requires avoiding placeholders that imply hidden private fields.

## Risks / Trade-offs

- [Risk] Back-end response property names may still shift while issue #63 is implemented. -> Mitigation: keep DTO names explicit, isolate mapping at `ILANClient`/service boundaries, and document any unverified contract assumptions during implementation.
- [Risk] Public and admin participant models may temporarily duplicate fields such as ID, username, and display label. -> Mitigation: favor small DTOs and a shared display model over broad inheritance from private account DTOs.
- [Risk] Mock data may still include private fields after code stops rendering them. -> Mitigation: update mock game/team responses to exercise username-only, public profile field, and optional linked identity cases.
- [Risk] Public components may lose useful labels when the API omits username. -> Mitigation: support a public display label field and fall back to neutral `TBD` or team display values only when the participant is genuinely unknown.

## Migration Plan

1. Add privacy-safe public participant DTOs/models and map `GameExtended`, teams, placements, and participant display helpers to those models where public game/team responses are consumed.
2. Update shared participant rendering, bracket resolution, match detail dialog, placements/results, and team member chips to use privacy-scoped display data.
3. Preserve authorized admin add/remove participant workflows and full user/current-user pages by keeping them on authorized service methods and private DTOs.
4. Update mock backend responses to include privacy-safe participant shapes and remove reliance on full user DTO fields for public participant displays.
5. Build the Blazor project and manually verify public game detail behavior in API-backed and mock modes when the real API contract is available.

## Back-end Contract Notes

- Verified against `D:\Github Repositories\mercurius-aalst-back-end` on `main`.
- `GetGameDTO.Users` and `GetPlacementDTO.Users` use `PublicUserDTO` with `Id`, `Username`, `DisplayName`, `DiscordId`, `SteamId`, and `RiotId`.
- `GetGameDTO.Teams`, `GetPlacementDTO.Teams`, and team list responses use `GetTeamDTO` with `Id`, `Name`, `CaptainUserId`, and `Members`.
- `GetTeamDTO.Members` uses `PublicUserDTO`.
- `GetMatchDTO` keeps participant references as user/team ID fields and does not inline participant objects.
- `PublicUserProfileDTO` includes `Username`, `Firstname`, `Lastname`, `DiscordId`, `SteamId`, and `RiotId`, but the current game/team participant DTO does not inline first or last names.

## Open Questions

- Should completed placement payloads include participant display labels directly, or only IDs resolvable through the game participant collections?
