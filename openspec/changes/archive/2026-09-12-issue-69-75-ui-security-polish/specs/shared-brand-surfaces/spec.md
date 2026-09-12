## ADDED Requirements

### Requirement: Sponsor logo treatments preserve artwork and theme contrast

Shared sponsor cards and carousel items MUST keep supplied logo artwork
recognizable while providing a readable theme-independent containing surface.
The treatment MUST work with transparent assets and MUST NOT blanket-recolor,
invert, or otherwise unnecessarily alter sponsor branding.

#### Scenario: A light or dark logo is rendered in either theme

- **WHEN** a sponsor card or carousel item renders a light logo on a light
  surface or a dark logo on a dark surface
- **THEN** its containing surface or backplate MUST provide sufficient visual
  contrast for the logo to remain clearly visible
- **AND** the logo MUST remain readable in both light and dark modes

#### Scenario: A transparent logo asset is rendered

- **WHEN** a sponsor supplies a logo with transparent edges or internal space
- **THEN** the containing treatment MUST preserve the asset's colors, shape, and
  transparency
- **AND** the asset MUST retain accessible alternative text identifying the
  sponsor

#### Scenario: Sponsor surfaces change size responsively

- **WHEN** sponsor cards or carousel items render at any supported breakpoint
- **THEN** the same contrast and artwork-preservation treatment MUST remain
  effective
- **AND** logo links, labels, and focus states MUST remain usable
