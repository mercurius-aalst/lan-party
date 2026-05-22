# button-visual-hierarchy Specification

## Purpose
Defines the shared visual hierarchy for primary, secondary, and utility-style buttons across the site, including stronger supporting-action states and preserved call-to-action emphasis.

## Requirements

### Requirement: Supporting buttons use solid interaction states
The site SHALL render shared supporting buttons with hover, active, and selected treatments that read as deliberate solid controls rather than translucent or washed-out surfaces.

#### Scenario: Visitor hovers a shared supporting button
- **WHEN** a visitor hovers a button that uses a shared supporting button class such as a secondary or outline treatment
- **THEN** the control shows stronger surface or border contrast than its resting state
- **AND** the button label remains clearly legible
- **AND** the control does not appear semi-transparent against the page background

#### Scenario: Supporting button is shown in a selected state
- **WHEN** a shared supporting button or button-like control is rendered in an active, open, or selected state
- **THEN** it uses a stable selected treatment that is visually stronger than its resting state
- **AND** that selected treatment remains visually aligned with the supporting-button style family
- **AND** the selected treatment does not visually collapse into the primary CTA treatment

### Requirement: Primary call-to-action buttons remain visually dominant
The site SHALL preserve a higher-emphasis visual treatment for primary CTA buttons than for supporting or utility buttons after the shared button styling update.

#### Scenario: Primary CTA appears next to supporting actions
- **WHEN** a primary CTA is displayed adjacent to supporting buttons
- **THEN** the primary CTA has stronger visual prominence than the supporting actions
- **AND** supporting buttons remain identifiable as interactive controls without matching the CTA emphasis

#### Scenario: Shared button styles are reused across pages
- **WHEN** pages render both primary and supporting shared button classes after the styling update
- **THEN** the primary button treatment remains the most visually assertive action style
- **AND** supporting button treatments remain visually consistent with one another across those pages
