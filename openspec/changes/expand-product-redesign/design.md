## Context

Mercurius LAN is an interactive Blazor Server front end with public discovery,
authenticated player/team workflows, and role-protected organizer tools. The
existing routes and service seams already map the current live and mock backend
contracts, while the landed visual foundation provides shared surfaces,
typography, focus treatment, semantic accent states, and a responsive shell.

The redesign therefore needs a product-level composition pass and a visual
calibration pass, not a new data layer. The stakeholders are players looking
for a fast path from discovery to registration and match context, teams
managing roster ownership and invites, organizers managing event data, and
maintainers who need the live and mock integrations to remain interchangeable.

**Design Read:** Reading this as an event/community product redesign for LAN
attendees, with confident editorial event language, leaning toward the existing
Blazor, Tailwind, and MudBlazor primitives with a calm neutral dual-theme,
sparse boundaries, and purposeful motion.

**Design dials:** `DESIGN_VARIANCE: 7`, `MOTION_INTENSITY: 5`, and
`VISUAL_DENSITY: 4`. The variance supports a more expressive Home composition
while keeping product surfaces predictable; motion is reserved for hierarchy,
feedback, and state changes; density stays breathable for event discovery and
team workflows.

## Goals / Non-Goals

**Goals:**

- Make the primary player journeys discoverable from the shell and predictable
  across Home, tournaments, registration, teams, profiles, and match context.
- Keep Home focused on Hero, Sponsors, Tournaments, Tickets, and Contact in that
  order. Existing compatibility anchors MAY remain available inside the
  relevant section, but the page MUST not add a separate anchor rail or filler
  content.
- Give organizers a clear role-aware workspace for the existing tournament,
  sponsor, lifecycle, registration, and team actions.
- Standardize section framing, entity identity, actions, filters, dialogs, and
  data-state presentation through existing Blazor/CSS/MudBlazor primitives,
  using whitespace and hierarchy before borders and avoiding redundant heading
  plus subtitle/description stacks.
- Give visitors a light/dark theme choice. The initial choice MUST follow the
  system preference when no explicit preference is stored, and an explicit
  choice MUST persist for later visits when client-side persistence is
  available.
- Preserve visible priority/type distinctions, including the accents used by
  "what to bring" items, in both themes through color plus an accessible icon or
  label where color alone would be insufficient.
- Recompose team management completely around its tasks and move user details
  in team management, tournament participant lists, and tournament lineups to
  privacy-safe information popups.
- Keep the existing GUID-based tournament detail route for new and existing
  links; do not introduce tournament name/slug routes or route-resolution
  compatibility logic.
- Preserve existing route URLs, stable anchors, callback/service/API behavior,
  auth and authorization checks, privacy projection rules, and legal copy unless
  a spec explicitly calls for a user-facing IA change that does not alter the
  underlying contract.
- Keep layouts usable at 640, 860, 960, 1100, and 1280 pixel breakpoints, with
  keyboard navigation, visible focus, live status announcements, and reduced
  motion.

**Non-Goals:**

- No backend endpoint, HTTP verb, request body, multipart field, DTO, Refit
  contract, authentication provider, authorization rule, or mock service change.
- No new npm, NuGet, icon, component, or state-management dependency.
- No replacement of the landed visual foundation with a competing component
  system, package, or page-specific styling framework. Adding a user-selectable
  light/dark mode within one semantic token model is in scope.
- No removal of successful actions, privacy-safe fields, loading/empty/error/
  unauthorized/forbidden/not-found/conflict states, or legal/privacy content.
- No broad URL migration or new tournament route format. Existing GUID-based
  tournament detail routes remain the canonical presentation and mutation entry
  points; no name/slug route or slug lookup is in scope.

## Decisions

### 1. Use goal-oriented IA with canonical routes

The shell will present a small player-first set of primary destinations and
group secondary account/team/admin actions by role. Home will contain only the
Hero, Sponsors, Tournaments, Tickets, and Contact sections. Page-level section
links are conditional: they MAY expose a meaningful next action on a dense
surface, but a short page MUST not receive a redundant anchor rail. Existing
URLs, required fragments, search result destinations, and navigation callbacks
remain canonical so links from notifications, profiles, and backend projections
continue to work.

New and existing tournament links MUST use the existing GUID-based detail route.
The route MUST preserve current bookmarks, notifications, and fragments, while
the GUID remains the identifier sent to the backend API. No name/slug route,
slug generation, or route-resolution endpoint is required.

Structural inspiration from thereality.nl is limited to Home and other
marketing-oriented surfaces. The Tournaments overview retains its general
browse layout, search/filter behavior, and result composition; it MUST NOT be
restructured into a thereality-style editorial page.

Alternative considered: introducing a new `/play`, `/manage`, or dashboard route
hierarchy. Rejected because it would duplicate route ownership, increase deep
link migration risk, and provide no backend capability that does not already
exist.

