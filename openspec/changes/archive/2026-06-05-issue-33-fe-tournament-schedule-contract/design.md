## Context

Backend schedule estimation work introduced a dedicated tournament schedule contract: admins provide `PlannedStartTime`, `AverageGameDurationMinutes`, and `RoundBreakDurationMinutes` when creating or updating a scheduled tournament, and API responses include planned tournament timing plus generated match estimates. The backend OpenSpec capability `tournament-schedule-estimation` remains the source contract for these DTO requirements.

The frontend currently has a narrower model. `CreateGameDTO` and `UpdateGameDTO` contain tournament identity, bracket, format, participation mode, image, and registration URL fields, but no planned schedule configuration. `Game` exposes `StartTime` and `EndTime`, which are lifecycle-oriented names and do not distinguish planned/estimated schedule values. `Match` exposes `StartTime` and `EndTime`, but not explicit estimated match fields. Admin create/edit forms do not collect schedule configuration, and public schedule surfaces order and display matches from the existing start/end fields.

The frontend also only defines `SingleElimination` and `DoubleElimination` bracket types. The backend enum also includes `RoundRobin` and `Swiss`, which the redesigned frontend should not render in this integration phase. Backend companion issue `mercurius-aalst/mercurius-aalst-back-end#81` should settle whether those types are blocked at write time, filtered from public reads, represented as unsupported, or exposed through a dedicated API/filter contract.

## Goals / Non-Goals

**Goals:**

- Make frontend create/update contracts send the backend-required schedule fields without weakening backend validation.
- Let admins configure tournament start time, average single-game duration, and break duration between rounds while a tournament is editable.
- Consume explicit planned and estimated schedule fields on frontend game and match models instead of treating lifecycle `StartTime`/`EndTime` as the schedule contract.
- Display API-backed tournament timing on games overview and game detail where the redesigned UI calls for schedule information.
- Label estimated timing clearly so visitors can distinguish planned/generated estimates from actual tournament lifecycle state.
- Preserve form state and surface backend validation errors when schedule validation fails.
- Keep frontend bracket rendering limited to single- and double-elimination until backend and frontend explicitly support additional types.
- Keep mock mode useful without relying on mock-only schedule assumptions.

**Non-Goals:**

- Introducing frontend support for `RoundRobin` or `Swiss` bracket rendering in this phase.
- Weakening backend create/update validation or making schedule fields optional in the integration contract.
- Adding live rescheduling after match generation starts unless the backend introduces a deliberate rescheduling rule.
- Performing extra per-match API calls to fill schedule timing; game detail should use the already loaded game detail/match data.
- Reworking bracket generation, score reporting, registration, sponsor placement, or tournament lifecycle actions outside schedule-related integration points.

## Decisions

### 1. Treat backend schedule fields as required frontend create/update inputs

Admin create and edit forms should collect planned tournament start time, average single-game duration, and break duration between rounds, then send those fields in the existing multipart create/update requests using backend-compatible field names. Frontend validation should require a planned start time and positive duration inputs before submit, but backend validation remains authoritative.

Alternative considered: send default schedule values from the service layer without showing controls. Rejected because it hides meaningful tournament planning decisions, creates mock-only assumptions, and still leaves admins unable to edit schedule values deliberately.

### 2. Use explicit planned/estimated model names in the frontend

Frontend `Game` and `Match` models should add explicit schedule properties such as `PlannedStartTime`, `AverageGameDurationMinutes`, `RoundBreakDurationMinutes`, `EstimatedEndTime`, `EstimatedStartTime`, and match `EstimatedEndTime` as appropriate for the backend response shape. Existing `StartTime`/`EndTime` should continue to represent actual lifecycle timing if the API still returns them, but UI schedule labels should prefer the explicit planned/estimated fields.

Alternative considered: remap backend planned/estimated values into the existing `StartTime`/`EndTime` properties. Rejected because it repeats the ambiguity the backend schedule spec intentionally avoided.

### 3. Keep schedule display data loaded with game and game detail responses

Games overview should use timing fields already present on game list responses. Game detail should use timing fields already present on the loaded game detail and match collection. The frontend should not add extra per-match requests solely to fetch estimated timing.

Alternative considered: query each match detail endpoint before rendering a schedule. Rejected because the issue explicitly avoids extra per-match API calls and because it creates avoidable latency and failure states.

### 4. Restrict schedule editing to tournaments the backend considers editable

Admin edit controls for schedule configuration should only be available while the tournament is still in the editable scheduled state. If a stale page submits edits after match generation or another lifecycle transition, the frontend should surface the backend validation error without losing form state and should let the admin reload or correct the form.

Alternative considered: hide all schedule fields after creation. Rejected because admins must be able to edit values while the tournament remains editable.

### 5. Coordinate unsupported bracket types with the backend companion change

The frontend should keep its bracket options and rendering support limited to `SingleElimination` and `DoubleElimination` for this phase. The backend companion explore/change should define whether `RoundRobin` and `Swiss` are blocked at admin write time, filtered from public frontend-facing reads, or represented explicitly as unsupported. Until that decision is implemented, frontend specs should require graceful handling if an unsupported value appears rather than expanding UI support accidentally.

Alternative considered: add placeholder frontend enum members for `RoundRobin` and `Swiss` and hide broken bracket sections. Rejected for this phase because it makes unsupported types look partially supported and shifts a backend contract gap into visitor-facing UI ambiguity.

### 6. Keep date/time serialization explicit

The frontend should send and display schedule times using a documented UTC-compatible contract. Admin local input can use browser-local date/time controls for usability, but serialization to the API should be normalized to UTC or otherwise match a documented backend expectation. Public display may format for the user's locale, provided labels do not obscure that estimated values are generated schedule estimates.

Alternative considered: pass browser-local strings through without normalization. Rejected because it creates timezone-dependent tournament schedules and makes backend validation/display inconsistently reproducible.

## Risks / Trade-offs

- Browser `datetime-local` controls do not carry timezone information -> convert deliberately at the API boundary and document the behavior.
- Backend validation messages may be raw or field-name-oriented -> display them without clearing form state, then refine field mapping if the backend exposes structured errors.
- Mock data can drift from API contract -> update mock store and JSON fixtures alongside models so mock mode exercises the same schedule fields.
- Unsupported bracket type behavior depends on backend companion work -> keep FE scope explicit and avoid adding partial support for types the frontend cannot render.
- Estimated match schedules may be absent before match generation -> render planned tournament timing separately from generated match estimates and use clear empty/unavailable states.

## Migration Plan

1. Add frontend OpenSpec requirements for the schedule create/update contract, schedule model fields, display labels, admin editability, validation, mock data, and unsupported bracket handling.
2. Update frontend DTOs and service multipart payloads to include backend-required schedule fields.
3. Add explicit schedule properties to game and match models and update mock data/store records.
4. Add admin-only schedule controls to create/edit forms with frontend validation and backend error preservation.
5. Update games overview and game detail schedule surfaces to prefer API-backed planned/estimated timing and label estimates clearly.
6. Verify build and coordinate backend contract checks with `mercurius-aalst-back-end#81` before implementation is considered complete.

## Open Questions

- Should the frontend convert admin-entered local date/time values to UTC at submit time, or should it display an explicit timezone selector before submit?
- Will backend validation errors for schedule fields be returned in a structured per-field shape or only as an error body string?
- What exact response field casing will the generated Refit/JSON deserialization path use for `plannedStartTime`, `estimatedStartTime`, and related values?
- Which backend companion decision will be adopted for `RoundRobin` and `Swiss`: write-time rejection, public-read filtering, explicit unsupported state, or a dedicated API/filter contract?
