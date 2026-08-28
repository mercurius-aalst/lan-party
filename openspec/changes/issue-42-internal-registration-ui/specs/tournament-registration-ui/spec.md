# Tournament registration UI Specification

## Purpose

Defines the internal tournament registration experience on the Blazor tournament detail page. The
backend registration API remains authoritative for eligibility, authorization, duplicate
participation, roster confirmation, and state transitions.

## ADDED Requirements

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
- **AND** the selected team MUST be checked against backend team eligibility
- **AND** an ineligible team MUST be disabled or clearly marked with its reason

#### Scenario: Captain selects a roster

- **WHEN** the captain reaches Step 2
- **THEN** the page MUST list members from the selected team
- **AND** the configured tournament team size MUST be shown as the exact number required,
      including the captain
- **AND** the captain MUST remain selected
- **AND** ineligible members MUST be disabled or clearly marked with their backend reason
- **AND** the captain MUST NOT progress to review until the local roster constraints pass

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

#### Scenario: Captain unregisters a team

- **WHEN** a captain chooses to unregister a team before tournament start
- **THEN** the page MUST ask for explicit confirmation
- **AND** a successful no-content mutation MUST remove the pending or active team state after refresh

### Requirement: Eligibility and state feedback are accessible and privacy-safe

The registration surface MUST show loading, empty, unauthorized, unavailable, and mutation-error
states while keeping public participant data separate from authenticated registration data.

#### Scenario: Eligibility returns reason codes

- **WHEN** a team or roster eligibility response contains reason codes
- **THEN** the page MUST render those reasons next to the affected team, member, or action
- **AND** the page MUST avoid replacing a specific reason with only generic failure text

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
