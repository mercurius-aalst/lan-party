## MODIFIED Requirements

### Requirement: Game detail page presents tournament content as standalone sections

The game detail page SHALL present tournament content as direct standalone
sections in the page flow and SHALL add clear local section reachability for
overview, schedule, bracket, participants, and results without introducing a
generic tabbed `Tournament information` shell. Registration MUST be represented
by a concise action that opens the accessible popup defined by the registration
specification, not by an inline workflow.

#### Scenario: Visitor opens a tournament detail page

- **WHEN** a visitor opens a tournament detail page
- **THEN** the page MUST present standalone sections for tournament overview
  metadata, match schedule, bracket, participants, and results as applicable
- **AND** the page MUST provide an additive way to reach those loaded sections
  when the content is dense, without requiring a redundant details tab or
  navigation rail on a short page

#### Scenario: Visitor views tournament overview metadata

- **WHEN** the tournament overview section is rendered
- **THEN** it MUST show tournament setup and registration metadata such as
  status, participation mode, bracket type, schedule, and registration status
- **AND** it MUST not duplicate participant listings, bracket visualization, or
  match schedule listings that belong to other sections

#### Scenario: Visitor selects a tournament action

- **WHEN** a visitor or authorized participant selects an existing detail,
  registration, unregister, confirmation, or match action
- **THEN** the action MUST use its existing route, service callback, and
  backend-authoritative refresh behavior
- **AND** the surrounding section navigation MUST remain usable after the
  action completes or fails

## ADDED Requirements

### Requirement: Tournament detail makes current context persistent

The detail composition SHALL keep the tournament identity, current status, and
relevant section context visible enough for visitors to understand where an
action applies while preserving privacy-safe participant data.

#### Scenario: Visitor moves between detail sections

- **WHEN** a visitor moves between overview, schedule, bracket, participants,
  or results, or opens the registration popup
- **THEN** the active section or heading MUST be understandable from the current
  page context
- **AND** no section may load private enrichment data solely to render context

### Requirement: Tournament detail preserves canonical GUID routes

New and existing tournament links MUST use the canonical GUID-based detail route
`/tournaments/{guid}`. The redesign MUST NOT introduce name/slug route
generation or require route-resolution logic. Existing bookmarks, notifications,
and fragments MUST remain usable, and the GUID MUST remain the backend identifier
for the existing detail and mutation contracts.

#### Scenario: Visitor follows a tournament link

- **WHEN** a visitor selects a tournament from browse, search, profile, or a
  contextual action
- **THEN** the application MUST use the canonical GUID detail route for the
  selected tournament
- **AND** the route MUST preserve existing fragments without inventing a new
  route-resolution API or name/slug format

### Requirement: Tournament participant and lineup users open privacy-safe dialogs

The detail page MUST make selecting a user identity in tournament participants
or a tournament team lineup open a labelled information dialog instead of
navigating to the public user route.

#### Scenario: Visitor selects a tournament user

- **WHEN** a visitor selects a user in the participants list or team lineup
- **THEN** the dialog MUST show returned first name, last name, username, and
  available public linked IDs only
- **AND** missing linked IDs MUST be omitted, private fields MUST remain hidden,
  and no enrichment request MUST be issued
- **AND** focus, Escape, close, and focus restoration MUST be keyboard usable
