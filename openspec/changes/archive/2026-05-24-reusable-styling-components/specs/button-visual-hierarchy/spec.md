## ADDED Requirements

### Requirement: Shared button styling preserves visual parity during reuse
The site SHALL allow pages to adopt centralized shared button styling without introducing intentional user-visible changes to the current primary and supporting button presentation.

#### Scenario: A page migrates to the shared button foundation
- **WHEN** a page replaces page-local button styling with the shared button foundation
- **THEN** the rendered primary and supporting buttons keep the same visitor-facing visual hierarchy and interaction meaning they had before the migration
- **AND** the migration does not introduce a deliberate redesign of the button family

### Requirement: Shared button styling supports local layout overrides
The site SHALL let page-scoped CSS adjust button placement, grouping, spacing, or alignment without redefining shared button state styling.

#### Scenario: A page needs custom button placement
- **WHEN** a page needs its buttons arranged differently from another page
- **THEN** the page can change button layout or spacing in scoped CSS
- **AND** the shared hover, active, selected, and disabled button treatments remain owned by the centralized shared styling layer
