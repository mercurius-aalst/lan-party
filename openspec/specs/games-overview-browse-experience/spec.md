# games-overview-browse-experience Specification

## Purpose
TBD - created by archiving change games-overview-redesign. Update Purpose after archive.
## Requirements
### Requirement: Games overview presents a unified tournament browse surface

The games overview page SHALL combine its page heading, search field, sort
control, tournament filters, available API-backed schedule context, and the most
relevant player next action into one cohesive browse surface at the top of the
page.

#### Scenario: Visitor opens games overview

- **WHEN** a visitor opens the games overview page
- **THEN** the page heading, search control, sort control, filter controls, any
  top-level planned schedule context, and the relevant browse/registration next
  action MUST be presented together in the same top browsing surface
- **AND** the controls MUST read as one coordinated interface rather than
  isolated stacked sections

#### Scenario: Visitor browses on smaller screens

- **WHEN** the games overview page is rendered on a narrow viewport
- **THEN** the browse controls, schedule context, and next action MUST remain
  part of the same unified surface
- **AND** the stacked mobile layout MUST remain visually grouped and usable
  without horizontal scrolling

### Requirement: Games overview uses API-backed tournament schedule timing

The games overview page SHALL use planned and estimated tournament timing from game list responses when schedule information is presented.

#### Scenario: Tournament has planned schedule timing
- **WHEN** a tournament card or browse result displays timing information
- **THEN** the page shows the planned tournament start time from the API-backed schedule fields
- **AND** the timing is labeled as planned or scheduled rather than an actual lifecycle start

#### Scenario: Tournament has an estimated end time
- **WHEN** a game list response includes an estimated tournament end time
- **THEN** the page may show the estimated end time as an estimate
- **AND** the page does not derive the estimate from unrelated actual lifecycle `EndTime` values

#### Scenario: Tournament schedule estimates are unavailable
- **WHEN** estimated timing fields are missing because matches have not been generated or the backend did not return estimates
- **THEN** the games overview continues to render the tournament card or result normally
- **AND** unavailable estimates are omitted or shown with a clear unavailable state instead of misleading fallback timestamps

### Requirement: Games overview removes redundant context callouts

The games overview page SHALL remove redundant visitor-orientation callouts once the unified browse surface is present.

#### Scenario: Visitor reaches the lower page sections
- **WHEN** a visitor scrolls through the games overview page
- **THEN** the page does not render a standalone `Need more context?` style callout between tournament guidance content and sponsor content
- **AND** the remaining supporting sections stay focused on tournament process or sponsor discovery

### Requirement: Games overview preserves admin tournament creation access

The games overview page SHALL preserve the admin-only add tournament card whenever an authenticated admin can view the tournament browse page.

#### Scenario: Admin sees add tournament card with matching tournaments
- **WHEN** an authenticated admin opens the games overview page and tournaments match the current filters
- **THEN** the add tournament card is visible in the tournament grid

#### Scenario: Admin sees add tournament card with no matching tournaments
- **WHEN** an authenticated admin opens the games overview page and no tournaments match the current filters or search
- **THEN** the add tournament card remains visible
- **AND** the empty state remains visible

#### Scenario: Non-admin does not see add tournament card
- **WHEN** a visitor without the admin role opens the games overview page
- **THEN** the add tournament card is not visible

### Requirement: Games overview uses the reusable sponsor marquee treatment

The games overview page SHALL present sponsors with the same animated marquee treatment used on the home page when sponsor data is available.

#### Scenario: Sponsors are available
- **WHEN** the games overview page loads and sponsor data is available
- **THEN** the page shows sponsor logos in a continuously animated marquee with the same linked logo-card treatment and softened marquee edges used on the home page
- **AND** each visible sponsor logo links to that sponsor's configured information URL
- **AND** sponsor logos include accessible text identifying the sponsor

#### Scenario: No sponsors are available
- **WHEN** the games overview page loads and sponsor data is unavailable or empty
- **THEN** the sponsor marquee is omitted
- **AND** the rest of the games overview layout continues to render normally

### Requirement: Tournaments overview keeps its general browse layout

The Tournaments overview MUST retain its general browse-first structure and
interaction model. Visual polish MAY improve hierarchy, spacing, contrast, and
motion, but the page MUST NOT be restructured into a thereality.nl-style
editorial composition or treated as the Home landing page.

#### Scenario: Visitor opens the Tournaments overview

- **WHEN** a visitor opens the Tournaments overview
- **THEN** search, sort, filters, results, pagination, and schedule context MUST
  remain part of the general browse experience
- **AND** the page MUST not replace that experience with a new marketing hero,
  bento story, or unrelated anchor rail

### Requirement: Browse results expose a clear next action

Each available tournament result SHALL expose the existing canonical detail or
registration path appropriate to its state without changing schedule labels,
filter semantics, admin visibility, or service behavior.

#### Scenario: Tournament can be explored

- **WHEN** a visitor views a published tournament result
- **THEN** the result MUST provide a clear path to its canonical detail route
- **AND** the result MUST retain the current status and schedule presentation

#### Scenario: No tournaments match

- **WHEN** search or filters produce no matching tournaments
- **THEN** the existing empty state MUST remain visible and recoverable
- **AND** an authorized admin's existing add-tournament entry MUST remain
  available according to the current rules

### Requirement: Overview Register opens the existing popup immediately

The Tournaments overview MUST retain its general browse layout while making an
available Register action an immediate entry point to the existing accessible
registration popup. Selecting Register MUST NOT first navigate to tournament
detail. Selecting the tournament identity or detail action remains a separate
canonical GUID route handoff.

#### Scenario: Visitor selects Register from an available result

- **WHEN** an authenticated visitor selects Register on an eligible tournament
  result
- **THEN** the existing registration popup MUST open immediately in the
  overview
- **AND** focus MUST move into the labelled popup and return to the Register
  trigger when it closes
- **AND** search, filters, results, pagination, and schedule context MUST remain
  part of the general browse surface

#### Scenario: Visitor selects tournament detail instead

- **WHEN** a visitor selects the tournament identity or detail action
- **THEN** the application MUST navigate to the canonical GUID detail route
- **AND** the detail page's separate Register action MUST remain available

### Requirement: Tournament browsing recovers from unavailable data

The games overview MUST remain navigable when its tournament request fails and MUST distinguish the unavailable state from loading and empty states.

#### Scenario: Tournament collection is unavailable
- **WHEN** the tournament collection request fails because the API is unavailable
- **THEN** the page shows a friendly recoverable unavailable state
- **AND** independently loaded sponsor content and site navigation remain usable

### Requirement: Tournament process steps expose clear descriptions

The Tournaments overview `How it works` section MUST show the localized
description for each process step and MUST give that supporting content a clear,
restrained hierarchy beneath its step heading.

#### Scenario: Visitor reads the process section

- **WHEN** the Tournaments overview renders the `How it works` section
- **THEN** each step MUST include its existing localized heading and a visible
  description of the action it explains
- **AND** the description MUST be easy to scan without competing with the step
  heading or adding unrelated content

#### Scenario: Visitor reads process steps in either theme or on mobile

- **WHEN** the process section renders in light or dark mode or at a supported
  narrow breakpoint
- **THEN** headings and descriptions MUST remain readable with clear separation
- **AND** the cards MUST remain inside their container without overlap or
  page-wide horizontal scrolling