### 2. Compose journeys from page-owned state and existing services

Each route remains responsible for loading its current projection and for
calling the existing service wrapper when a user performs an action. Journey
composition may add local summaries, conditional next-action links, or
progressive disclosure, but it MUST NOT issue speculative enrichment calls or
move mutation authority into a visual component. After every successful
mutation, the existing backend-authoritative refresh and toast/error behavior
remains the source of truth.

Alternative considered: a global client-side journey store. Rejected because it
would duplicate server state, complicate interactive-server lifecycle behavior,
and make mock/live parity harder to reason about.

### 3. Extend existing shared primitives rather than adding a UI library

The implementation will consolidate recurring composition around existing shared
components and semantic classes for page headers, section headings, entity
identity, action groups, badges, filters, cards, dialogs, and data states. New
shared Razor components are allowed only where markup or accessibility behavior
is genuinely reused; simple visual variants remain CSS classes. MudBlazor stays
for the existing stepper/dialog surfaces that already carry registration or
admin behavior. Cards and borders are reserved for real hierarchy or semantic
state. Repeated title-subtitle or title-description combinations MUST collapse
to the smallest useful heading and supporting copy, and nested surfaces MUST
not create duplicate boundaries.

Alternative considered: adopting a design-system package or rebuilding every
page from a new component abstraction. Rejected due to dependency cost,
interactive-server risk, and unnecessary indirection.

### 4. Preserve contract boundaries and authorization at the current seams

The product reorganization will call the existing `Services/` wrappers and
`APIClients/ILANClient` methods exactly as they are. Anonymous surfaces use only
public projections; authenticated registration, profile, and team controls
remain gated by the existing authentication state; admin controls remain gated
by the existing role checks. The UI may relocate a control, but it may not make
an action appear successful before the confirmed response or expose a protected
resource through a public journey.

Alternative considered: combining reads or adding a thin facade API for the new
journeys. Rejected because it would widen the contract and risk drift from the
current live and mock implementations.

### 5. Treat all data states as first-class content

Loading, empty, error, unauthorized, forbidden, not-found, conflict, and
success/confirmation states will use consistent page and inline state framing.
Existing message meaning and backend-provided validation text remain intact.
Interactive controls expose disabled/busy states while a mutation is pending,
and live regions announce important state changes without stealing focus.

Alternative considered: hiding unavailable sections until data exists. Rejected
because it obscures user expectations and makes failure recovery inconsistent.

### 6. Use one semantic visual language with two user-selectable themes

The implementation will reuse the existing shared surfaces, semantic accent and
warning/danger/success meanings, single radius scale, sharp system typography,
restrained boundaries, and reduced-motion policy in both themes. Light mode
will use an off-white/white surface hierarchy with readable dark text. Dark mode
will use neutral near-black surfaces and off-white text, with no green tint in
body text or general surfaces. The existing green accent MAY remain for actual
brand actions or semantic state, but MUST NOT wash the whole page in green.
The theme choice is page-wide, supports system preference as the default, and
persists an explicit user choice without introducing a backend dependency. The
visual system will favor layout hierarchy and information density over
decorative effects. Motion MAY be used for initial hierarchy, hover/active
feedback, and popup or section state transitions. Every animation MUST have a
stated communication purpose, animate only transform/opacity where practical,
and provide a static or reduced-motion equivalent.

Alternative considered: a separate page-specific theme for organizer surfaces.
Rejected because players and organizers still need to recognize the same product,
and duplicated tokens would regress visual consistency. A single semantic token
model with light and dark values preserves that continuity.

### 7. Make responsive behavior composition-aware

Desktop layouts may use asymmetric compositions and bounded dense panels; section
headings MUST stack a focused heading with an optional meaningful description
instead of using a decorative split header. At smaller breakpoints they collapse
into ordered single-column sections with the primary action and state visible
first, with enough spacing between labels, controls, and content to avoid a
compressed appearance. Tables, bracket views, schedule lists, and notification
rows retain contained overflow rather than causing page overflow. The mobile
navigation keeps its current close callbacks and gains accurate
expanded/controlled semantics where markup can support it.

Alternative considered: one universal mobile stack with no density-specific
rules. Rejected because brackets, rosters, and organizer forms have different
interaction and overflow needs.

### 8. Correct the visual foundation at the source

The corrective pass will keep one semantic token source for the active light or
dark product theme. Shared CSS variables and the existing MudBlazor theme
provider will own canvas, surface, text, border, accent, and semantic status
values. Component-scoped CSS will replace any remaining hardcoded theme
declarations or specificity conflicts at the owning component. Broad global
`!important` rules MUST NOT be used to erase danger, warning, success,
sponsor-tier, winner, loser, podium, blocked, or priority/type distinctions.
Borders MUST be sparse and purposeful: form boundaries and focus/error states
remain available, but parent and child surfaces MUST not receive duplicate
frames when whitespace or a single divider communicates the grouping.

