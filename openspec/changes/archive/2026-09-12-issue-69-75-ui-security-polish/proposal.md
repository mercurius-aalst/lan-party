## Why

Issues #69–#74 identify a single front-end polish pass: practical ticket
information, sponsor discovery, the tournament process explanation, and the
homepage layout all need clearer visual priority while retaining the current
content, links, data sources, and responsive behavior. Issue #75 adds a
production boundary to the existing local mock backend so a development aid
cannot become a production authentication or data path.

The issues are related at the product-surface level but do not require a new
API, DTO, persistence model, or backend change. One OpenSpec change keeps the
visual decisions and the production guard reviewable together on the stacked
branch.

## What Changes

- Improve Info ticket hierarchy with intentional semantic accents that remain
  readable in both themes and at narrow widths (#69).
- Put current sponsors first on the Sponsors page, reduce the intro weight,
  remove the extra `Why sponsors matter` description, and place the sponsorship
  CTA in that section (#70).
- Make the reusable sponsor carousel use compact centered tiles and no movement
  when its items fit; allow movement only when the available set overflows the
  visible capacity, including a deliberate single-sponsor state (#71).
- Give transparent sponsor artwork a theme-independent contrast treatment that
  preserves the supplied logo art in cards and carousel items (#72).
- Render the existing localized descriptions in each Tournaments `How it
  works` step and give the descriptions a clear, restrained hierarchy (#73).
- Narrow the featured tournament presentation and give Contact more horizontal
  space than Join the LAN while preserving the existing Home flow and anchors
  (#74).
- Make mock mode an explicitly opted-in Debug/dev-test capability: normal
  Release/production artifacts exclude mock implementation, routes,
  registrations, and UI; a runtime guard still fails closed if production
  configuration enables the flag, while local mock workflows remain available
  (#75).

## Capabilities

### New Capabilities

- `production-mock-mode-safety`: Prevent mock authentication, data, and UI
  paths from being enabled by a production configuration or client state.

### Modified Capabilities

- `lan-info-page`: Add accessible visual hierarchy to ticket options.
- `public-sponsor-showcase`: Prioritize current sponsors and relocate the
  sponsorship action on the public Sponsors page.
- `home-sponsor-showcase`: Define count-aware sponsor carousel behavior.
- `shared-brand-surfaces`: Define contrast-safe sponsor logo treatment that
  preserves source artwork.
- `games-overview-browse-experience`: Make tournament process descriptions
  visible and scannable.
- `visual-correction`: Define proportionate Home featured and contact layouts.

## Impact

This is a front-end-only change. Expected implementation surfaces are the
Info, Sponsors, Home, Tournaments overview, and shared SponsorScroller Razor
components and their scoped styles/localized strings, plus the web project
build configuration, configuration/DI/startup boundary, and focused contract
tests for mock safety. `APIClients/`, DTOs, live services, backend contracts,
database state, and sponsor data remain unchanged. The existing
`SponsorService`, sponsor tier ordering, links, accessibility labels, Home
anchors, ticket content, and explicitly opted-in local mock fixture semantics
remain part of the contract.

## Acceptance criteria

- Each issue #69–#75 has at least one executable scenario in the change specs.
- Light and dark theme states remain readable; color never carries the only
  meaning for ticket or sponsor distinctions.
- Sponsor links, names, alt text, loading/empty/error behavior, and reduced
  motion behavior remain available after layout changes.
- The Home and Tournaments responsive layouts contain their content without
  page-wide horizontal overflow.
- Normal Release/production artifacts default `IncludeMockBackend` to false and
  omit mock implementation types, mock routes, mock registrations, mock-only
  UI, local fixtures, and local mock configuration; an explicit Debug/dev-test
  opt-in can still exercise existing mock flows.
- A non-mock production artifact fails closed if `MockBackend:Enabled=true` is
  supplied, and client state or URL parameters cannot activate mock behavior.
- Strict OpenSpec validation passes before source implementation begins.

## Worker boundaries

- UI workers own only the page/component markup, scoped styles, and localized
  presentation needed for #69–#74; they must not change API contracts or mock
  service semantics.
- The security worker owns the production environment/configuration guard and
  its automated test/build check for #75; it must preserve development/test
  fixture behavior.
- Validation/review checks the combined diff for issue coverage, theme and
  responsive behavior, no production mock path, and unrelated-file changes.
