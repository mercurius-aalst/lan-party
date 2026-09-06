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
