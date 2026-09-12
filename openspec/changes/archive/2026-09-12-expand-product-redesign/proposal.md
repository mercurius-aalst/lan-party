## Why

The current Mercurius LAN front end has strong visual foundations, but its product
journeys still make players, teams, and organizers assemble the next step from
page-specific controls. This change builds on the landed event-tech visual
foundation to reorganize the experience around clear player-first discovery,
registration, team, profile, and organizer journeys while keeping every existing
successful API-coupled action and authorization boundary reliable. The visual
language is refined into a calm, neutral dual-theme system that gives visitors
control over light and dark presentation without losing the event identity.

## What Changes

- Reorganize primary navigation and page entry points around player goals:
  discover tournaments, understand an event, register, manage a team, follow
  matches, and find public participants.
- Keep the Home information architecture focused on five useful sections in
  this order: Hero, Sponsors, Tournaments, Tickets, and Contact. Existing
  compatibility anchors MAY remain embedded in those sections, but Home MUST
  not add a redundant anchor rail or filler sections.
- Add reusable product content primitives for page headers, section navigation,
  entity identity, action groups, status messaging, loading/empty/error/not-found
  states, and responsive collections.
- Improve tournament browse and detail composition so schedule, bracket,
  participants, registration, results, and next actions are easier to scan and
  reach without duplicating data or calls.
- Improve authenticated team and profile journeys, including team selection,
  invites, roster ownership, profile completion, public identity, and match
  summary context.
- Recompose team management around the user's tasks while retaining create,
  invite, accept/decline, roster, captain, leave, delete, and logo actions.
- Replace user navigation from team-management members, tournament participants,
  and tournament team lineups with privacy-safe information popups that show
  returned names and linked IDs only.
- Move tournament registration into an accessible popup. Team selection MUST
  advance to roster selection after eligibility is confirmed, and individual
  registration MUST use a concise click-and-friendly-confirmation flow.
- Group organizer/admin workflows around tournament, sponsor, lifecycle,
  registration, and team-management tasks with clear role-aware entry points and
  mutation feedback.
- Define responsive and accessibility behavior for navigation, dense data,
  dialogs, forms, keyboard flows, focus states, and every loading, empty,
  unauthorized, forbidden, not-found, conflict, and error state.
- Preserve login/logout, registration, tournament enrollment/unregister,
  create/manage team, invites, profile updates, sponsor/admin mutations, legal
  and privacy copy, mock/live parity, and all existing backend contracts.
- Provide user-selectable light and dark themes with system preference as the
  default when no explicit choice exists and persistence of the user's choice
  where the current client architecture allows it. The two modes MUST share
  one semantic token model, use neutral off-white text in dark mode, and avoid
  green-tinted body text or surfaces.
- Reduce visual noise by using whitespace and hierarchy before borders, removing
  redundant title-subtitle or title-description stacks, and adding enough
  spacing between labels, controls, and content to avoid compressed layouts.
- Add purposeful motion for hierarchy, storytelling, feedback, and state
  transitions while retaining a complete reduced-motion path. Structural
  inspiration from thereality.nl MAY inform Home and marketing composition only;
  the Tournaments overview MUST retain its general browse layout.
- Keep the existing GUID-based tournament detail route for new and existing
  links. The redesign MUST NOT introduce tournament name/slug routes or route
  resolution logic, and backend calls MUST continue to use the existing
  tournament identifier contract.

## Post-baseline feedback alignment

This active change also includes a preserve-mode correction pass based on the
latest browser review. Existing fonts, brand palette, logo treatment, hero
content and assets, landing information architecture, route and anchor IDs,
service boundaries, and established component details MUST remain unchanged
unless an outcome below explicitly calls for a presentation change. The pass
MUST preserve all form, auth, authorization, API, service, DTO, mock/live, and
successful mutation contracts.

- Previously identified low-value copy MUST be removed, including redundant
  taglines, descriptive filler, and read-only explanatory prose on information
  and match surfaces. Action labels, field labels, legal copy, loading/error
  states, match status, schedule context, and other decision-supporting content
  MUST remain.
- Purposeful motion MAY be added with the existing CSS, JavaScript, Blazor, and
  component stack; no new package or dependency is permitted for this pass.
  Motion MUST communicate hierarchy, feedback, storytelling, or a state
  transition and MUST have a static or `prefers-reduced-motion` equivalent.
- Dark-mode contrast MUST be improved at the owning component selectors or
  semantic tokens to meet WCAG AA for text, controls, focus, and status states.
  Broad global recoloring, brand-palette replacement, and broad `!important`
  overrides MUST NOT be used.
- Home MUST preserve the existing hero content and assets and the five-section
  IA. After the hero, Sponsors, Tournaments, Tickets, and Contact MAY use
  varied composition rather than repeating title-content enumeration, while the
  Tournaments overview MUST retain its general browse layout.
- Tickets and Contact MUST share the same responsive final row, retaining their
  existing actions and anchors and stacking in reading order on narrow screens.
- Team and roster option lists MUST show only a compact visual `Unavailable`
  state with an icon or accessible label when a team or member cannot be used.
  Validation reasons, raw reason codes, and explanatory paragraphs MUST not be
  rendered beside those options.
