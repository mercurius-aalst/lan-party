## ADDED Requirements

### Requirement: Home hero is a full-viewport, readable event entry point

The Home route MUST present the existing dynamic tournament imagery or its
existing fallback as a full-viewport event hero with the current headline,
date/location context, and both current call-to-action links available in the
initial desktop and mobile viewport.

#### Scenario: Desktop visitor opens Home

- **WHEN** a visitor opens Home at a 1440x900 or 1280x900 viewport
- **THEN** the hero MUST occupy the initial viewport using a header-aware
  `min-height: 100dvh` or equivalent minimum
- **AND** the headline MUST fit the intended two-line composition, the image
  MUST share the first hero grid row with the content, and date/location plus
  both existing CTAs MUST be visible without scrolling
- **AND** the page MUST not create horizontal overflow or crop the hero with a
  max-height constraint

#### Scenario: Mobile visitor opens Home

- **WHEN** a visitor opens Home at a 390x844 viewport
- **THEN** the hero MUST reflow into a readable single-column composition
- **AND** the existing image resolver/fallback, date/location, headline, and
  both CTA destinations MUST remain available in the initial hero experience
- **AND** object positioning and scrim treatment MUST preserve essential image
  content and readable copy

#### Scenario: Visitor prefers reduced motion

- **WHEN** the browser reports `prefers-reduced-motion: reduce`
- **THEN** the hero MUST not use animated scroll cues, glow loops, parallax, or
  image drift
- **AND** all hero content and actions MUST remain available

### Requirement: Product surfaces use complete light and dark semantic states

The visual system MUST style custom dialogs, MudBlazor controls and steppers,
filters, tabs, brackets, match dialogs, public relationship cards, team invite
and danger surfaces, nested labels/values, loading/empty/error/status states,
and sponsor/winner/loser/priority states from shared light and dark semantic
tokens. Component-local theme declarations MUST NOT override those tokens in
the rendered state.

#### Scenario: Visitor reaches a corrected interactive surface

- **WHEN** a visitor or authenticated user opens a dialog, filter, tab,
  bracket, match view, profile relationship card, team invite, or form state
- **THEN** the surface MUST render the active theme's neutral canvas/surface
  hierarchy with readable labels, values, placeholders, controls, focus
  indicators, close actions, and status feedback at WCAG AA contrast
- **AND** pending, success, error, blocked, warning, danger, winner, loser,
  podium, sponsor-tier, and priority/type meanings MUST remain visually distinct
- **AND** dark mode MUST use neutral surfaces and off-white text without a
  green tint in general copy or surfaces

#### Scenario: MudBlazor surface is rendered

- **WHEN** a MudBlazor input, select, filter, tab, stepper, dialog, or status
  control is rendered in the product shell
- **THEN** its computed background, text, border, focus, and disabled states
  MUST use the active explicit light or dark theme and accessible
  accent/semantic values
- **AND** no default light, green-tinted dark, or purple theme value MAY be
  visible in the wrong state

### Requirement: Home keeps a useful five-section landing flow

The Home route MUST contain only Hero, Sponsors, Tournaments, Tickets, and
Contact sections in that order. Existing compatibility anchors MAY remain
embedded in the relevant section, but the page MUST not add filler sections or
a redundant anchor rail when the content already fits the flow.

#### Scenario: Visitor opens Home

- **WHEN** a visitor opens Home
- **THEN** the five useful sections MUST be discoverable in the required order
- **AND** redundant title/description blocks and section jump controls MUST be
  omitted when they add no information

### Requirement: Visual hierarchy uses spacing, restrained borders, and purposeful motion

Shared and page-local composition MUST use spacing and typography before borders,
avoid duplicate parent/child frames, and provide enough separation between
labels, controls, and content. Motion MAY support hierarchy, feedback, or state
transitions, but it MUST not be the only state cue and MUST honor reduced-motion
preferences.

#### Scenario: Priority or type content is rendered

- **WHEN** a priority/type item such as a "what to bring" item is rendered
- **THEN** its distinct accent or status MUST remain visible in both themes
- **AND** an icon, label, or accessible text MUST supplement color when color
  alone would not convey the distinction

### Requirement: Browser-computed-style evidence gates completion

The corrective implementation MUST include browser-computed-style evidence
before its tasks are marked complete.

#### Scenario: Representative mock routes are checked

- **WHEN** browser QA exercises representative public, authenticated, and
  organizer routes in mock mode at 1440x900, 1280x900, and 390x844 widths in
  both light and dark modes
- **THEN** it MUST inspect computed foreground/background/border/focus styles,
  hero bounds, document width, reachable dialog states, and priority/type
  distinctions
- **AND** it MUST record contrast and overflow results for the corrected
  surfaces before completion

#### Scenario: A live or Auth0 path is unavailable

- **WHEN** live backend or Auth0 access cannot be exercised in the environment
- **THEN** the implementation MUST document that limitation
- **AND** it MUST still validate static contracts, mock parity, public routes,
  and every reachable local state without weakening the API/auth boundaries
