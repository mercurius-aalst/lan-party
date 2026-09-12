## ADDED Requirements

### Requirement: Sponsors page prioritizes current sponsor content

The public Sponsors page MUST give available current sponsors stronger visual
and viewport priority than its introductory or explanatory copy while keeping a
coherent visitor-facing page hierarchy at every supported breakpoint.

#### Scenario: Visitor opens the Sponsors page with sponsors available

- **WHEN** sponsor data is available
- **THEN** the current sponsor content MUST appear earlier in the page flow and
  MUST be the primary visual attraction
- **AND** the top introduction MUST remain useful while using less vertical
  weight than a large hero treatment
- **AND** existing SponsorService data, tier ordering, sponsor links, and
  accessible labels MUST remain unchanged

#### Scenario: Visitor reads why sponsors matter

- **WHEN** the `Why sponsors matter` section is rendered
- **THEN** the redundant extra description paragraph MUST be omitted
- **AND** the section MUST retain useful value content that explains the role
  of sponsors without visual noise
- **AND** the existing become-a-sponsor action MUST be available in that
  section at a logical point in the reading order

#### Scenario: Visitor reaches a sparse or empty sponsor state

- **WHEN** sponsor data is empty, loading, or unavailable
- **THEN** the page MUST retain its existing loading, empty, or error meaning
- **AND** the available sponsorship action MUST remain discoverable
- **AND** the responsive layout MUST not create page-wide overflow
