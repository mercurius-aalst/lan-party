## MODIFIED Requirements

### Requirement: Home page hero creates a strong landing impression

The home page SHALL present a visually striking hero that acts as the primary entry experience for the site. The hero SHALL keep event information visually grouped, aligned with the surrounding hero information elements, and smoothly connected to the following homepage content.

#### Scenario: Home hero is visually prominent
- **WHEN** a visitor lands on the home page
- **THEN** the hero presents a more eye-catching visual composition than a simple text block with a static supporting panel
- **AND** the hero remains aligned with the site's existing visual language

#### Scenario: Home hero uses motion or layered visual treatment
- **WHEN** the home page hero is rendered
- **THEN** it uses imagery, motion, layered composition, or a comparable visual treatment to create a stronger sense of impact
- **AND** the presentation remains compatible with reduced-motion preferences

#### Scenario: Home hero groups event information below the logo and title
- **WHEN** the home page hero is rendered
- **THEN** the event date display is grouped with the rest of the hero information rather than appearing above the logo
- **AND** the event date display uses a visual treatment consistent with the surrounding information elements
- **AND** the event title remains visible
- **AND** the smaller event description is not shown in the hero

#### Scenario: Home hero fills the first viewport and hands off smoothly to page content
- **WHEN** a visitor lands on the home page and starts scrolling
- **THEN** the hero occupies the initial viewport height
- **AND** the hero is presented as a full-bleed screen rather than a framed card or panel
- **AND** the bottom viewport fade has a subtle breathing effect when motion is allowed
- **AND** the transition from the fade into the white homepage content feels softened rather than abruptly cut off
- **AND** the scroll prompt label, arrow, and highlight animate as a synchronized cue that invites the visitor to continue down the page
- **AND** the transition remains visually continuous when reduced-motion preferences disable animation
