# page-hero-experience Specification

## Purpose
Defines how homepage and non-homepage hero or intro surfaces create a clear first impression, guide visitor orientation, and preserve appropriate visual hierarchy across the site.

## Requirements

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

The games overview page SHALL open with a browse-first orientation surface instead of a dedicated page intro or dashboard-style hero treatment.

#### Scenario: Visitor opens games overview page
- **WHEN** a visitor opens the games overview page
- **THEN** the page heading is presented together with tournament browsing controls in the first interactive surface
- **AND** visitors can begin searching, sorting, or filtering tournaments without scrolling past a separate hero or intro block

#### Scenario: Games intro avoids dashboard-first framing
- **WHEN** the games overview page is rendered
- **THEN** internal-style event snapshot metrics are not the primary supporting content at the top of the page
- **AND** the page emphasizes immediate tournament discovery actions over a standalone orientation panel

### Requirement: Non-homepage hero surfaces reuse a shared foundation without visible redesign
The site SHALL allow non-homepage page intro and hero surfaces to share a reusable styling foundation while preserving each affected page's current visitor-facing appearance.

#### Scenario: A non-homepage hero migrates to the shared foundation
- **WHEN** a non-homepage page intro or hero surface adopts the shared hero foundation
- **THEN** the page keeps the same visible hero or intro presentation it had before the migration
- **AND** the shared foundation replaces duplicated styling definitions rather than introducing a deliberate redesign

#### Scenario: A page needs hero-specific layout tuning
- **WHEN** a non-homepage page uses the shared hero foundation inside a page-specific layout
- **THEN** the page can adjust spacing, alignment, or positioning in scoped CSS
- **AND** the shared hero visual treatment remains the common base

### Requirement: Home page hero remains separate from the shared non-homepage hero foundation
The site SHALL keep the home page hero on its distinct existing styling path when non-homepage hero styling is centralized.

#### Scenario: Shared non-homepage hero styles are introduced
- **WHEN** the shared non-homepage hero foundation is added to the application
- **THEN** the home page hero does not adopt that shared non-homepage hero treatment
- **AND** the home page hero remains governed by its own existing experience requirements
