# tournament-registration-ui Specification

## Purpose
TBD - created by archiving change issue-42-internal-registration-ui. Update Purpose after archive.
## Requirements
### Requirement: Tournament registration stays inside the application

The tournament detail page MUST use the internal registration workflow and MUST NOT present an
external registration URL as the registration path.

#### Scenario: Visitor opens a scheduled tournament

- **WHEN** a visitor views a scheduled tournament detail page
- **THEN** the page MUST expose an in-application registration surface
- **AND** no registration action MAY navigate to Google Forms or another external form

#### Scenario: Visitor opens a closed tournament

- **WHEN** a tournament is no longer scheduled
- **THEN** the page MUST show registration as closed and MUST NOT offer a registration mutation

### Requirement: Individual registration requires authenticated confirmation

The page MUST let an eligible authenticated user register and unregister an individual tournament
registration through explicit confirmation and backend-authoritative refresh.

#### Scenario: Eligible user registers

- **WHEN** an eligible authenticated user chooses to register
- **THEN** the page MUST ask for explicit confirmation before sending the mutation
- **AND** a confirmed mutation MUST refresh the tournament and current-user registration state
- **AND** the page MUST show the state returned by the backend

#### Scenario: Registered user unregisters

- **WHEN** an eligible registered user chooses to unregister before tournament start
- **THEN** the page MUST ask for explicit confirmation before sending the no-content mutation
- **AND** a successful mutation MUST refresh the tournament and current-user registration state

#### Scenario: Individual mutation is rejected

- **WHEN** the backend rejects registration or unregistration
- **THEN** the page MUST show the backend error or a recoverable explanation
- **AND** the page MUST retain the last confirmed registration state

#### Scenario: A mutation becomes stale after navigation

- **WHEN** a user confirms a registration, unregister, roster submission, roster confirmation, or
  administrative removal action and the tournament route changes while confirmation or latest-state
  revalidation is in progress
- **THEN** the page MUST abort the action
- **AND** the page MUST NOT send the mutation for either the old or the newly displayed tournament

#### Scenario: Mutation succeeds but the follow-up refresh fails

- **WHEN** a registration mutation succeeds but the subsequent tournament or current-user refresh
  fails
- **THEN** the page MUST tell the user that the change was saved but the displayed state could not
  be refreshed
- **AND** the page MUST NOT present the refresh failure as a mutation failure

### Requirement: Team registration uses a three-step MudBlazor Stepper

An authenticated captain MUST use a MudBlazor Stepper with exactly three logical steps: team
selection, roster selection, and review/submit.

#### Scenario: Captain selects a team

- **WHEN** a captain opens team registration
- **THEN** Step 1 MUST list only teams the current user captains
- **AND** the selected team MUST be checked against backend team eligibility before the captain can
  continue
- **AND** eligibility for another team MAY be loaded when that team is selected rather than when
  the page first opens
- **AND** an ineligible team MUST be disabled or clearly marked with its reason

#### Scenario: Captain selects a roster

- **WHEN** the captain reaches Step 2
- **THEN** the page MUST list members from the selected team
- **AND** the configured tournament team size MUST be shown as the exact number required,
      including the captain
- **AND** the captain MUST remain selected
- **AND** ineligible members MUST be disabled or clearly marked with their backend reason
- **AND** the captain MUST NOT progress to review until the local roster constraints pass
- **AND** when a preserved dirty draft is rechecked, the current captain MUST be added without
  removing an existing selected member
- **AND** a roster made oversized by a captain transfer MUST remain invalid until the captain
  explicitly removes a member
- **AND** a selected draft member who is no longer in the current team projection MUST remain
  listed as removable until the draft is repaired or cleared

#### Scenario: Candidate discovery does not invalidate the selected roster

- **WHEN** a team has more members than the configured team size
- **THEN** roster eligibility MUST validate only the exact selected roster for progression and submit
- **AND** eligibility reasons for unselected candidates MUST be shown independently
- **AND** candidate-reason requests MUST be bounded to the backend roster-eligibility endpoint's
  maximum of 50 user ids per request and merged without dropping or duplicating candidates
- **AND** an existing registration containing a former team member MUST expose that member as
  removable or clearly explain the repair path
- **AND** eligibility-only candidates returned by the backend MUST remain available for removal
  when they are part of the selected draft

#### Scenario: Candidate discovery handles an oversized team projection

- **WHEN** the current team projection contains more than 50 possible roster candidates
- **THEN** the page MUST split candidate-reason discovery into backend-safe requests of at most 50
  user ids each because the endpoint rejects larger requests
- **AND** the selected exact roster MUST still be validated separately and remain eligible to
  advance when it satisfies the configured team size

#### Scenario: Captain reviews and submits

- **WHEN** the captain reaches Step 3
- **THEN** the page MUST summarize the team, roster, required size, and pending confirmation behavior
- **AND** submission MUST send the exact selected roster through the backend team roster route
- **AND** a pending response MUST remain visible as pending until all required confirmations complete

#### Scenario: Captain edits an existing registration

- **WHEN** a captain has a pending or active pre-start team registration
- **THEN** the page MUST load its current roster into the Stepper
- **AND** the captain MUST be able to submit an edited roster when backend rules allow it
- **AND** the UI MUST not block editing solely because the team is already registered

#### Scenario: Captain edits after captain transfer

- **WHEN** the saved roster identifies a former captain but the current team captain is a different
  member
- **THEN** the page MUST include the current captain in the selected roster before validation
- **AND** the page MUST NOT remove the former captain or another roster member automatically
- **AND** when the exact team size requires a removal, the page MUST mark the roster invalid and
  require the captain to choose who leaves
