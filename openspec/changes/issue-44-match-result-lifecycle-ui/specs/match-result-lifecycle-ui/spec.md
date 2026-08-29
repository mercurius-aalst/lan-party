# Match result lifecycle UI

## ADDED Requirements

### Requirement: State-aware match details

The match details UI MUST show the backend lifecycle state, both ended-confirmation statuses, current scores/reports, and any server-provided confirmation or correction deadline. A client countdown MUST be advisory and MUST NOT transition a match locally.

#### Scenario: Match is awaiting ended confirmation

- **WHEN** a player opens a match before both sides confirm ended
- **THEN** the UI MUST show which side has confirmed
- **AND** it MUST offer the eligible player/captain a confirm-ended action

#### Scenario: Both sides confirmed

- **WHEN** the backend reports that both sides confirmed ended
- **THEN** the score form MUST become available to eligible participants
- **AND** the UI MUST explain the server deadline after the first report

### Requirement: Consensus and dispute actions

The UI MUST allow an eligible participant/captain to submit a valid score only when the backend allows it. It MUST show pending confirmation, disputed, and admin-resolution-required states distinctly, including correction deadlines and an actionable refresh/error path. The UI MUST NOT claim a result is complete until a refreshed backend projection reports completion.

#### Scenario: Scores differ

- **WHEN** the backend reports a disputed score
- **THEN** the UI MUST show the differing reports and correction window
- **AND** it MUST allow eligible correction submission only while the backend state permits it

#### Scenario: Correction window expires

- **WHEN** the backend reports admin resolution required
- **THEN** normal score actions MUST be disabled
- **AND** admins MUST see a resolve action while other users see a clear explanation

#### Scenario: Protected action projection exposes both dispute reports

- **WHEN** an eligible participant or team captain opens a disputed match
- **THEN** the UI MUST show both authoritative reports from the protected action projection
- **AND** an unrelated authenticated viewer MUST see neither private report

### Requirement: Forfeit and administrative actions

The UI MUST require explicit confirmation before a participant or administrator forfeits, and before an administrator reverses a result. It MUST show actions only when the fresh protected action projection grants the corresponding capability, show admin resolution/force/reversal only to admins, and explain why an action is blocked when tournament state or linked downstream matches disallow it. An administrator assignment MAY scope private report visibility but MUST NOT suppress an eligible global-admin action. Team matches MUST identify captain-only actions.

#### Scenario: Participant forfeits

- **WHEN** an eligible participant confirms the forfeit dialog
- **THEN** the UI MUST invoke the backend forfeit command
- **AND** it MUST refresh the match and bracket before presenting the resulting state

#### Scenario: Reversal is unavailable

- **WHEN** the backend rejects reversal because downstream play has started
- **THEN** the UI MUST retain the current projection
- **AND** it MUST display the server-provided reason

### Requirement: Safe loading and error handling

The UI MUST show loading immediately when a match refresh or mutation begins, disable stale actions during that operation, and distinguish unauthorized, not-found, expired/invalid-state, and transient failures. A failed refresh after a successful mutation MUST be reported as “saved, but the displayed state could not be refreshed” while retaining the last known projection.

#### Scenario: Unauthorized viewer

- **WHEN** the backend rejects a protected action for the current identity
- **THEN** the UI MUST hide or disable the action and provide a sign-in/permission explanation

#### Scenario: Refresh failure after mutation

- **WHEN** a lifecycle command succeeds but the subsequent match refresh fails
- **THEN** the UI MUST communicate that the result was saved but the displayed state may be stale
- **AND** it MUST provide a retry action

#### Scenario: Protected refresh is forbidden

- **WHEN** the protected action refresh is unauthorized or forbidden
- **THEN** the UI MUST fetch and render the public match projection
- **AND** it MUST keep all mutations disabled while showing a distinct sign-in or permission explanation

### Requirement: Mock/live parity

The mock service MUST expose the same lifecycle DTOs, command methods, validation outcomes, and state transitions as the live client for happy paths, duplicate actions, disputes, deadlines, forfeits, admin resolution, and reversal guards.

#### Scenario: Mock match follows lifecycle actions

- **WHEN** a participant or administrator uses the mock match service
- **THEN** the mock MUST apply the same confirmation, score, dispute, forfeit, resolution, deadline, and reversal guards exposed by the live contract
- **AND** the mock MUST return explicit lifecycle state instead of inferring completion from a winner or score alone
