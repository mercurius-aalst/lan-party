## MODIFIED Requirements

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

## ADDED Requirements

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
