## MODIFIED Requirements

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

## ADDED Requirements

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
