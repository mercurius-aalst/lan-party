## MODIFIED Requirements

### Requirement: Eligibility and state feedback are accessible and privacy-safe

The registration surface MUST show loading, empty, unauthorized, unavailable,
conflict, and mutation-error states, make the current player/team ownership and
next step clear, and keep public participant data separate from authenticated
registration data.

#### Scenario: Eligibility renders only an unavailable state

- **WHEN** a team or roster eligibility response marks a team or member as
  unavailable
- **THEN** the affected option MUST render only a compact visual `Unavailable`
  state with an icon or accessible label
- **AND** validation reasons, raw reason codes, and explanatory paragraphs MUST
  not be rendered beside the option

#### Scenario: Anonymous visitor views registration

- **WHEN** an anonymous visitor opens a tournament detail page
- **THEN** the page MUST show a sign-in prompt as the next action
- **AND** the page MUST NOT call authenticated registration or team-summary
  endpoints

#### Scenario: Public participants render

- **WHEN** public participants are rendered
- **THEN** only active privacy-safe registrations from the loaded tournament
  response MAY be shown
- **AND** pending roster members, confirmation identifiers, email addresses,
  auth identifiers, and other private account fields MUST NOT be rendered

#### Scenario: Mock mode renders registration

- **WHEN** mock backend mode is enabled
- **THEN** internal registration actions MUST use the same state semantics as
  live mode
- **AND** mock data MUST not require an external registration URL

#### Scenario: Realtime invalidation refreshes both projections

- **WHEN** a supported team or roster realtime event invalidates registration
  state
- **THEN** the page MUST refresh the public tournament projection and
  authenticated registration context
- **AND** an unsaved roster draft MUST be preserved while its selected team
  remains available
- **AND** if that team disappears, the page MUST warn the captain and clear the
  stale draft instead of restoring it if the team later reappears
- **AND** the invalidation handler MUST dispatch all component state work through
  the renderer context
- **AND** bursts of invalidations MUST be coalesced so only one registration
  refresh runs at a time
- **AND** a superseded refresh MUST NOT change loading, submission, or error
  state or emit a stale toast

#### Scenario: Backend event gap remains recoverable

- **WHEN** a backend mutation does not publish a roster invalidation event
- **THEN** the existing backend-authoritative response and the next supported
  page or popup load MUST remain the source of truth
- **AND** the UI MUST NOT render `Check again`, `live update`, `refresh
  registration state`, or an equivalent technical availability control

#### Scenario: Player reaches a registration next step

- **WHEN** a visitor is eligible to register, confirm, edit, or withdraw
- **THEN** the UI MUST identify the supported next action and its ownership in
  plain user language
- **AND** selecting it MUST preserve the existing confirmation, mutation, and
  authoritative refresh flow

### Requirement: Tournament registration is an accessible popup workflow

The tournament detail page MUST expose a concise registration trigger and render
the registration workflow in an accessible modal popup rather than inline in the
page content.

#### Scenario: Visitor opens registration

- **WHEN** a visitor selects the tournament registration action
- **THEN** the workflow MUST open in a labelled dialog with a clear close action
- **AND** keyboard focus MUST move into the dialog, remain usable within it,
  and return to the trigger after dismissal
- **AND** the underlying tournament page MUST not render a second inline
  registration component

#### Scenario: Eligible captain selects a team

- **WHEN** an eligible captain selects a team in the team-registration popup
- **THEN** the selection MUST advance automatically to roster selection after
  eligibility is confirmed
- **AND** an unavailable team MUST stay selectable only as a visibly disabled or
  status-marked option with the visual label `Unavailable`
- **AND** validation reasons, raw reason codes, and explanatory paragraphs MUST
  not be rendered beside the option

#### Scenario: Eligible player registers individually

- **WHEN** an eligible authenticated player selects the individual registration
  action
- **THEN** the UI MUST use a concise action label without restating that the
  player is an individual
- **AND** the next interaction MUST be a friendly confirmation describing the
  tournament outcome before the existing mutation is sent

#### Scenario: User sees an unavailable roster option

- **WHEN** a team or roster member cannot be used for the tournament
- **THEN** the UI MUST communicate unavailability through disabled/status
  styling and an icon or label, not explanatory validation text
- **AND** the accessible label MUST communicate `Unavailable` without exposing
  raw backend reason codes or technical refresh terminology

#### Scenario: Unavailable roster members remain visible but excluded

- **WHEN** a roster member is marked unavailable or ineligible for the
  tournament
- **THEN** the member MUST remain visible in the roster selection
- **AND** the member's selection control MUST initialize unchecked and render
  as non-selectable/disabled with the accessible state `Unavailable`
- **AND** the member MUST be excluded from any restored or realtime-preserved
  roster draft
- **AND** the member MUST be excluded from the submitted roster even if a
  stale draft or client state previously contained that member
- **AND** eligible roster members MUST retain their existing selectable and
  checked-state semantics
- **AND** captain and team selection semantics MUST remain unchanged

### Requirement: Registration context recovers when the popup is reopened

The registration popup MUST automatically request the current registration
context again when a previous popup load failed or ended incomplete and the
player closes and reopens the popup. The recovery MUST remain behind the
existing popup entry point and MUST NOT add a user-facing technical retry
control.

#### Scenario: Player reopens after a failed popup load

- **WHEN** the registration context cannot be loaded completely and the player
  closes and reopens the registration popup
- **THEN** the popup MUST start a fresh registration-context load
- **AND** the popup MUST show the confirmed result of that fresh load rather
  than requiring a full page reload
- **AND** the UI MUST NOT render `Check again`, `Retry`, `Refresh`, or equivalent
  technical availability controls for this recovery

### Requirement: Popup-only registration loads avoid hidden administrator work

The registration component MUST avoid loading administrator-only registration
data when it is rendered as a popup-only surface. Public participants and the
authenticated registration context MUST keep their existing loading and state
semantics.

#### Scenario: Popup-only registration is opened by an administrator

- **WHEN** the registration component is rendered with popup-only behavior,
  including for an administrator, and its registration context loads
- **THEN** the component MUST NOT request the administrator registration list
- **AND** it MUST continue to load only the context required by the popup
  workflow

### Requirement: Superseded registration loads are cancellable and stale-safe

Registration-context loads MUST be cancellable when a newer load supersedes
them, the popup closes, or the component is disposed. The cancellation MUST be
passed through the existing registration, team, eligibility, roster, realtime,
and administrator service calls that participate in that load, while request
generation guards continue to prevent late results from changing current state.

#### Scenario: A registration load is superseded or disposed

- **WHEN** a newer registration load starts, the registration popup closes, or
  the component is disposed while a registration-context request is pending
- **THEN** the pending load MUST be cancelled and its cancellation token MUST
  reach the participating service calls
- **AND** a late completion from the cancelled load MUST NOT change loading,
  submission, error, or toast state for the current load
- **AND** the next supported popup load MUST remain available as the source of
  truth
