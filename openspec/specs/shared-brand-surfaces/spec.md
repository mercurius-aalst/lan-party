# shared-brand-surfaces Specification

## Purpose
Defines reusable shared styling foundations for sponsor cards, tournament cards, and branded section shells while preserving the current visitor-facing UI during adoption.
## Requirements
### Requirement: Shared brand surfaces preserve current presentation during reuse
The site SHALL provide reusable shared styling foundations for sponsor cards, tournament cards, and branded section shells without introducing intentional user-visible redesigns to the affected pages.

#### Scenario: Existing page surfaces adopt the shared foundation
- **WHEN** an affected page migrates a sponsor card, tournament card, or branded section shell onto the shared styling foundation
- **THEN** the rendered surface keeps the same user-facing visual identity, hierarchy, and purpose it had before the migration
- **AND** the migration does not require visitors to relearn the surface

### Requirement: Shared brand surfaces support page-level overrides
The site SHALL let page-scoped CSS adjust spacing, arrangement, sizing, and positioning of shared brand surfaces without redefining the full underlying visual treatment.

#### Scenario: A page needs layout-specific tuning
- **WHEN** a page needs a shared sponsor card, tournament card, or section shell to fit a page-specific layout
- **THEN** the page can override documented surface-level layout or spacing hooks in scoped CSS
- **AND** the shared surface retains its common visual styling contract

### Requirement: Sponsor and tournament card treatments are reusable surface families
The site SHALL expose sponsor card and tournament card styling as named shared surface families that can be applied across pages instead of being reimplemented inside each page stylesheet.

#### Scenario: A page renders sponsor cards with the shared family
- **WHEN** a page renders sponsor display cards using the shared sponsor card surface family
- **THEN** the page uses the shared sponsor-card styling contract for the card shell and its core visual treatment
- **AND** page-local CSS only needs to supply page-specific composition or variant tuning

#### Scenario: A page renders tournament cards with the shared family
- **WHEN** a page renders tournament cards using the shared tournament card surface family
- **THEN** the page uses the shared tournament-card styling contract for the card shell and its core visual treatment
- **AND** page-local CSS only needs to supply page-specific composition or variant tuning

### Requirement: Branded section shells are reusable across pages
The site SHALL expose recurring branded section-shell styling through shared classes or equivalent shared styling primitives so pages can reuse the same section treatment without duplicating page-local definitions.

#### Scenario: A page uses a shared branded section shell
- **WHEN** a page adopts the shared branded section-shell styling
- **THEN** the section uses the same shared border, radius, spacing, and background treatment defined by the shared styling layer
- **AND** the page can still tune section placement or surrounding layout in scoped CSS

### Requirement: Image upload fields use branded picker affordances
Image upload fields SHALL use the shared branded native upload picker treatment instead of exposing default browser file controls.

#### Scenario: A page renders an image upload field
- **WHEN** an admin or authenticated user can upload an image such as a sponsor logo, game image, or team logo
- **THEN** the upload field MUST show the branded dashed picker surface with an image icon and action label
- **AND** the native browser file input MUST NOT visually overlap or obscure the branded picker
- **AND** the selected filename or empty-file state MUST remain visible near the picker
- **AND** existing upload validation, accepted image types, and form binding behavior MUST remain intact

