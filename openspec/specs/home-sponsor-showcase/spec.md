# home-sponsor-showcase Specification

## Purpose
TBD - created by archiving change replace-home-sponsor-cards-with-logo-slider. Update Purpose after archive.
## Requirements
### Requirement: Homepage sponsor showcase uses an animated logo slider

The home page SHALL present sponsors in a unified animated logo slider instead of tiered sponsor card groups.
The sponsor showcase SHALL appear before the feature cards section on the home page.
The sponsor showcase SHALL integrate its supporting text and homepage CTA into the same sponsor panel as the slider.
The sponsor showcase SHALL use layout, sizing, and edge treatments that make the slider feel visually complete within the home page design.

#### Scenario: Sponsors are available

- **WHEN** the home page loads and sponsor data is available
- **THEN** the page shows a sponsor showcase section with sponsor logos in a continuously animated slider
- **AND** the section does not render tier-specific sponsor card groups
- **AND** the sponsor showcase appears before the feature cards section

#### Scenario: Sponsor showcase uses a unified panel

- **WHEN** the sponsor showcase is rendered on the home page
- **THEN** the supporting sponsor copy and homepage CTA are presented inside the same visual panel as the slider
- **AND** the logo marquee retains most of the horizontal space in that panel

#### Scenario: Sponsor showcase is visually balanced

- **WHEN** the sponsor showcase is rendered with its supporting text content
- **THEN** the slider panel spacing is visually aligned with nearby homepage sections
- **AND** sponsor logo frames provide enough vertical space for logos to fit comfortably within their containers

#### Scenario: Sponsor showcase has softened marquee edges

- **WHEN** sponsor logos move through the animated slider
- **THEN** the left and right edges of the marquee use soft transitions
- **AND** the hard boundary of the moving track is not visually obvious during the animation

#### Scenario: Sponsor showcase links to sponsor destinations

- **WHEN** a sponsor logo is shown in the homepage slider
- **THEN** the logo links to that sponsor's configured information URL
- **AND** the logo includes accessible text identifying the sponsor

#### Scenario: Users need full sponsor details

- **WHEN** users want more sponsor information than the homepage slider provides
- **THEN** the home page provides a link to the full sponsors page

