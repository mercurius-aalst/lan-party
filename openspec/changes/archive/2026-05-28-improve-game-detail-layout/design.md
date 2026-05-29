## Context

The game detail page currently renders overview and schedule content in the main column, sponsor and participant content in a sticky sidebar, and the tournament bracket as a later full-span grid item. This works for small tournaments, but larger participant lists and match schedules make the page tall and dense, while the sticky sidebar can remain visually present when the visitor reaches the full-width bracket.

The bracket components are intentionally wide, using horizontally scrollable/draggable roots and absolute-positioned match cards. The layout improvement should keep that full-width inspection surface, but make the bracket section an isolated page moment instead of sharing visual space with participant content.

## Goals / Non-Goals

**Goals:**

- Keep the bracket section full width across the game detail page.
- Prevent participant sidebar content from overlapping, sticking into, or visually competing with the bracket section.
- Make large match schedules and participant lists easier to scan without changing the existing game data model.
- Preserve the standalone section model for overview, schedule, bracket, participants, and results.
- Improve responsive behavior so the same content remains contained on desktop, tablet, and mobile widths.

**Non-Goals:**

- Redesign the page hero, registration flow, sponsor assignment flow, or results section.
- Change match generation, bracket seeding, participant registration, APIs, or persistence.
- Replace the existing single- and double-elimination bracket rendering algorithms.
- Add a new dependency for virtualized lists or bracket rendering.

## Decisions

### Separate the top overview grid from the bracket flow

The page should treat overview/schedule/sidebar content as a bounded top layout region and render the bracket as the next independent full-width section in normal document flow. The participant section can still sit beside overview and schedule on large screens, but it should not remain sticky across the full page container once the bracket begins.

Alternative considered: keep the sidebar sticky and increase spacing around the bracket. This leaves the core problem in place because sticky participant content can still follow the visitor into a section that is meant to use the full viewport width.

### Give dense sections their own body containment

The schedule and participant sections should separate their headers/filter controls from their dense bodies. The schedule body should remain filterable and scan-friendly when many matches are present. The participant body should use a compact, bounded presentation in the sidebar so a long roster does not dominate the first screen or stretch the top layout indefinitely.

Alternative considered: move all participants below the bracket. That protects the bracket, but weakens the quick registration overview that is useful near tournament metadata.

### Keep the bracket full width, but contain its internal overflow

The bracket section should span the full content width, while the bracket viewport inside it owns horizontal overflow. Bracket roots and absolute containers should size against their parent section rather than `100vw`, because viewport-based widths can ignore section padding and create lateral spillover. The bracket's calculated layout height must remain part of normal flow so following sections start below it.

Alternative considered: constrain the bracket to the main column. This improves containment but conflicts with the requirement that the bracket view remain full width.

### Prefer responsive reflow over duplicated content

At narrower widths, the top layout should collapse to a single column, and dense section bodies should keep usable spacing without duplicating schedule, bracket, or participant datasets elsewhere on the page.

Alternative considered: add a mobile-only summary or duplicate participant preview. This risks reintroducing redundant content that the current game detail spec already avoids.

## Risks / Trade-offs

- Internal scrolling in dense participant or schedule bodies can hide content from quick page scanning → Mitigate with visible counts, existing filters, and clear section headers that describe the currently visible dataset.
- Removing sidebar stickiness reduces persistent access to participant information → Mitigate by keeping participants near the overview/schedule area and preserving the `tournament-participants` anchor.
- Bracket overflow changes may affect drag/scroll behavior → Mitigate by verifying single-elimination, double-elimination upper bracket, lower bracket, and grand final views after CSS changes.
- Compact participant cards may truncate long names → Mitigate with stable card sizing, wrapping or ellipsis rules, and participant detail dialogs for full information.

## Migration Plan

1. Update the game detail page markup/CSS so the bracket is an independent full-width section after the top overview/sidebar region.
2. Adjust participant and schedule section bodies to handle high item counts with compact, bounded, responsive layouts.
3. Update bracket viewport CSS to use parent-contained sizing and horizontal overflow without lateral spillover.
4. Verify desktop and mobile layouts for tournaments with small and large match/participant counts.

Rollback is limited to reverting the markup/CSS changes because no API or data model changes are expected.

## Open Questions

- What participant count should trigger the compact bounded participant body, or should the compact body always be used in the sidebar?
- Should the schedule list use a fixed maximum height, round grouping, or a visible-item limit with progressive disclosure?