- User-facing `Check again`, `live update`, `refresh registration state`, and
  equivalent technical availability controls MUST not be rendered. Existing
  backend-authoritative refresh behavior MUST remain intact behind the scenes.
- Read-only match prose that does not change a decision, action, or state MUST
  be removed, while real match actions, status, schedule, links, loading,
  empty, and error states remain available.
- Register from the Tournaments overview MUST open the existing registration
  popup immediately. Register from tournament detail MUST remain a separate
  detail-page action using that existing popup, and all tournament links MUST
  remain canonical GUID routes.

## Latest preserve-mode feedback batch

The next screenshot review extends the same preserve-mode correction pass. The
implementation MUST keep existing fonts, brand palette, useful content, form
fields and order, routes, anchors, APIs, authentication/authorization, and the
Home hero content/assets. Screenshots remain visual evidence only and MUST not
replace contract, accessibility, or behavior checks.

- Team detail match cards MUST have comfortable internal padding and aligned
  text without dropping match context, actions, status, or schedule meaning.
- Private profile email MUST remain readable in dark mode. Inputs MUST retain
  dark, readable surfaces and focus, selected, and autofill states in dark mode
  without changing field order, labels, validation, or submission behavior.
- Info packing items MUST retain distinct hardware, basic, and optional accents
  with dark-mode contrast that meets WCAG AA.
- Section spacing MUST be consistent and intentional. Excessive gaps MAY be
  reduced, but useful breathing room between headings, controls, and content
  MUST remain.
- SponsorScroller MAY lose an outer framing wrapper only when that wrapper adds
  no semantic grouping, accessible name, interaction boundary, state, or
  anchor value. Non-Home SponsorScroller surfaces SHOULD use the available
  content width while Home retains its existing width and composition.
- Representative pages MAY remove a title, subtitle, or description only when
  it adds zero value and merely restates obvious surrounding UI. Labels,
  details, context, navigation, status, accessibility text, decision support,
  useful content, and information architecture MUST be preserved.
- Privacy surfaces in dark mode MUST use dark semantic surfaces rather than
  bright white cards while preserving legal/privacy copy and its hierarchy.

Completion MUST include focused contrast, spacing, responsive, and preservation
checks for these surfaces, plus strict OpenSpec validation and `git diff
--check`.

Completion MUST include static route/API/preservation checks, focused browser
checks where available, and `git diff --check`; screenshots MAY support visual
evidence but MUST NOT replace behavior, accessibility, or contract checks.

## Capabilities

### New Capabilities

- `player-first-journeys`: Goal-oriented entry points and cross-page journey
  handoffs for players, teams, and organizers.
- `content-primitives`: Reusable semantic UI primitives for entities, sections,
  actions, statuses, and data-state presentation.
- `organizer-workflows`: Role-aware organization of tournament, sponsor,
  registration, lifecycle, and administrative tasks.
- `responsive-accessible-states`: Shared responsive, keyboard, focus, dialog,
  and state-rendering rules across the product.

### Modified Capabilities

- `site-navigation`: Reorganize information architecture and entry points while
  retaining route-safe navigation, global search keyboard behavior, temporary
  surface closing, account visibility, and required actions.
- `games-overview-browse-experience`: Make tournament discovery and next actions
  the primary player entry journey while preserving schedule semantics, filters,
  admin creation visibility, and sponsor behavior.
- `game-detail-page-experience`: Improve section reachability and action context
  for tournament overview, schedule, bracket, participants, results, and
  registration without changing public data or route contracts. Registration
  is a popup trigger, not an inline workflow.
- `tournament-registration-ui`: Clarify individual and team registration
  journeys, ownership, confirmation, eligibility, user-friendly update
  feedback, and failure states while preserving backend-authoritative mutations.
- `user-owned-team-management`: Reorganize the authenticated team hub and invite
  flows while retaining ownership rules, roster semantics, logo actions, all
  existing team actions, privacy-safe member popups, and current team API
  contracts.
- `public-participant-profiles`: Improve discoverability and relationship
  navigation among public users, teams, tournaments, and match summaries while
  preserving privacy-safe projections.
- `shared-brand-surfaces`: Extend the existing shared visual foundation into
  reusable semantic product primitives without creating a parallel styling
  system.
- `live-backend-contract-integration`: Require the reorganized UI to remain a
  presentation and orchestration change at the existing Refit/service boundary,
  with mock/live parity and no invented endpoint or DTO contracts.

## Impact

- Affected Blazor routes and components under `src/Mercurius.LAN.Web/Components/`
  across layout, public pages, authenticated player pages, tournament flows,
  team/profile flows, shared components, and admin pages.
- Affected shared styling and component composition, using the existing CSS,
  Tailwind, and MudBlazor stack with no new dependencies.
- Existing `APIClients/`, `Services/`, `DTOs/`, authentication, authorization,
  mock backend, and JavaScript contracts remain integration boundaries and must
  not be changed unless a separately approved compatibility fix is required.
- Legal/privacy copy, privacy-safe public projections, and all successful data
  mutations remain unchanged in meaning and outcome.
- Validation must cover the Blazor build, OpenSpec consistency, route and action
  preservation, mock-mode journeys where available, responsive breakpoints, and
  keyboard/focus behavior.
