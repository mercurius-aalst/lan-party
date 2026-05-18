## ADDED Requirements

### Requirement: Home page hero creates a strong landing impression

The home page SHALL present a visually striking hero that acts as the primary entry experience for the site.

#### Scenario: Home hero is visually prominent
- **WHEN** a visitor lands on the home page
- **THEN** the hero presents a more eye-catching visual composition than a simple text block with a static supporting panel
- **AND** the hero remains aligned with the site's existing visual language

#### Scenario: Home hero uses motion or layered visual treatment
- **WHEN** the home page hero is rendered
- **THEN** it uses imagery, motion, layered composition, or a comparable visual treatment to create a stronger sense of impact
- **AND** the presentation remains compatible with reduced-motion preferences

#### Scenario: Home hero fills the first viewport and hands off smoothly to page content
- **WHEN** a visitor lands on the home page and starts scrolling
- **THEN** the hero occupies the initial viewport height
- **AND** the hero is presented as a full-bleed screen rather than a framed card or panel
- **AND** the transition into the following homepage sections feels visually continuous rather than abruptly cut off

### Requirement: Sponsors page opens with a simple visitor-facing intro

The sponsors page SHALL use a concise page intro instead of a heavyweight hero treatment.

#### Scenario: Visitor opens sponsors page
- **WHEN** a visitor opens the sponsors page
- **THEN** the top section explains what the sponsors page is for and what value the visitor can expect from it
- **AND** the page does not present a large showcase hero or internal-style metric summary ahead of the sponsor content

#### Scenario: Sponsors intro guides the next action
- **WHEN** the sponsors page intro is shown
- **THEN** it includes a clear visitor-facing call to action or next step
- **AND** that call to action is relevant to sponsor discovery or sponsor engagement

### Requirement: Games overview page opens with simple visitor-facing orientation

The games overview page SHALL use a concise page intro instead of a dashboard-style hero treatment.

#### Scenario: Visitor opens games overview page
- **WHEN** a visitor opens the games overview page
- **THEN** the top section explains what the page helps them do
- **AND** it focuses on useful tournament orientation for regular visitors

#### Scenario: Games intro avoids dashboard-first framing
- **WHEN** the games overview page is rendered
- **THEN** internal-style event snapshot metrics are not the primary supporting content at the top of the page
- **AND** the page instead emphasizes visitor-facing context or actions
