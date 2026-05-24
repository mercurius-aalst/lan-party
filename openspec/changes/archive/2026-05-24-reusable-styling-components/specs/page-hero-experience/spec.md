## ADDED Requirements

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
