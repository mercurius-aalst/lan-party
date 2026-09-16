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

### Requirement: Detail Register remains a separate popup action

The tournament detail page MUST retain a distinct Register action in its detail
context. Selecting it MUST open the existing accessible registration popup
without changing the GUID route or rendering a second inline workflow. This
detail action is separate from the overview's immediate Register action.

#### Scenario: Visitor registers from tournament detail

- **WHEN** a visitor selects Register on the tournament detail page
- **THEN** the existing registration popup MUST open from the detail action
- **AND** the detail identity, GUID route, current context, and surrounding
  actions MUST remain available after dismissal or failure

### Requirement: Match sections retain actions and states without read-only prose

Tournament match sections MUST retain real match actions, status, schedule
context, links, loading, empty, and error states. Read-only explanatory prose
that does not change a decision, action, or state MUST be omitted.

#### Scenario: Visitor views a match section

- **WHEN** a visitor opens a match schedule, bracket, result, or match dialog
- **THEN** the real match context, statuses, actions, and recovery states MUST
  remain available
- **AND** redundant read-only explanatory paragraphs MUST not be rendered

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

#### Scenario: User details dialog opens above an existing participant surface

- **WHEN** a visitor selects a user from an already open participant or
  team-roster dialog
- **THEN** the user details dialog MUST become the active topmost surface
- **AND** keyboard focus MUST move into the user details dialog
- **AND** the user details dialog MUST be dismissible through its close action
  or Escape
- **AND** the underlying participant or team-roster surface MUST remain below
  the user details dialog without covering it or trapping interaction
- **AND** closing the user details dialog MUST leave a coherent focus and state
  destination in the underlying participant surface
- **AND** a zero-value eyebrow or summary line that merely repeats the dialog
  context MAY be omitted
- **AND** the team name MUST remain the accessible dialog heading wherever the
  participant or team-roster surface identifies a team

### Requirement: Tournament detail match cards are breathable and aligned

Match cards and match-summary cards on tournament detail MUST provide
comfortable internal padding and aligned identity, schedule, result, status,
and action content at supported breakpoints. The correction MUST preserve the
existing match data, links, mutation callbacks, status semantics, and contained
overflow behavior.

#### Scenario: Visitor views tournament match cards

- **WHEN** a schedule, bracket result, or match-summary card is rendered
- **THEN** its text and controls MUST have enough internal spacing to avoid a
  compressed appearance and MUST align consistently within the card
- **AND** match identity, schedule, status, actions, links, loading, empty, and
  error states MUST remain available
- **AND** the card MUST remain readable without page-wide horizontal overflow at
  desktop and narrow supported widths
