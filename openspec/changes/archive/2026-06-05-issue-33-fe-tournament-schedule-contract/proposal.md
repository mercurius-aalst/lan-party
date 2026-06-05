## Why

The redesigned frontend tournament create/update experience is out of sync with the backend schedule contract. The backend now requires planned tournament timing inputs on create/update and returns planned and estimated timing fields for games and matches, but the frontend DTOs, admin forms, domain models, and schedule displays still rely on the older `StartTime`/`EndTime` shape and do not collect or send the required schedule fields.

This creates an integration risk: admins cannot create or update tournaments against the stricter backend validation, public pages cannot accurately communicate planned or estimated schedule timing, and unsupported backend bracket types could still surprise frontend pages that only know how to render single- and double-elimination tournaments.

## What Changes

- Add OpenSpec coverage for frontend schedule configuration during tournament create and editable update flows.
- Align frontend create/update DTOs and multipart API payload expectations with backend fields for planned start time, average single-game duration, and round break duration.
- Extend frontend game and match model expectations to consume planned tournament timing and estimated match/tournament timing from API responses.
- Specify display behavior for games overview and game detail schedule sections so API-backed planned and estimated timing is clearly labeled.
- Specify admin-only schedule controls, validation behavior, and non-editable tournament handling in line with backend validation.
- Track the backend companion decision for excluding or explicitly handling unsupported `RoundRobin` and `Swiss` bracket types before the redesigned frontend consumes them.

## Capabilities

### New Capabilities

- `frontend-tournament-schedule-contract`: Frontend admin forms, DTOs, models, and public schedule displays align with backend tournament schedule configuration and estimated timing responses.

### Modified Capabilities

- `games-overview-browse-experience`: Games overview consumes API-backed tournament timing where the redesigned overview calls for planned or estimated schedule information.
- `game-detail-page-experience`: Game detail schedule and match sections use API-backed planned and estimated timing, with clear labels for estimated values.

## Impact

- Affects frontend game DTOs and API multipart mapping under `src/Mercurius.LAN.Web/DTOs/Games` and `src/Mercurius.LAN.Web/Services/GameService.cs`.
- Affects frontend game and match models under `src/Mercurius.LAN.Web/Models/Games` and `src/Mercurius.LAN.Web/Models/Matches`.
- Affects admin create/edit UI in `AddGameDialog` and `OverviewTab`.
- Affects public games overview and game detail schedule rendering.
- Affects mock backend data and mock store behavior so local/mock mode remains useful with the stricter schedule contract.
- Requires coordination with backend issue `mercurius-aalst/mercurius-aalst-back-end#81` for unsupported bracket type behavior.
