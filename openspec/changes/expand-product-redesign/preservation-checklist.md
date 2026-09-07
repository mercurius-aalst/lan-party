# Front-end Preservation Checklist

This checklist is the implementation baseline for `expand-product-redesign`.
The redesign may change composition and navigation grouping, but it must not
move ownership of API-coupled actions or alter their contracts.

## Routes and stable anchors

| Surface | Canonical route | Required anchors / handoffs |
| --- | --- | --- |
| Home | `/` | `home-content`, `home-sponsors`, `schedule`, `tickets`, `contact`; `schedule` remains embedded in the Tournaments section when compatibility requires it |
| Event information | `/info` | `packing`, `where`, `tickets`, `catering`, `contact-info` |
| Public sponsors | `/sponsors` | `why-partners`, `current-partners`, `/#contact` handoff |
| Tournament browse | `/tournaments` | `tournaments-grid`, `process` |
| Tournament detail | `/tournaments/{guid}` | overview, schedule, participants, `registration`, bracket, results, and admin section anchors; GUID links remain canonical |
| Public user | `/users/{Username}` | `profile-details`, `profile-matches`, canonical username link, and public match-summary tournament links |
| Public team | `/teams/{TeamName}` | `team-roster`, `team-tournaments`, `team-matches`, canonical team/member/tournament links |
| Team hub | `/teams/manage` | `received-invites`, `team-workspace`, `team-{guid}`, `invite-search`, `invite-search-results` |
| Authenticated profile | `/profile` | `profile-details`, inline action wrapper `profile-security`, `profile-danger`, profile field ids, and `deleteConfirm` |
| Profile completion | `/complete-profile` | step form ids and field order |
| Organizer sponsors | `/admin/sponsors` | sponsor search and editor field ids |

## Ownership and behavior

- `APIClients/`, `Services/`, `DTOs/`, authentication/authorization, mock
  routing/data, and JavaScript are contract boundaries and remain unchanged.
- Existing page callbacks remain the owners of login/logout, registration and
  withdrawal, team creation/invites/roster/captain/leave/delete/logo, profile
  updates, sponsor mutations, and tournament lifecycle actions.
- Tournament registration is launched from the detail page as an accessible
  popup. The popup retains the existing backend-authoritative individual and
  three-step team actions; eligible team selection may advance automatically,
  but no mutation contract changes.
- Clicking a user in team management, a tournament participant list, or a
  tournament line-up opens a privacy-safe information popup showing only the
  returned first name, last name, username, and linked IDs. These contextual
  clicks MUST not navigate to a public user route or issue private enrichment
  calls.
- Public identity links use only supplied public identifiers. Missing optional
  identifiers render as text or are omitted; they never trigger private
  enrichment calls.
- Existing loading, empty, error, unauthorized, forbidden, not-found,
  conflict, pending, success, and recovery branches remain visible with their
  current service ownership and backend-authoritative refresh behavior.
- Forms and dialogs retain field order, validation, multipart field names,
  confirmation steps, and keyboard semantics.

## Shell and responsive baseline

- Primary nav labels, search/keyboard behavior, notification and ticket
  actions, account visibility, admin visibility, and mobile close behavior are
  preserved while goal grouping and local links are clarified.
- The shared MudBlazor/theme baseline supports user-selectable light and dark
  modes through one semantic token model. No stored preference follows the
  system preference by default; an explicit preference persists when client
  storage is available. Dark mode uses neutral surfaces and off-white text,
  while the semantic green accent is reserved for actual brand or status
  meaning and MUST not tint general copy or surfaces.
- Responsive review points are 640, 860, 960, 1100, and 1280 pixels. Dense
  content owns its scrolling surface and page-wide lateral overflow is not
  introduced. New local fragment targets use the shared sticky-header
  scroll-margin offset only when a local link clarifies a dense journey; Home
  MUST not show a redundant anchor rail. Reduced motion remains supported.

## Presentation calibration baseline

- Shared framing MUST prefer spacing and typography over borders. Form and
  focus/error boundaries remain available, but nested parent and child surfaces
  MUST not render duplicate frames.
- Page headings MUST omit a subtitle or description when it adds no new meaning;
  a focused heading with optional supporting copy is preferred to stacked filler.
- Priority and type distinctions, including "what to bring" item accents, MUST
  remain visually distinct in both themes and MUST not rely on color alone.
- Motion MAY support hierarchy, feedback, and state transitions, but each
  animation MUST have a reduced-motion or static equivalent.
- New and existing tournament links use the canonical GUID-based detail route.
  Backend calls continue to use that existing identifier; no name/slug route or
  slug lookup is introduced.
- The Tournaments overview retains its general layout and is excluded from any
  thereality.nl-style structural rewrite. Structural inspiration is limited to
  Home and marketing-oriented surfaces.

## Post-baseline feedback preservation

- Existing fonts, brand palette, logo treatment, hero content/assets, Home
  section order, route/anchor IDs, and established component details remain the
  baseline unless an explicit post-baseline outcome changes presentation.
- Low-value taglines, descriptive filler, and read-only match prose MAY be
  removed only when they do not change a decision, action, or state. Actions,
  statuses, schedules, links, loading, empty, and error states remain.
- Motion uses the existing CSS, JavaScript, Blazor, and MudBlazor stack, has a
  hierarchy, feedback, storytelling, or state-transition purpose, and has a
  static or reduced-motion equivalent. No new package is introduced.
- Dark contrast fixes are local to the owning selector or semantic token and
  meet WCAG AA. Broad global recoloring, brand-palette replacement, and broad
  `!important` overrides are prohibited.
- Home composition MAY vary after the hero, but the five-section IA and hero
  content/assets remain. Tickets and Contact share one responsive final row,
  retain their anchors/actions, and stack in reading order on narrow screens.
- Team/member availability options show only a visual `Unavailable` state with
  an icon or accessible label. Validation reasons, raw reason codes, and
  explanatory paragraphs are not rendered.
- `Check again`, `live update`, `refresh registration state`, and equivalent
  technical availability controls are not rendered; backend-authoritative
  refresh behavior remains intact.
- The overview Register action opens the existing popup immediately; detail
  Register remains a separate detail action, and GUID tournament routes remain
  canonical.

## Latest preserve-mode feedback batch

- Existing fonts, brand palette, useful content, form field names/order, routes,
  anchors, APIs, authentication/authorization, and the Home hero content/assets
  remain unchanged.
- Team detail match cards retain match identity, schedule, status, and actions
  while gaining comfortable internal padding and aligned text.
- Private profile email remains authenticated and readable in dark mode. Dark
  input surfaces, focus, selected, and autofill states remain dark and readable
  without changing form fields, order, labels, validation, or submission.
- Info packing items retain visible hardware/basic/optional distinctions and
  WCAG AA dark contrast. Section spacing MAY lose excessive gaps but MUST keep
  useful breathing room around headings, controls, and content.
- SponsorScroller framing MAY be removed only when it adds no semantic,
  accessibility, interaction, state, or anchor value. Non-Home instances MAY
  use a wider content frame; Home width and composition remain unchanged.
- Representative headings MAY lose title/subtitle/description copy only when it
  adds zero value and merely restates obvious surrounding UI. Context, meaning,
  navigation, status, accessibility, decision support, useful details, and IA
  remain.
- Privacy page dark mode uses dark semantic surfaces instead of bright white
  cards while preserving legal/privacy copy and hierarchy.
