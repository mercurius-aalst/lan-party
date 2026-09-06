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

## Corrective visual expansion

This active change also includes a focused visual-quality correction pass based
on browser evidence from the landed foundation. The correction MUST preserve
all route, anchor, form, auth, authorization, API, service, DTO, mock/live, and
successful mutation contracts while fixing composition and styling defects.

- Home MUST render a true full-viewport hero using the existing dynamic
  tournament imagery and fallback branches, with the current date/location and
  both current calls to action visible in the initial desktop and mobile
  viewport. The hero MUST use responsive crop/object positioning, avoid
  horizontal overflow and max-height clipping, contain no animated scroll cue,
  glow, or image drift, and remain safe under reduced motion.
- The visual system MUST provide complete light/dark semantic conversion and WCAG AA
  readable states for custom dialogs, MudBlazor controls and steppers, filters,
  tabs, brackets, match dialogs, public relationship cards, team invite and
  danger surfaces, nested labels/values, loading/empty/error/status states, and
  semantic sponsor, winner, loser, and priority/type distinctions. Component-
  local theme declarations MUST be corrected at their source instead of being
  flattened by broad global overrides. Dark mode MUST use neutral surfaces and
  readable off-white text rather than green-tinted text.
- Registration MUST be rendered as a modal dialog launched from the tournament
  page. Visible copy MUST use plain user language: technical labels such as
  "refresh registration state", raw reason codes, and internal status names
  MUST not be shown to users. Update affordances MAY use an icon, but MUST
  retain an accessible label and a concise result message.
- Completion MUST include browser-computed-style checks at representative
  public, authenticated, and organizer routes, including reachable dialogs and
  the 1440, 1280, and 390 pixel viewport targets, with contrast and overflow
  evidence recorded before the corrective tasks are marked complete.

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
