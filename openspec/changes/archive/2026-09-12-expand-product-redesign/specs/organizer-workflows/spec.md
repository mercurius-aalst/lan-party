## ADDED Requirements

### Requirement: Organizer tools are grouped by authorized task

The front end SHALL group existing organizer capabilities around tournament
setup, schedule and lifecycle, registration oversight, sponsor placement, and
team administration without exposing those controls to users lacking the
existing administrator role.

#### Scenario: Admin opens organizer tools

- **WHEN** an authenticated administrator opens an organizer-facing route or
  menu
- **THEN** the UI MUST present the existing authorized tournament, sponsor,
  lifecycle, registration, and team-management entry points in task-oriented
  groups
- **AND** each entry point MUST retain its existing route and callback

#### Scenario: Non-admin views the same navigation

- **WHEN** an authenticated non-admin or anonymous visitor opens the shell
- **THEN** administrator-only entries and controls MUST remain hidden or
  unauthorized according to the existing authorization behavior
- **AND** public/player navigation MUST remain usable

### Requirement: Organizer mutations remain backend-authoritative

Organizer composition MUST preserve the current tournament create/update/delete,
lifecycle, sponsor placement, registration removal, team, and profile mutation
contracts and MUST refresh from the confirmed response before presenting the new
state.

#### Scenario: Admin submits an organizer mutation

- **WHEN** an authorized administrator confirms an existing organizer action
- **THEN** the front end MUST call the current service/API contract with the same
  request shape
- **AND** it MUST show the existing success feedback only after the mutation and
  authoritative refresh succeed

#### Scenario: Organizer mutation fails

- **WHEN** the backend returns validation, conflict, unauthorized, forbidden,
  not-found, or generic failure feedback
- **THEN** the UI MUST preserve the current data projection
- **AND** it MUST expose a recoverable message without optimistic mutation state

### Requirement: Organizer workflows preserve mock/live parity

Organizer task composition SHALL use the same service-level semantics in mock and
live modes, including representative loading, empty, success, blocked, conflict,
and error states.

#### Scenario: Admin validates a workflow in mock mode

- **WHEN** mock backend mode is enabled and an administrator exercises an
  organizer task
- **THEN** the UI MUST use the existing mock service behavior and response shapes
- **AND** the journey MUST remain usable without a live-only endpoint or field

### Requirement: Sponsor context stays subordinate to event tasks

Sponsor discovery and sponsor administration SHALL remain distinct: public
sponsor content is discoverable for visitors, while sponsor mutation controls
remain in the authorized organizer context.

#### Scenario: Visitor views sponsors

- **WHEN** a public visitor opens sponsor content
- **THEN** the page MUST show public sponsor information and configured links
- **AND** no sponsor mutation control or private organizer data MUST be shown

#### Scenario: Admin manages sponsor placement

- **WHEN** an authorized administrator edits an existing sponsor placement
- **THEN** the admin surface MUST retain the current zero-or-one placement
  semantics and refresh behavior
- **AND** the public sponsor presentation MUST update only from confirmed state
