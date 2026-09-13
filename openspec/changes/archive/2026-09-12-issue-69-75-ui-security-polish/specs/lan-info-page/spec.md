## ADDED Requirements

### Requirement: Info ticket options use intentional visual hierarchy

The Info page MUST distinguish ticket options with intentional semantic color
and surface accents while preserving readable text, existing ticket content,
and the existing ticket-inspired presentation. Color MUST supplement labels or
other non-color cues rather than carrying the ticket distinction alone.

#### Scenario: Visitor compares tickets in either theme

- **WHEN** a visitor views the ticket section in light or dark mode
- **THEN** each ticket option MUST have a clear visual hierarchy that supports
  comparing the ticket name, description, and price
- **AND** the text and price MUST remain readable at accessible contrast
- **AND** labels or structure MUST still identify the options if color is not
  perceived

#### Scenario: Visitor views tickets on a narrow viewport

- **WHEN** the ticket section is rendered at a supported mobile or desktop
  breakpoint
- **THEN** the ticket options MUST remain inside their owning container
- **AND** the hierarchy MUST remain scannable without overlap, clipped prices,
  or page-wide horizontal scrolling
