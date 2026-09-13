## ADDED Requirements

### Requirement: Sponsor carousel adapts to available sponsor count

The reusable sponsor carousel used by the Home sponsor showcase MUST adapt its
layout and movement to the number of available sponsors and the active
responsive capacity. It MUST use bounded tiles rather than stretching sparse
content across the full track.

#### Scenario: One to four sponsors fit in the visible carousel

- **WHEN** the available sponsor items fit within the visible capacity at the
  active breakpoint
- **THEN** the items MUST be centered in a compact static layout
- **AND** the carousel MUST NOT auto-scroll or introduce unnecessary movement
- **AND** a wide layout SHOULD show up to four bounded sponsor tiles across when
  the available width allows

#### Scenario: A single sponsor is available

- **WHEN** exactly one sponsor is available
- **THEN** the sponsor MUST render as one intentional compact centered tile
- **AND** it MUST NOT appear as a stretched full-width slide or an empty-looking
  marquee

#### Scenario: More sponsors are available than fit

- **WHEN** the sponsor count exceeds the visible capacity at the active
  breakpoint
- **THEN** the carousel MAY move the sponsor track to reveal the remaining
  items
- **AND** links, accessible sponsor names, focus behavior, and existing hover
  pause behavior MUST remain available
- **AND** reduced-motion preferences MUST settle the presentation to a usable
  static equivalent

#### Scenario: Sponsor carousel reaches a supported breakpoint

- **WHEN** the viewport changes between supported responsive breakpoints
- **THEN** tile sizing and visible capacity MUST adapt without clipping or
  page-wide horizontal scrolling
- **AND** the carousel MUST switch between static and moving behavior according
  to the capacity at that breakpoint