- **AND** the page MUST explain the adjustment and require the captain to review it before saving

### Requirement: Roster confirmation and ownership state are clear

The page MUST expose an authenticated pending roster selection as a player-owned decision, clearly
identify the tournament and team, and distinguish that decision from captain-owned roster editing.

#### Scenario: Selected member has a pending confirmation

- **WHEN** an authenticated user has a pending roster confirmation
- **THEN** the registration surface MUST prominently state that the user has been selected for the
  named team in the named tournament
- **AND** it MUST show the pending decision in plain user language without technical captain-workflow
  instructions
- **AND** the user MUST be able to choose accessible `Accept` or `Decline` actions directly from
  this state

#### Scenario: Selected member accepts their place

- **WHEN** the selected member chooses `Accept`
- **THEN** the front end MUST call the existing authenticated roster-member confirmation route with
  `confirmationStatus: Confirmed`
- **AND** it MUST refresh the current-user registration context and show the backend-authoritative
  result
- **AND** it MUST remove the pending decision actions when the selection is no longer pending

#### Scenario: Selected member declines their place

- **WHEN** the selected member chooses `Decline`
- **THEN** the front end MUST call the authenticated member-only roster-member `DELETE` route
- **AND** it MUST treat a successful `204` response as removal of that user's pending selection while
  leaving the captain's and other members' pending responses available
- **AND** it MUST refresh the current-user registration context and hide the declined decision

#### Scenario: Roster decision is unavailable or already handled

- **WHEN** the backend no longer exposes the pending selection, or an accept/decline action returns
  the backend's privacy-safe no-longer-actionable result
- **THEN** the front end MUST refresh and show the current registration state
- **AND** it MUST NOT leave stale Accept or Decline controls visible

#### Scenario: Roster decision mutation fails

- **WHEN** the backend rejects an accept or decline mutation because the tournament state,
  eligibility, authorization, or request is invalid
- **THEN** the front end MUST retain the last confirmed pending state
- **AND** it MUST show a localized, recoverable explanation
- **AND** it MUST NOT claim that the selection was accepted or declined

#### Scenario: Confirmed member views a team registration

- **WHEN** a confirmed non-captain roster member views their team registration
- **THEN** the page MUST NOT show a self-unregister or roster-edit action
- **AND** any ownership explanation MUST be concise, plain language, and limited to the captain or
  administrator managing roster changes

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

### Requirement: Roster selections are reload-safe account notifications

The authenticated account notification surface MUST expose pending roster selections from the
server-derived pending-roster endpoint alongside existing team invitations.

#### Scenario: Pending roster selections are loaded

- **WHEN** an authenticated user opens or refreshes the account notification surface
- **THEN** the front end MUST request `GET /v1/lan/tournament-roster-confirmations/me` using its
  supported pagination parameters
- **AND** it MUST load all available pages or provide an accessible continuation until all returned
  pending selections are available
- **AND** each item MUST identify the tournament, team, and selected roster member context supplied by
  the backend

#### Scenario: Pending roster selection survives a reload

- **WHEN** a roster selection was created while the user was offline, disconnected, or not viewing the
  application and the user later authenticates or reloads
- **THEN** the pending selection MUST appear from the authenticated notification load
- **AND** its discovery MUST NOT depend on having received the realtime event

#### Scenario: Bell provides roster quick actions

- **WHEN** a user opens a pending roster-selection notification
- **THEN** the notification MUST provide accessible quick `Accept` and `Decline` actions
- **AND** `Accept` MUST use the existing confirmation route while `Decline` MUST use the member-only
  roster `DELETE` route
- **AND** the notification MUST disable or guard duplicate action submissions until the action result
  is refreshed

#### Scenario: Notification action completes

- **WHEN** a roster quick action succeeds
- **THEN** the front end MUST refresh the pending-roster list, notification count, and current
  registration state that is visible in the active flow
- **AND** it MUST remove the no-longer-pending item from actionable notifications
- **AND** selecting the notification content MUST navigate to the corresponding tournament detail
  surface

#### Scenario: Realtime signal or reconnect occurs

- **WHEN** the supported roster confirmation realtime event or a realtime reconnect invalidates team
  state
- **THEN** the notification service MUST refresh the server-derived pending-roster list
- **AND** a missed or unavailable realtime event MUST remain recoverable through the next authenticated
  notification load

#### Scenario: Team invitations remain distinct

- **WHEN** team invitations and roster selections are present together
- **THEN** the account surface MUST retain both notification types with stable type-specific identity
  and their existing action semantics
- **AND** a roster Decline action MUST NOT call the team-invite endpoint
- **AND** a team-invite response MUST NOT remove a roster-selection item

#### Scenario: Pending notification data remains private

- **WHEN** public pages or anonymous users render tournament, team, or participant data
- **THEN** pending roster selections and notification identifiers MUST NOT be included
- **AND** only the authenticated account notification surface MAY render pending selection details

### Requirement: Roster selection messaging is localized and player-facing

All new roster-selection state, action, notification, loading, and error copy MUST be localized in
the supported en-US and nl-BE resources and MUST use the existing localization formatting contract.

#### Scenario: Player reads a roster selection

- **WHEN** a pending roster selection is rendered in either supported language
- **THEN** the player MUST see a clear statement that they were selected for a team and tournament
- **AND** the message MUST avoid raw placeholders, backend reason codes, and technical refresh terms
- **AND** Accept and Decline labels MUST remain understandable without relying on color alone
