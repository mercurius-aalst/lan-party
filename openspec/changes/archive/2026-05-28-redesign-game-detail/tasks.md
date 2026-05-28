## 1. Restructure the page shell

- [x] 1.1 Refactor `GameDetail.razor` to remove the tabbed `Tournament information` / `Details` shell, including the tab buttons, dropdown selector, and tab-switching layout
- [x] 1.2 Remove redundant summary surfaces from the page shell, including the stat strip, upcoming matches preview card, participant preview card, and sidebar `At a glance` summary
- [x] 1.3 Update `GameDetail.razor.css` so the page layout supports direct standalone sections for overview, schedule, bracket, participants, and results across desktop and mobile breakpoints

## 2. Separate tournament content into standalone sections

- [x] 2.1 Update the overview content so it only presents tournament setup and registration metadata without duplicating participant, bracket, or schedule details
- [x] 2.2 Split the current combined match experience into two standalone sections: one for schedule-oriented match listings and one for bracket visualization
- [x] 2.3 Render participants and results as direct standalone sections in the page flow, reusing existing tab content where practical without preserving the tabbed interaction model

## 3. Simplify hero and partner presentation

- [x] 3.1 Restyle the game detail hero to align with the shared non-homepage hero language while preserving tournament identity, concise summary, and primary actions
- [x] 3.2 Replace multi-context public sponsor rendering with a single featured tournament partner selection and presentation path
- [x] 3.3 Reduce or remove competing sidebar tournament metadata so the overview section remains the primary owner of general tournament details

## 4. Verify behavior and polish

- [x] 4.1 Verify empty and populated states for overview, schedule, bracket, participants, results, and featured partner rendering
- [x] 4.2 Verify responsive behavior after removing tabs, including section flow, CTA visibility, and readability on narrow viewports
- [x] 4.3 Verify that existing admin actions and tournament management interactions still function correctly within the simplified page layout

## 5. Review follow-up refinements

- [x] 5.1 Add a 16-participant double-elimination mock tournament with generated upper bracket, lower bracket, and grand final data
- [x] 5.2 Make double-elimination bracket views selectable with upper bracket, lower bracket, and grand final tabs
- [x] 5.3 Add schedule filters for bracket group and dynamically available rounds
- [x] 5.4 Make the bracket section span the full game detail layout width for larger tournament trees
- [x] 5.5 Compact participant tiles in the sidebar so at least two can fit next to each other where space allows
- [x] 5.6 Render closed registration as a disabled state instead of an inviting primary CTA
- [x] 5.7 Remove the inner spacing around 16:9 game banners while preserving non-cropping image fit
