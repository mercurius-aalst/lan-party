## Context

The current game detail page combines several independently useful surfaces, but they overlap heavily in the information they present. `GameDetail.razor` currently renders:

- a custom hero with image, tournament summary, badges, actions, and a tournament partner strip
- a stat banner with registered participants, loaded matches, rounds, and decided matches
- preview cards for upcoming matches and registered participants
- a tab shell labeled `Tournament information` where `OverviewTab`, `MatchesTab`, `ParticipantsTab`, and `PlacementsTab` are nested behind a generic details container
- a sidebar with tournament details, sponsor blocks for multiple contexts, registration copy, and an `At a glance` quick-facts list

That structure causes the same tournament facts to appear in multiple places. Match counts and round counts appear before the actual matches section. Participant counts and previews appear before the participants section. Tournament format metadata appears in both the overview tab and the sidebar. Important content is also hidden inside a generic tabbed details shell instead of being presented as first-class page sections. Match schedule information and bracket structure are also bundled together even though they serve different visitor needs. Sponsor presentation is split between a hero strip and multiple sidebar cards even though the intended visitor-facing model is a single tournament partner highlight.

This is a frontend-only restructuring change centered on `GameDetail.razor`, `GameDetail.razor.css`, and the related game detail tabs. No new APIs or backend schema changes are required to deliver the visitor-facing redesign.

## Goals / Non-Goals

**Goals:**

- Remove the generic tabbed details shell and replace it with clearer standalone page sections
- Give each game detail section a clearer ownership boundary so overview, match schedule, bracket, participants, and results are not competing with a catch-all details container
- Remove low-value summary surfaces that repeat information without improving navigation or understanding
- Align the game detail hero with the site's broader non-homepage hero language instead of maintaining a one-off spotlight treatment
- Reduce tournament sponsor presentation to a single featured partner surface for visitors
- Preserve the existing admin actions, edit flows, and match/participant functionality while simplifying the public layout

**Non-Goals:**

- Change tournament registration rules, match generation, participant registration flows, or placement logic
- Redesign the bracket components inside `MatchesTab`
- Rework the admin sponsor management workflow beyond whatever minimal UI constraints are needed to support a single featured partner
- Introduce backend enforcement for a one-partner rule unless implementation later proves it is necessary
- Change sitewide hero styling contracts outside the game detail page and existing shared hero foundations

## Decisions

### Decision 1: Remove the tabbed details shell entirely and promote each content area to a standalone section

The page should no longer use the generic `Tournament information` / `Details` tab shell at all. Instead, the public game detail layout should render distinct sections directly in the page flow for tournament overview metadata, match schedule, bracket content, participants, and results as needed.

This makes the content model much clearer: visitors should not have to enter a catch-all details container and then switch tabs to reach the page's real content. Each section becomes visible and self-describing on its own, with headings and supporting copy tailored to that content area.

Concretely, the redesign should remove the tab button row, mobile tab dropdown, and tab-content switching behavior from `GameDetail.razor`. Existing tab components can be reused as standalone sections if that remains the simplest implementation path, but they should no longer be presented as tabs inside a shared details shell.

Alternative considered: keep the tab shell and let it remain the primary owner of detailed content.
Why not chosen: the user explicitly wants the whole details section gone, and preserving the tab shell would keep the same generic container and interaction model the redesign is trying to remove.

### Decision 2: Keep standalone sections focused on their own data domain

Once the tab shell is removed, each standalone section should own only its own content:
- the tournament overview section owns core metadata and registration context
- the match schedule section owns upcoming and scheduled match information
- the bracket section owns tournament progression and bracket visualization
- the participants section owns participant lists and participant actions
- the results section owns placements and outcomes

The page should also stop previewing detailed participant and match content above those standalone sections. Upcoming matches cards and registered participant preview cards should be removed so visitors do not see a teaser of information that is fully repeated again a short distance below.

Alternative considered: keep preview cards above the standalone sections.
Why not chosen: they would still duplicate the content now owned by the main sections and would preserve the clutter the redesign is meant to remove.

### Decision 3: Split match schedule content from bracket visualization

The redesign should not treat "matches" as one monolithic section. Visitors often need either the upcoming schedule or the tournament tree, and combining both into one block makes each harder to scan. The page should therefore present match scheduling information separately from bracket structure, even if both continue to draw from the existing match dataset.

This can still reuse current components internally, but the public page structure should distinguish between:
- a schedule-oriented section that lists upcoming or timed matches in chronological order
- a bracket-oriented section that shows elimination progression and pairings

Alternative considered: keep one matches section and visually subdivide it within the same container.
Why not chosen: that would still preserve a mixed-purpose section and would not create the clean information split the user asked for.

### Decision 4: Reduce tournament metadata to one clear overview surface

The tournament overview section should focus on the tournament's core configuration and registration metadata: name, status, participation mode, bracket type, schedule, and registration link state. Supporting format-related metadata should not be split between both an overview section and a separate sidebar details card.

