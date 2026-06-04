## 1. OpenSpec Coverage

- [x] 1.1 Add a frontend schedule contract capability spec covering create/update inputs, response model fields, display behavior, validation, and unsupported bracket handling.
- [x] 1.2 Update `games-overview-browse-experience` requirements so overview schedule surfaces consume API-backed planned/estimated tournament timing.
- [x] 1.3 Update `game-detail-page-experience` requirements so match schedule sections consume loaded match estimates and label estimated timing clearly.

## 2. DTOs, Models, And API Mapping

- [x] 2.1 Add planned start time, average game duration minutes, and round break duration minutes to `CreateGameDTO` and `UpdateGameDTO` with frontend validation annotations.
- [x] 2.2 Add explicit planned/estimated schedule properties to `Game` and `Match` models without reusing lifecycle `StartTime`/`EndTime` labels for estimates.
- [x] 2.3 Include schedule fields in `GameService` create/update multipart form data using backend-compatible field names and UTC-compatible serialization.
- [x] 2.4 Confirm list/detail deserialization handles backend schedule field casing and missing pre-generation estimate values safely.

## 3. Admin Create/Edit Experience

- [x] 3.1 Add admin create form controls for tournament start time, average single-game duration, and round break duration.
- [x] 3.2 Add admin edit form controls for the same schedule values while the tournament remains editable.
- [x] 3.3 Prevent or gracefully handle schedule edits after the tournament is no longer editable, matching backend validation.
- [x] 3.4 Preserve form state and surface backend validation errors when schedule submit fails.
- [x] 3.5 Keep bracket type choices limited to frontend-supported single- and double-elimination values.

## 4. Public Schedule Display

- [x] 4.1 Update games overview timing displays to use API-backed planned tournament start and estimated end timing where the design calls for schedule information.
- [x] 4.2 Update game detail overview timing to distinguish planned start, estimated end, and actual lifecycle timestamps where applicable.
- [x] 4.3 Update game detail match schedule ordering/labels to use loaded match estimated start/end timing and clearly identify estimates.
- [x] 4.4 Render clear empty/unavailable states when match estimates are not generated yet, without additional per-match API calls.
- [x] 4.5 Gracefully handle any unsupported bracket type returned before backend companion constraints are complete.

## 5. Mock Mode And Verification

- [x] 5.1 Update `MockBackendStore` and `MockData.Local/backend.json` with schedule fields that mirror the backend contract.
- [x] 5.2 Build the frontend project with `dotnet build src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj`.
- [x] 5.3 Run or coordinate the backend companion contract check with `dotnet test LAN.API.sln` in `mercurius-aalst-back-end`.
- [x] 5.4 Verify create/edit, games overview, and game detail schedule behavior in both API-backed and mock modes.
