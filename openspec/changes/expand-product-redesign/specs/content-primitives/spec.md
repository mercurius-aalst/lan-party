## ADDED Requirements

### Requirement: Product pages use shared semantic framing

The front end SHALL provide reusable page headers, section headers, local section
navigation, action groups, and surface primitives so comparable public, player,
and organizer content has consistent hierarchy without duplicating page-specific
behavior.

#### Scenario: A page renders a primary section

- **WHEN** a public, authenticated, or organizer page renders a primary content
  section
- **THEN** the section MUST expose a clear heading, purpose, and relevant action
  group through the shared framing language
- **AND** page-specific CSS MAY change layout without redefining the semantic
  interaction contract

### Requirement: Entity identity primitives preserve safe navigation or safe details

Shared entity primitives SHALL render users, teams, tournaments, sponsors, and
matches with the safe display fields and either the canonical link or the
privacy-safe details trigger selected by the containing page.

#### Scenario: Entity has a public route identifier

- **WHEN** a loaded entity includes the public identifier required for its route
- **THEN** the shared identity surface MUST render the display label and link to
  the canonical public destination
- **AND** it MUST preserve existing keyboard and accessible-name behavior

#### Scenario: Entity route identifier is absent

- **WHEN** an entity lacks a usable public route identifier
- **THEN** the primitive MUST render the safe display label without a broken link
- **AND** it MUST NOT infer an identifier or issue an enrichment request

#### Scenario: Entity appears in a contextual user list

- **WHEN** a user appears in team management, a tournament participant list, or
  a tournament team lineup and the page selects popup behavior
- **THEN** the primitive MUST render a keyboard-usable details trigger rather
  than a public-user navigation link
- **AND** the trigger MUST open only the returned names and linked IDs without
  private enrichment

### Requirement: Shared action groups expose confirmed interaction states

Reusable action groups SHALL distinguish primary, supporting, destructive,
disabled, busy, and completed states while leaving callback ownership with the
page or existing service wrapper.

#### Scenario: Action is pending

- **WHEN** an existing API-coupled action is awaiting confirmation or response
- **THEN** its control MUST expose a busy or disabled state
- **AND** the action group MUST prevent duplicate submissions without changing
  the underlying callback

#### Scenario: Action succeeds or fails

- **WHEN** the existing callback completes
- **THEN** the action group MUST return to the confirmed state supplied by the
  page
- **AND** success, validation, conflict, unauthorized, or error feedback MUST
  remain visible through the existing toast or state surface

### Requirement: Shared data-state primitives cover the full state model

Reusable state surfaces SHALL represent loading, empty, error, unauthorized,
forbidden, not-found, conflict, and successful confirmation states with recovery
actions where the current flow supports recovery.

#### Scenario: A page state changes while loading

- **WHEN** a page request is pending, empty, fails, or is unauthorized
- **THEN** the state surface MUST identify that state without rendering stale
  data as current
- **AND** the page MUST keep unrelated navigation usable

#### Scenario: A state offers recovery

- **WHEN** the current flow has a retry, sign-in, back, or known-destination
  recovery action
- **THEN** the shared state MUST present that action with an accessible label
- **AND** the action MUST use the existing route or callback
