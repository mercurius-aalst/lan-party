## ADDED Requirements

### Requirement: Homepage sponsor showcase uses an animated logo slider

The home page SHALL present sponsors in a unified animated logo slider instead of tiered sponsor card groups.

#### Scenario: Sponsors are available

- **WHEN** the home page loads and sponsor data is available
- **THEN** the page shows a sponsor showcase section with sponsor logos in a continuously animated slider
- **AND** the section does not render tier-specific sponsor card groups

#### Scenario: Sponsor showcase links to sponsor destinations

- **WHEN** a sponsor logo is shown in the homepage slider
- **THEN** the logo links to that sponsor's configured information URL
- **AND** the logo includes accessible text identifying the sponsor

#### Scenario: Users need full sponsor details

- **WHEN** users want more sponsor information than the homepage slider provides
- **THEN** the home page provides a link to the full sponsors page
