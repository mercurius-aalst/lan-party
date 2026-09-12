## Context

The current front end already has shared brand surfaces, localized copy,
theme tokens, and a reusable `SponsorScroller`. The requested work is a set of
small, visible corrections over those existing boundaries. Some requests
touch the same sponsor surfaces, so the design must keep the sponsor page,
carousel, and logo treatment consistent without turning a page polish pass
into a new component or data architecture.

Mock mode currently selects authentication, service registrations, and local
login routes from `MockBackend:Enabled`. That switch is useful for local UI
validation but must be rejected in a production environment before the app can
serve the mock path.

## Goals

- Make each affected surface easier to scan while preserving content purpose,
  links, existing data sources, localization, and accessibility hooks.
- Keep sponsor logos recognizable in both themes without rewriting sponsor
  artwork.
- Keep sparse sponsor sets calm and centered and reserve movement for content
  that cannot fit at the current responsive capacity.
- Add a server-side production boundary for mock mode while keeping explicit
  development/test workflows intact.

## Non-goals

- No backend endpoint, DTO, persistence, sponsor model, or API contract change.
- No new package, animation library, or site-wide design-system rewrite.
- No replacement of the existing sponsor tier model, Home section order,
  ticket data, tournament browse interactions, or mock fixture semantics.
- No removal of useful loading, empty, error, unauthorized, or not-found states.

## Decisions

### 1. Reuse existing page and surface boundaries

Info, Sponsors, Home, and Tournaments keep their current route ownership and
data loading. Page-local layout corrections stay in the existing Razor and
scoped CSS files. Shared sponsor markup and treatment remain owned by
`SponsorScroller` and the established sponsor card surface rather than adding
another abstraction.

### 2. Make sponsor hierarchy content-first

The Sponsors page uses a lighter introductory block, gives the current sponsor
tier content the earliest and strongest position, and keeps the value section
useful through its heading and existing value cards. The redundant value
paragraph is omitted and the existing become-a-sponsor action is placed with
that section. Empty-state and tier-order behavior stay unchanged.

### 3. Let carousel count and responsive capacity choose movement

The carousel exposes a compact static presentation whenever all available
items fit the visible capacity at the active breakpoint. At wide widths this
means up to four bounded tiles rather than stretched full-width slides; one
item remains a compact centered tile. The moving presentation is retained only
for a set that exceeds the capacity. Hover/focus behavior, links, labels,
reduced-motion handling, and non-Home consumers remain intact.

### 4. Protect logo artwork with its containing surface

Theme contrast is solved through the logo frame/backplate and semantic surface
tokens around transparent assets. The implementation MUST NOT apply a blanket
recolor, inversion, or filter that changes supplied sponsor artwork. The same
rule applies to dedicated sponsor cards and carousel items at every supported
breakpoint.

### 5. Use existing localized process copy

The Tournaments process cards render the already-defined localized
`processPickText`, `processRegisterText`, and `processPlayText` values. Styling
uses typography, spacing, and restrained accents to separate heading from
description without adding new backend content or visual noise.

### 6. Tune Home composition locally

The featured tournament lead/list grid gets a more natural measure and less
boxed visual weight. The final Join the LAN/Contact row allocates more width to
Contact at desktop/tablet sizes and stacks in reading order on narrow screens.
Existing section order, anchors, actions, data states, and image behavior are
preserved.

### 7. Exclude mock mode at build time and fail closed at runtime

The web project exposes an explicit MSBuild `IncludeMockBackend` switch. Debug
builds default it to `true` so local UI validation keeps its existing mock
workflow. Release and other non-Debug builds default it to `false`; a
deliberate local development/test build MAY override that value with
`-p:IncludeMockBackend=true`, including when a Release test variant is needed.

With `IncludeMockBackend=false`, the normal production/Release build boundary
MUST exclude the mock implementation source/types, mock authentication route
and persona branches, mock DI/service registrations, mock-only navigation UI,
local mock fixtures, and local mock configuration from the artifact wherever
the existing project build can enforce that boundary. The live composition
remains the only compiled application path. A minimal configuration guard is
retained in the non-mock build so `MockBackend:Enabled=true` fails startup
closed rather than being ignored or activating a partial mock path.

Production client state, query parameters, cookies, local storage, and other
browser-controlled values cannot override this server/build decision. Approved
development/test environments retain the current explicit mock registration,
personas, fixtures, and parity workflows. Focused automated coverage proves
the default Release exclusion, the explicit local opt-in, and runtime
rejection of an unsafe production configuration.

## Verification approach

- Strictly validate this change before implementation.
- Add focused contract/static coverage for carousel count states, process
  descriptions, Home layout hooks, sponsor logo treatment, and production mock
  build/runtime safety without duplicating every CSS declaration in tests.
- Run the web build, the focused contract test project, `git diff --check`, and
  strict OpenSpec validation after implementation. Browser checks are only
  required if the parent workflow has an environment that supports them; any
  unavailable live/Auth0 check is documented. Final CSS geometry review passed
  at 340, 500, and 800 pixels; browser rendering was unavailable because
  uncached Playwright and WSL were denied and no CUA surface was available, so
  no screenshot or browser-rendering success is claimed.
