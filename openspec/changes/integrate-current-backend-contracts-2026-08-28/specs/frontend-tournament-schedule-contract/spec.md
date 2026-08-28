## MODIFIED Requirements

### Requirement: Frontend create and update requests use backend schedule field names
The frontend SHALL serialize tournament schedule configuration in create and update multipart
requests using the current backend tournament contract.

#### Scenario: Create request is submitted
- **WHEN** the frontend submits a create tournament multipart request
- **THEN** the payload includes `Name`, `BracketType`, `Format`, `FinalsFormat`, `ParticipationMode`, `Image`, `TeamSize` when applicable, `PlannedStartTime`, `AverageGameDurationMinutes`, and `RoundBreakDurationMinutes`
- **AND** the planned start time is serialized using a UTC-compatible representation that matches the backend API contract
- **AND** the payload does not include the removed `RegisterFormUrl` field

#### Scenario: Update request is submitted
- **WHEN** the frontend submits an update tournament multipart request
- **THEN** the payload includes the current planned schedule values and `TeamSize` alongside the other editable tournament fields
- **AND** duration values are sent as positive minute values rather than formatted display strings
- **AND** the payload does not include the removed `RegisterFormUrl` field

#### Scenario: Backend rejects schedule or team-size values
- **WHEN** the backend rejects a create or update submission because schedule configuration, team size, or editability is invalid
- **THEN** the frontend surfaces the backend validation error to the admin
- **AND** the form preserves the values the admin submitted so they can correct the input without re-entering the entire tournament

### Requirement: Frontend models distinguish planned and estimated schedule values
The frontend SHALL model the current tournament and match response fields with explicit names
instead of treating lifecycle timestamps as generated schedule estimates.

#### Scenario: Tournament list or detail response includes schedule fields
- **WHEN** the frontend deserializes a tournament response
- **THEN** it can read `PlannedStartTime`, `AverageGameDurationMinutes`, `RoundBreakDurationMinutes`, `EstimatedEndTime`, and optional `TeamSize` from explicit properties
- **AND** `StartTime` and `EndTime` remain available only for actual lifecycle timing if the backend returns them
- **AND** no external registration URL is required for the response to render

#### Scenario: Match response includes estimated timing
- **WHEN** the frontend deserializes a match in a tournament detail response
- **THEN** it can read `EstimatedStartTime`, `EstimatedEndTime`, and `TournamentId`
- **AND** the UI does not label those values as actual started or completed times
- **AND** it does not require a legacy `GameId` property

#### Scenario: Estimates have not been generated yet
- **WHEN** a scheduled tournament has no generated match estimates yet
- **THEN** the frontend treats estimated match and tournament end values as unavailable
- **AND** public schedule UI continues to show planned tournament configuration when that data is available

#### Scenario: API datetime values are received
- **WHEN** the frontend deserializes DateTime values from API-backed responses
- **THEN** the frontend converts those values to the local timezone before models are consumed by UI components
- **AND** outbound DateTime values remain serialized in a UTC-compatible representation when sent back to the backend

#### Scenario: Schedule values are edited and displayed
- **WHEN** the frontend displays schedule DateTime values
- **THEN** the UI formats them using local time
- **AND** datetime input and picker controls use local DateTime values while editing
- **AND** create and update submissions convert those local input values to UTC ISO strings with a `Z` suffix before sending them to the backend

### Requirement: Mock mode mirrors the schedule contract
The frontend mock backend SHALL include the same planned, estimated, and tournament-identity fields
used by API-backed mode and SHALL not depend on the removed external registration URL.

#### Scenario: Mock tournaments are loaded
- **WHEN** the application runs against mock data
- **THEN** mock tournament records include planned start time, schedule duration configuration, optional team size, and estimated end values consistent with the frontend models
- **AND** mock match records include estimated start and end values and `TournamentId` when match estimates are expected

#### Scenario: Mock create or update is submitted
- **WHEN** a mock create or update request includes schedule fields or team size
- **THEN** mock handling stores and returns those values using the same frontend model fields as API-backed mode
- **AND** mock handling does not require or persist `RegisterFormUrl`
