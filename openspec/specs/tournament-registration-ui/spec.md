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

The page MUST expose the authenticated pending roster confirmation action and MUST distinguish
captain-owned actions from member-owned state.

#### Scenario: Selected member has a pending confirmation

- **WHEN** an authenticated user has a pending roster confirmation
- **THEN** the page MUST show the tournament and roster-member context returned by the backend
- **AND** the user MUST be able to submit the supported `Confirmed` action
- **AND** the page MUST refresh state after the confirmation response

#### Scenario: Confirmation is no longer available

- **WHEN** the backend no longer returns a pending confirmation
- **THEN** the page MUST hide the confirmation action and show the refreshed state

#### Scenario: Confirmed member views a team registration

- **WHEN** a confirmed non-captain roster member views their team registration
- **THEN** the page MUST NOT show a self-unregister action
- **AND** the page MUST explain that roster changes are managed by the captain or an administrator

#### Scenario: Confirmed member remains visible on a pending team roster

- **WHEN** the authenticated member has confirmed their roster place while another selected member
  remains pending
- **THEN** the page MUST use the authenticated current-team registration context to show the team,
  pending roster status, and current confirmation states
- **AND** the page MUST NOT show a self-unregister action for the confirmed member
- **AND** the public participant projection MUST remain unchanged until the backend marks the team
  registration active

#### Scenario: Captain unregisters a team

- **WHEN** a captain chooses to unregister a team before tournament start
- **THEN** the page MUST ask for explicit confirmation
- **AND** a successful no-content mutation MUST remove the pending or active team state after refresh

#### Scenario: Registration data is unavailable

- **WHEN** team summary, team eligibility, or roster eligibility cannot be loaded
- **THEN** the page MUST distinguish the unavailable state from an in-progress loading state
- **AND** the affected workflow MUST provide a retry action

### Requirement: Eligibility and state feedback are accessible and privacy-safe

The registration surface MUST show loading, empty, unauthorized, unavailable, and mutation-error
states while keeping public participant data separate from authenticated registration data.

#### Scenario: Eligibility returns reason codes

- **WHEN** a team or roster eligibility response contains reason codes
- **THEN** the page MUST render those reasons next to the affected team, member, or action
- **AND** the page MUST avoid replacing a specific reason with only generic failure text
- **AND** starting a new selected-team or roster eligibility check MUST clear stale team-level
  error text before displaying the new result

#### Scenario: Anonymous visitor views registration

- **WHEN** an anonymous visitor opens a tournament detail page
- **THEN** the page MUST show a sign-in prompt
- **AND** the page MUST NOT call authenticated registration or team-summary endpoints

#### Scenario: Public participants render

- **WHEN** public participants are rendered
- **THEN** only active privacy-safe registrations from the loaded tournament response MAY be shown
- **AND** pending roster members, confirmation identifiers, email addresses, auth identifiers, and
      other private account fields MUST NOT be rendered

#### Scenario: Mock mode renders registration

- **WHEN** mock backend mode is enabled
- **THEN** internal registration actions MUST use the same state semantics as live mode
- **AND** mock data MUST not require an external registration URL

#### Scenario: Realtime invalidation refreshes both projections

- **WHEN** a supported team or roster realtime event invalidates registration state
- **THEN** the page MUST refresh the public tournament projection and authenticated registration
  context
- **AND** an unsaved roster draft MUST be preserved while its selected team remains available
- **AND** if that team disappears, the page MUST warn the captain and clear the stale draft instead
  of restoring it if the team later reappears
- **AND** the invalidation handler MUST dispatch all component state work through the renderer
  context
- **AND** bursts of invalidations MUST be coalesced so only one registration refresh runs at a time
- **AND** a superseded refresh MUST NOT change loading, submission, or error state or emit a stale
  toast

#### Scenario: Backend event gap remains recoverable

- **WHEN** a backend mutation does not publish a roster invalidation event
- **THEN** the page MUST provide an explicit refresh action for the authenticated registration
  context
- **AND** the UI MUST NOT claim that realtime synchronization is guaranteed for that mutation
