## MODIFIED Requirements

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

## ADDED Requirements

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
