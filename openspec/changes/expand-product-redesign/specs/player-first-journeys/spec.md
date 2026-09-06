## ADDED Requirements

### Requirement: Player goals have discoverable entry points

The front end SHALL organize public and authenticated entry points around the
player goals of discovering tournaments, understanding the event, registering,
managing a team, following matches, and finding public participants.

#### Scenario: Anonymous visitor starts exploring

- **WHEN** an anonymous visitor opens the site
- **THEN** the shell MUST expose clear paths to tournament discovery, event
  information/tickets, sponsors, and public search
- **AND** the visitor MUST NOT be shown authenticated mutation controls

#### Scenario: Authenticated player starts a next action

- **WHEN** an authenticated player opens the shell or a player-facing page
- **THEN** the UI MUST expose the existing profile, team-management,
  notification, registration, and logout actions appropriate to that user's
  authorization
- **AND** the UI MUST preserve the existing route and service callback for each
  selected action

### Requirement: Journey handoffs preserve canonical context

Player-facing pages SHALL expose contextual next-action links and local section
navigation without replacing canonical route URLs, notification targets, public
profile links, tournament fragments, or validated account return destinations.

#### Scenario: Visitor moves from discovery to tournament detail

- **WHEN** a visitor selects a tournament from a browse card, search result, or
  contextual next-action link
- **THEN** the application MUST navigate to the existing canonical GUID-based
  tournament detail route
- **AND** it MUST preserve the loaded tournament identifier and existing route
  behavior
- **AND** it MUST not convert the link to a name/slug route or require route
  resolution

#### Scenario: Visitor opens a participant identity in a contextual list

- **WHEN** a user is present in team management, a tournament participant list,
  or a tournament team lineup
- **THEN** selecting that user MUST open a privacy-safe information popup instead
  of navigating to the public user route
- **AND** the popup MUST use only returned names and linked IDs and MUST not
  request private enrichment

#### Scenario: Visitor follows a public profile relationship

- **WHEN** a public user or team profile presents a returned related team,
  tournament, or match context
- **THEN** the UI MAY link to the corresponding canonical public route
- **AND** it MUST not request private enrichment solely to create the link

### Requirement: API-coupled actions retain confirmed outcomes

Journey composition MUST keep login/logout, registration, tournament enrollment
and withdrawal, team creation and management, invites, profile updates, sponsor
mutations, and organizer mutations attached to their existing service calls and
backend-authoritative success refreshes.

#### Scenario: Player completes a mutation from a reorganized journey

- **WHEN** an authenticated user confirms an existing supported registration,
  team, invite, profile, or organizer action
- **THEN** the application MUST invoke the same service operation and request
  contract as the current flow
- **AND** the UI MUST show success only after the confirmed response and existing
  refresh behavior

#### Scenario: Player mutation is rejected

- **WHEN** the backend rejects or cannot complete a journey action
- **THEN** the UI MUST retain the last confirmed state
- **AND** it MUST show the existing recoverable error or validation feedback
  without claiming that the action succeeded

### Requirement: Journey composition respects role and privacy boundaries

The front end SHALL keep public discovery separate from authenticated player
state and administrator controls while allowing each authorized role to reach
the actions already available to it.

#### Scenario: Anonymous visitor views a protected journey

- **WHEN** an anonymous visitor reaches a protected player or organizer route
- **THEN** the application MUST retain its existing authorization or sign-in
  state
- **AND** it MUST NOT call authenticated or administrator data endpoints

#### Scenario: Public profile is reached from discovery

- **WHEN** a visitor follows a public user or team handoff
- **THEN** the page MUST render only the privacy-safe projection already returned
  by the public resource
- **AND** invites, email, Auth0 identifiers, roles, and other private fields MUST
  remain unavailable
