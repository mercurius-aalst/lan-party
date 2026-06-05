# games-overview-browse-experience Specification

## Purpose
TBD - created by archiving change games-overview-redesign. Update Purpose after archive.

## Requirements

### Requirement: Games overview presents a unified tournament browse surface

The games overview page SHALL combine its page heading, search field, sort control, tournament filters, and available API-backed schedule context into one cohesive browse surface at the top of the page.

#### Scenario: Visitor opens games overview
- **WHEN** a visitor opens the games overview page
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
