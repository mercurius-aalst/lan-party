## ADDED Requirements

### Requirement: Team management contains API and realtime failures

Authenticated team management MUST handle expected API and realtime transport failures as recoverable page state without terminating the interactive circuit. Initialization and signal refresh work MUST honor component disposal cancellation before starting realtime work or requesting a render.

#### Scenario: Team summary is unavailable
- **WHEN** the current user's team summary cannot be loaded because the API is unavailable
- **THEN** stale private data is cleared and the page shows a recoverable unavailable state
- **AND** unrelated navigation remains usable

#### Scenario: Team realtime connection is unavailable
- **WHEN** the team realtime connection cannot start
- **THEN** team management remains usable with available REST-backed data
- **AND** the connection failure does not block initial page rendering
