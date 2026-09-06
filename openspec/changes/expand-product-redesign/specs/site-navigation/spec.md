## MODIFIED Requirements

### Requirement: Header action buttons preserve clear action hierarchy

The site SHALL organize header actions by player goal and role while styling
supporting account, search, and admin actions as lower-emphasis utility controls
and keeping the tickets action visually prominent when it is available.

#### Scenario: Visitor views signed-out header actions

- **WHEN** a visitor views the header while signed out
- **THEN** the header MUST expose clear paths to discovery, event information,
  sponsors, login, and registration according to the active account-access flow
- **AND** the tickets action MUST remain more visually prominent than supporting
  account actions

#### Scenario: Visitor views authenticated header actions

- **WHEN** an authenticated visitor views the header with the user menu trigger,
  team, notification, and tickets actions
- **THEN** the actions MUST be grouped by player goal without hiding any action
  authorized by the existing navigation behavior
- **AND** the tickets action MUST remain the most visually prominent header call
  to action

## ADDED Requirements

### Requirement: Primary navigation follows player goals without breaking routes

The navigation SHALL present a concise player-first information architecture for
discovering tournaments, understanding the event, finding public participants,
and reaching authenticated team/profile actions while preserving canonical route
destinations and existing navigation callbacks.

#### Scenario: Visitor selects a player-goal destination

- **WHEN** a visitor selects a primary, secondary, search, profile, team, or
  notification navigation item
- **THEN** the application MUST use the item's existing Blazor route or action
  callback
- **AND** any temporary mobile, dropdown, or search surface MUST close using its
  existing behavior

#### Scenario: Navigation is rendered for a role

- **WHEN** the shell renders for an anonymous user, authenticated player, or
  authenticated administrator
- **THEN** only the destinations authorized for that role MUST be shown
- **AND** reorganizing their grouping MUST NOT reveal protected data or remove a
  successful action from its canonical entry point

### Requirement: Navigation exposes contextual next actions

The site SHALL provide local section links or contextual next-action affordances
when they clarify a journey, but these affordances MUST remain additive to the
existing route, anchor, search, and keyboard contracts.

#### Scenario: Visitor scans a dense page

- **WHEN** a tournament, team, profile, or organizer page contains multiple
  sections
- **THEN** the page MAY expose local links to the loaded sections and next
  action when the content is dense enough to benefit from them
- **AND** a short page, including Home when its useful sections fit the page
  flow, MUST NOT render a redundant anchor rail or duplicate jump buttons
- **AND** the links MUST target existing route fragments or callbacks rather
  than inventing an unimplemented destination

### Requirement: Theme preference supports system default and explicit modes

The shell SHALL provide an accessible light/dark theme choice while keeping one
page-wide semantic theme at a time. When no explicit preference is stored, the
initial theme MUST follow the browser or operating-system preference. An
explicit user choice MUST persist across later visits when client-side
persistence is available, without requiring a new backend contract.

#### Scenario: Visitor changes the theme

- **WHEN** a visitor selects light or dark mode from the shell control
- **THEN** the current page MUST update to the selected theme without changing
  route, form data, or loaded journey state
- **AND** the control MUST expose its current mode and an accessible name
- **AND** all sections MUST remain in the selected theme rather than flipping
  individual sections independently

#### Scenario: Visitor has no stored theme preference

- **WHEN** a visitor opens the site without a stored explicit theme choice
- **THEN** the shell MUST use the current system color-scheme preference
- **AND** the first rendered state MUST remain readable while the preference is
  resolved

### Requirement: Home navigation reflects the concise landing information architecture

Home navigation and in-page entry points SHALL support only the useful landing
sections: Hero, Sponsors, Tournaments, Tickets, and Contact, in that order.
Existing compatibility anchors MAY remain available inside their relevant
section, but the shell MUST not add a second navigation rail solely to jump
between sections that are already visible in the page flow.

#### Scenario: Visitor opens Home

- **WHEN** a visitor opens the Home route
- **THEN** the page MUST present the five useful sections in the required order
- **AND** redundant feature, orientation, or filler sections MUST not be
  introduced as part of the redesign
- **AND** existing compatibility fragments MUST continue to resolve without
  requiring a visible anchor menu
