## ADDED Requirements

### Requirement: Tournament process steps expose clear descriptions

The Tournaments overview `How it works` section MUST show the localized
description for each process step and MUST give that supporting content a clear,
restrained hierarchy beneath its step heading.

#### Scenario: Visitor reads the process section

- **WHEN** the Tournaments overview renders the `How it works` section
- **THEN** each step MUST include its existing localized heading and a visible
  description of the action it explains
- **AND** the description MUST be easy to scan without competing with the step
  heading or adding unrelated content

#### Scenario: Visitor reads process steps in either theme or on mobile

- **WHEN** the process section renders in light or dark mode or at a supported
  narrow breakpoint
- **THEN** headings and descriptions MUST remain readable with clear separation
- **AND** the cards MUST remain inside their container without overlap or
  page-wide horizontal scrolling