This means the current sidebar `Tournament details` card should either be removed or reduced until it no longer competes with the overview section. The redesign should leave one obvious place where visitors read the tournament's general setup details.

Alternative considered: keep the sidebar details card as a secondary overview summary.
Why not chosen: that would continue splitting tournament metadata across two places and preserve the exact duplication the change is trying to remove.

### Decision 5: Remove dashboard-style stat and quick-fact surfaces instead of replacing them

The stat strip beneath the hero and the sidebar `At a glance` list should be deleted outright. These elements use operational metrics like loaded matches, decided matches, and round counts that are not the visitor's primary task on this page. They also create a dashboard framing that clashes with the rest of the site's public page patterns.

If a count still matters for orientation, it should be communicated inside the owning section header, such as the participant count in `ParticipantsTab`, schedule counts in the schedule section, or bracket context in the bracket section, rather than in a separate cross-page metric banner.

Alternative considered: keep only the registered teams metric and remove the rest.
Why not chosen: even the remaining count is already conveyed in the participants section, so keeping a partial metric strip would still preserve redundant structure.

### Decision 6: Rebuild the hero on the shared non-homepage hero foundation

The current hero should be restyled to feel like the site's other page heroes rather than a bespoke "Tournament spotlight" card with a detached image-and-copy layout. The redesign should reuse the existing non-homepage hero styling approach where practical, while still allowing game-specific elements like hero artwork, registration CTA, and a featured partner callout.

The hero should keep only the information that helps visitors orient and act immediately:
- tournament identity
- concise supporting summary
- status or format badges if they still improve scanability
- primary action(s), such as registration and jumping to bracket content
- one featured tournament partner when available

Alternative considered: keep the existing hero structure and only rename or restyle the eyebrow.
Why not chosen: the issue is not just wording; the current surface reads as a separate custom pattern instead of part of the shared page language the user wants.

### Decision 7: Present at most one visitor-facing tournament partner without changing the raw placement model first

The public page should highlight a single sponsor or partner surface even if the underlying data model still contains multiple sponsor placements or contexts. The initial implementation should derive one featured partner from the existing placements, using a stable precedence order and first-in-order selection, while leaving the broader placement model intact for now.

This keeps the redesign frontend-only and avoids forcing a backend/data migration into what is primarily a page simplification change. If later product direction requires true single-partner enforcement, that can be handled as a follow-up without blocking this redesign.

Alternative considered: immediately enforce a single sponsor placement in admin and API flows.
Why not chosen: it increases scope, couples the redesign to backend behavior changes, and is not required to improve the visitor-facing page.

## Risks / Trade-offs

- [Removing preview cards could make the page feel less content-rich at first glance] -> Keep strong section headers and a more intentional hero so the page still feels complete while reducing clutter
- [Visitors may lose quick visibility into counts they previously saw in the stat strip] -> Surface essential counts inside the owning section headers instead of in a global metric banner
- [Removing tabs could make the page longer to scroll] -> Use clear section hierarchy, anchors or action links only if needed, and tighter section composition so the page feels simpler rather than heavier
- [Splitting schedule from bracket could make the page feel more fragmented] -> Use a clear section order and complementary headings so the two surfaces read as related but distinct tournament views
- [A frontend-only single-partner rule may diverge from admin-configured sponsor data] -> Use deterministic precedence and document that the public page intentionally selects one featured partner from the available placements
- [Reusing the shared hero foundation may expose gaps in current shared hero flexibility] -> Keep the shared foundation as the base and apply page-scoped CSS overrides for game-specific media and action layout rather than creating another bespoke hero system
- [Removing sidebar detail cards can shift too much responsibility onto the overview section] -> Ensure the overview section remains concise and scannable, centered on setup metadata rather than becoming a new dumping ground

## Migration Plan

- Refactor `GameDetail.razor` so the hero and standalone content sections become the primary page surfaces and remove the tab shell, stat strip, preview cards, and `At a glance` card
- Separate the current combined match experience into distinct schedule and bracket sections, reusing existing match data and components where practical
- Update `OverviewTab` or its replacement so it owns tournament setup details without repeating match or participant information
- Preserve `MatchesTab`, `ParticipantsTab`, and `PlacementsTab` content as reusable building blocks where helpful, but render schedule, bracket, participant, and result content as separate standalone sections with local header/copy adjustments as needed
- Replace the current multi-context sponsor rendering on the public page with a single featured partner selection strategy derived from existing sponsor placements
- Restyle the page hero and supporting layout CSS to align with shared non-homepage hero patterns and the simplified page composition
- Verify responsive behavior, section flow without tab navigation, registration CTA visibility, and graceful rendering when no partner is assigned

## Open Questions

- Should the public hero still show all current badges (`ParticipationMode`, `BracketType`, `Format`, `Status`), or should some of those move fully into the overview metadata surface?
- If multiple tournament-partner placements exist in current data, should precedence prefer `TournamentPartner` only, or fall back across other sponsor contexts when no tournament partner is configured?
- Should the schedule section include only upcoming timed matches, or all generated matches in chronological order when bracket data exists?