### 9. Restore a full-viewport media-led Home hero

Home will use the existing dynamic tournament image resolver and fallback
branches in a responsive asymmetric grid. The hero MUST use a
`min-height: 100dvh` or equivalent header-aware minimum, must not use a
max-height clip or a `100vw` negative-margin full-bleed trap, and must keep the
existing headline, date/location, and both current CTA links visible at the
initial 1440x900, 1280x900, and 390x844 viewports. Object positioning and a
readable scrim will keep artwork labels and essential copy legible. Decorative
scroll cues, glow fields, parallax/drift, and non-essential continuous motion
are removed; reduced-motion behavior leaves content and actions unchanged.

### 10. Keep registration and participant identity close to the user's task

Tournament registration will be an accessible popup launched from a concise
detail-page action. The popup may retain the existing three logical steps for a
team registration, but selecting an eligible team MUST automatically advance to
roster selection. Individual registration will be a single action followed by a
plain-language confirmation, without repeating that the user is an individual.
Unavailable teams and members will use disabled or status styling plus an icon
or label and a concise accessible explanation; internal reason codes MUST not
be displayed as user copy. Any update action will use friendly text or a
meaningful icon with an accessible name, never an internal phrase such as
"refresh registration state".

Team management may change its complete composition while preserving every
existing team, invite, roster, captain, leave, delete, and logo callback. A user
shown in team management, a tournament participant list, or a tournament team
lineup will open a privacy-safe dialog with returned first name, last name,
username, and linked IDs only. These contextual interactions MUST not navigate
to a public user page or issue private enrichment calls.

### 11. Require browser-computed-style evidence

Static inspection and build output alone are insufficient for this correction.
Before marking corrective tasks complete, browser QA MUST inspect computed
background, foreground, border, focus, and control-state styles for the
representative dialog, MudBlazor, filter, tab, bracket, profile, team, sponsor,
loading, empty, error, and status surfaces in both light and dark modes, and
MUST record viewport document width/overflow, hero bounds, popup focus behavior,
and priority/type distinctions. The checks MUST cover public, authenticated,
and administrator routes in mock mode where available. Unavailable Auth0/live
paths may be documented, but they MUST NOT weaken the route and contract
preservation requirements.

## Risks / Trade-offs

- [IA drift] Moving controls can make a valid action harder to discover. → Keep
  route-level acceptance scenarios, preserve labels where contract-sensitive,
  and add local next-action links without removing the existing entry point.
- [State duplication] Journey summaries could diverge from server truth. → Build
  summaries from the loaded response and refresh after mutations; do not add
  optimistic success state for API-coupled actions.
- [CSS isolation precedence] Component-scoped styles may override shared tokens.
  → Prefer existing shared hooks, audit generated style order, and use focused
  semantic selectors rather than broad universal overrides.
- [Dense responsive content] Brackets and admin forms can overflow or lose
  context on mobile. → Give each dense surface its own bounded scroll/stack
  behavior and test at every required breakpoint.
- [Accessibility regressions] Recomposition can break focus order or live-state
  announcements. → Verify keyboard paths, focus visibility, labels, expanded
  state, and status live regions for each moved interaction.
- [Visual baseline overlap] The existing foundation may already be present in
  shared and page-local CSS. → Treat existing tokens/classes as baseline and
  remove only duplicate visual rules that are demonstrably superseded.

## Migration Plan

1. Inventory route ownership, shared components, existing anchors, auth gates,
   service callbacks, the existing GUID-based tournament detail route, and the
   final visual foundation before page changes.
2. Introduce or extend shared content primitives and state styles without moving
   service or DTO responsibilities.
3. Recompose shell/navigation and the five-section Home journey, then tournament
   detail/registration and team/profile journeys, then organizer workflows.
4. Preserve and verify all state branches and successful mutation flows in mock
   mode before live-contract validation.
5. Run theme, popup, route, and action checks, followed by OpenSpec validation,
   `dotnet build`, focused route/action checks, and responsive/accessibility
   smoke checks. Roll back by reverting the composed page changes; no data
   migration or backend rollback is required.

## Open Questions

- Which existing primary navigation labels should remain verbatim when a player
  goal grouping would otherwise suggest a clearer label? Resolve against the
  current navigation and link-contract specs before implementation.
- Which new shared primitives provide enough repeated structure to justify a
  Razor component rather than a class-only composition? Decide during the
  component inventory and keep the smallest useful surface.
- Which mock-mode scenarios can be exercised automatically in the available
  browser environment? Record any unavailable live/Auth0 or browser checks in
  implementation validation rather than weakening requirements.
- Which component-local declarations still win over the shared light/dark tokens?
  Resolve this from browser-computed-style evidence before closing the
  corrective pass.
