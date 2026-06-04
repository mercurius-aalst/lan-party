# frontend-tournament-schedule-contract Specification

## ADDED Requirements

### Requirement: Admin tournament forms collect required schedule configuration

The frontend SHALL require admins to provide planned tournament schedule configuration when creating or updating a scheduled tournament.

#### Scenario: Admin creates a scheduled tournament
- **WHEN** an admin opens the create tournament form
- **THEN** the form includes controls for planned tournament start time, average single-game duration, and break duration between rounds
- **AND** the form prevents submission when planned start time is missing or duration values are missing, zero, or negative

#### Scenario: Admin updates an editable scheduled tournament
- **WHEN** an admin edits a tournament that remains in the scheduled editable state
- **THEN** the edit form shows the existing planned start time, average single-game duration, and round break duration values
- **AND** the admin can submit changed values with the rest of the editable tournament fields

#### Scenario: Backend rejects schedule values
- **WHEN** the backend rejects a create or update submission because schedule configuration is invalid or no longer editable
- **THEN** the frontend surfaces the backend validation error to the admin
- **AND** the form preserves the values the admin submitted so they can correct the input without re-entering the entire tournament

### Requirement: Frontend create and update requests use backend schedule field names

The frontend SHALL serialize schedule configuration in create and update requests using the backend tournament schedule contract.

#### Scenario: Create request is submitted
- **WHEN** the frontend submits a create tournament multipart request
- **THEN** the payload includes `PlannedStartTime`, `AverageGameDurationMinutes`, and `RoundBreakDurationMinutes`
- **AND** the planned start time is serialized using a UTC-compatible representation that matches the backend API contract

#### Scenario: Update request is submitted
- **WHEN** the frontend submits an update tournament multipart request
- **THEN** the payload includes the current planned schedule values alongside the other editable tournament fields
- **AND** duration values are sent as positive minute values rather than formatted display strings

### Requirement: Frontend models distinguish planned and estimated schedule values

The frontend SHALL model planned and estimated schedule values with explicit names instead of treating lifecycle timestamps as generated schedule estimates.

#### Scenario: Game list or detail response includes schedule fields
- **WHEN** the frontend deserializes a game response
- **THEN** it can read planned start time, average single-game duration, round break duration, and estimated tournament end time from explicit schedule properties
- **AND** `StartTime` and `EndTime` remain available only for actual lifecycle timing if the backend returns them

#### Scenario: Match response includes estimated timing
- **WHEN** the frontend deserializes a match in a game detail response
- **THEN** it can read estimated match start and end times from explicit estimated schedule properties
- **AND** the UI does not label those values as actual started or completed times

#### Scenario: Estimates have not been generated yet
- **WHEN** a scheduled tournament has no generated match estimates yet
- **THEN** the frontend treats estimated match and tournament end values as unavailable
- **AND** public schedule UI continues to show planned tournament configuration when that data is available

### Requirement: Public timing displays label generated values as estimates

The frontend SHALL distinguish planned schedule values, generated estimates, and actual lifecycle timestamps in visitor-facing displays.

#### Scenario: Visitor views planned tournament timing
- **WHEN** a tournament has a planned start time before match generation
- **THEN** the UI labels the value as planned or scheduled timing rather than an actual start timestamp

#### Scenario: Visitor views generated match timing
- **WHEN** a match has estimated start and end times
- **THEN** the schedule UI labels the timing as estimated
- **AND** the schedule can order matches by estimated start time without requiring extra per-match API calls

#### Scenario: Visitor views actual lifecycle timing
- **WHEN** actual tournament start or end timestamps are shown alongside planned or estimated values
- **THEN** labels make the lifecycle status clear so visitors do not confuse actual timestamps with estimates

### Requirement: Frontend handles unsupported bracket types safely

The frontend SHALL avoid presenting unsupported bracket types as usable tournament options or broken bracket pages.

#### Scenario: Admin chooses a bracket type in frontend forms
- **WHEN** the frontend renders create or edit bracket type options
- **THEN** only frontend-supported bracket types are presented as selectable options
- **AND** unsupported backend-only bracket types are not introduced as partially supported choices

#### Scenario: API returns an unsupported bracket type
- **WHEN** a game response contains a bracket type the frontend cannot render
- **THEN** the frontend shows a clear unsupported or unavailable state for bracket-specific content
- **AND** non-bracket tournament details remain accessible when possible

### Requirement: Mock mode mirrors the schedule contract

The frontend mock backend SHALL include the same planned and estimated schedule fields used by API-backed mode.

#### Scenario: Mock games are loaded
- **WHEN** the application runs against mock data
- **THEN** mock game records include planned start time, schedule duration configuration, and estimated end values consistent with the frontend models
- **AND** mock match records include estimated start and end values when match estimates are expected

#### Scenario: Mock create or update is submitted
- **WHEN** a mock create or update request includes schedule fields
- **THEN** mock handling stores and returns those schedule values using the same frontend model fields as API-backed mode
