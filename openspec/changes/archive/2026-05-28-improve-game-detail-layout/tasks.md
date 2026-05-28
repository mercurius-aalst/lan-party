## 1. Page Layout

- [x] 1.1 Split the game detail layout into a bounded top region for overview, schedule, partner, and participants, followed by an independent full-width bracket section.
- [x] 1.2 Remove or constrain participant sidebar stickiness so participant content cannot remain visible over the bracket section.
- [x] 1.3 Preserve existing anchors for schedule, participants, bracket, and results after the layout restructure.

## 2. Dense Schedule And Participants

- [x] 2.1 Update the schedule section markup/CSS so its header and filter controls remain distinct from the dense match list body.
- [x] 2.2 Add contained, responsive styling for long schedule lists while preserving filter behavior and visible result-state messaging.
- [x] 2.3 Update the participant section styling so large rosters use compact, bounded cards in the sidebar/top region.
- [x] 2.4 Ensure participant cards handle long names without resizing, spilling, or overlapping adjacent sections.
- [x] 2.5 Verify participant detail and add-participant dialogs still open and close correctly after layout changes.

## 3. Bracket Containment

- [x] 3.1 Update single-elimination bracket viewport CSS to size against its parent section instead of the full viewport.
- [x] 3.2 Update double-elimination bracket viewport CSS to size against its parent section instead of the full viewport.
- [x] 3.3 Ensure horizontal bracket overflow is contained inside the bracket viewport and does not create page-level lateral overflow.
- [x] 3.4 Confirm bracket layout height remains part of normal document flow so following sections start below the bracket.

## 4. Verification

- [x] 4.1 Build the Blazor web project to catch Razor/CSS isolation errors.
- [x] 4.2 Verify desktop layout with a tournament that has many matches and participants.
- [x] 4.3 Verify mobile/tablet layout collapses to a single readable column with contained schedule, participant, and bracket sections.
- [x] 4.4 Verify single-elimination, double-elimination upper bracket, lower bracket, and grand final views remain usable.
