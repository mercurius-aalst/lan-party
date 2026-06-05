# games-overview-browse-experience Specification Delta

## MODIFIED Requirements

### Requirement: Games overview presents a unified tournament browse surface

The games overview page SHALL combine its page heading, search field, sort control, tournament filters, and available API-backed schedule context into one cohesive browse surface at the top of the page.

#### Scenario: Visitor opens games overview
- **WHEN** a visitor opens games overview
- **THEN** the page heading, search control, sort control, filter controls, and any top-level planned schedule context are presented together in the same top browsing surface
- **AND** the controls read as one coordinated interface rather than isolated stacked sections

#### Scenario: Visitor browses on smaller screens
- **WHEN** the games overview page is rendered on a narrow viewport
- **THEN** the browse controls and schedule context remain part of the same unified surface
- **AND** the stacked mobile layout remains visually grouped and usable without horizontal scrolling

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
