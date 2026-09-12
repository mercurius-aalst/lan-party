# shared-brand-surfaces Specification

## Purpose
Defines reusable shared styling foundations for sponsor cards, tournament cards, and branded section shells while preserving the current visitor-facing UI during adoption.
## Requirements
### Requirement: Shared brand surfaces preserve current presentation during reuse

The site SHALL provide reusable shared styling and composition foundations for
sponsor cards, tournament cards, branded section shells, entity identity, action
groups, and state surfaces. Reuse MAY improve hierarchy and journey clarity, but
MUST preserve each surface's user-facing purpose, safe fields, and interaction
outcome.

#### Scenario: Existing page surfaces adopt the shared foundation

- **WHEN** an affected page migrates a sponsor card, tournament card, branded
  section shell, entity identity, action group, or data-state surface onto the
  shared foundation
- **THEN** the rendered surface MUST keep its user-facing purpose, privacy
  boundaries, and successful callbacks
- **AND** the surface MAY adopt the landed product visual foundation and clearer
  journey hierarchy without requiring visitors to relearn its action outcome

#### Scenario: A shared surface is used in a new journey composition

- **WHEN** a player, public, or organizer page composes a shared surface in a
  new section order
- **THEN** the surface MUST retain its documented route, callback, state, and
  accessibility hooks
- **AND** page-local CSS MUST only supply layout or variant tuning rather than a
  competing visual system

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

### Requirement: Shared primitives have explicit semantic ownership

Each reusable content primitive SHALL have one clear responsibility and SHALL
leave data loading, authorization, and mutation ownership with the containing
page or existing service boundary.

#### Scenario: Primitive renders data supplied by a page

- **WHEN** a page supplies loaded data and callbacks to a shared primitive
- **THEN** the primitive MUST render that projection without issuing speculative
  API calls
- **AND** it MUST invoke only the callback supplied for the existing action

### Requirement: Shared surfaces use neutral dual-theme styling with intentional boundaries

Shared surfaces MUST use one semantic token model that has readable light and
dark values. Dark mode MUST use neutral near-black surfaces and off-white text,
without green-tinted body copy or general surfaces. Borders MUST communicate a
real boundary, focus state, validation state, or semantic status; nested parent
and child surfaces MUST not render duplicate frames when spacing or one divider
is sufficient.

#### Scenario: Surface renders in either theme

- **WHEN** a shared surface is rendered in light or dark mode
- **THEN** its background, text, controls, and status values MUST remain
  readable at WCAG AA contrast
- **AND** the semantic accent MUST remain reserved for actual brand/action or
  status meaning rather than tinting every surface

#### Scenario: Shared framing has little or no supporting copy

- **WHEN** a page heading has no meaningful subtitle or description
- **THEN** the shared primitive MUST omit the redundant supporting block
- **AND** content MUST retain comfortable spacing and a clear hierarchy

### Requirement: Shared section spacing stays consistent and intentional

Shared and page-local section composition MUST use a consistent spacing rhythm.
Excessive gaps MAY be reduced, but useful separation between headings, labels,
controls, content, and state surfaces MUST remain. Spacing changes MUST NOT
remove an action, context, accessibility relationship, or stable anchor.

#### Scenario: A page is corrected for excessive gaps

- **WHEN** a representative page has visibly uneven or excessive vertical space
- **THEN** the correction MUST reduce only the unnecessary gap and retain
  comfortable internal and inter-section spacing
- **AND** the page MUST preserve reading order, useful copy, actions, and
  responsive behavior

### Requirement: SponsorScroller framing and width preserve content purpose

SponsorScroller MUST preserve its sponsor content, links, labels, loading,
empty, error, accessibility, and interaction behavior. An outer visual framing
wrapper MAY be removed only when it contributes no semantic grouping, accessible
name, interaction boundary, state, or required anchor. Non-Home instances MAY
use the available content width, while Home MUST retain its existing width and
composition.

#### Scenario: SponsorScroller renders on Home

- **WHEN** Home renders the sponsor scroller
- **THEN** its existing width, composition, sponsor content, and actions MUST
  remain unchanged
- **AND** any wrapper removal MUST not change its semantics or accessibility

#### Scenario: SponsorScroller renders outside Home

- **WHEN** a non-Home page renders the sponsor scroller
- **THEN** it MAY use a wider page-local content frame when that improves the
  composition
- **AND** sponsor links, labels, state behavior, and keyboard interaction MUST
  remain unchanged

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

