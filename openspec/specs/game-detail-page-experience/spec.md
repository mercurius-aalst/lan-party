# game-detail-page-experience Specification

## Purpose
Defines the public tournament detail page experience, including standalone section ownership, schedule and bracket separation, focused tournament controls, simplified partner presentation, and the shared page-hero treatment.

## Requirements

### Requirement: Game detail page presents tournament content as standalone sections

The game detail page SHALL present tournament content as direct standalone sections in the page flow rather than inside a generic tabbed `Tournament information` or `Details` shell.

#### Scenario: Visitor opens a tournament detail page
- **WHEN** a visitor opens a tournament detail page
- **THEN** the page presents standalone sections for tournament overview metadata, match schedule, bracket, participants, and results as applicable
- **AND** the page does not require the visitor to switch tabs or use a section dropdown to reach those content areas

#### Scenario: Visitor views tournament overview metadata
- **WHEN** the tournament overview section is rendered
- **THEN** it shows tournament setup and registration metadata such as status, participation mode, bracket type, schedule, and registration state
- **AND** it does not duplicate participant listings, bracket visualization, or match schedule listings that belong to other sections

### Requirement: Game detail page separates match schedule from bracket progression

The game detail page SHALL present API-backed match scheduling estimates separately from bracket visualization so visitors can inspect planned timing and tournament progression independently.

#### Scenario: Tournament has generated match schedule estimates
- **WHEN** a tournament detail page renders match information
- **THEN** the page shows a dedicated schedule-oriented section using estimated match start and end times from the loaded game detail response
- **AND** the page shows a separate bracket-oriented section for tournament progression and pairings
- **AND** the page does not issue additional per-match API calls solely to obtain schedule estimates

#### Scenario: Tournament lacks estimated match timing
- **WHEN** match estimate data is unavailable for the current tournament state
- **THEN** the schedule section renders an appropriate empty or unavailable state in its own section
- **AND** planned tournament timing remains visible in overview or schedule context when available
- **AND** the bracket section continues to render independently when bracket data is available

### Requirement: Game detail page contains dense tournament sections

The game detail page SHALL keep match schedule and participant sections scannable when many matches or participants are present, using loaded estimated schedule fields for schedule ordering and labels.

#### Scenario: Visitor views a large match schedule
- **WHEN** the match schedule contains enough matches to create a long list
- **THEN** the schedule section keeps its header and filter controls separate from the dense match list
- **AND** the match list remains visually contained within the schedule section
- **AND** the section communicates the current visible match count or filtered result state
- **AND** match schedule values are derived from generated estimates without being labeled as actual lifecycle timestamps

#### Scenario: Visitor views a large participant roster
- **WHEN** the participant section contains enough registered participants to create a long roster
- **THEN** the participant list uses a compact, bounded presentation that preserves the surrounding page overview
- **AND** participant cards do not resize or spill outside the participant section in a way that overlaps adjacent sections
- **AND** selecting a participant still provides access to the participant detail content

### Requirement: Game detail page presents planned and estimated tournament timing clearly

The game detail page SHALL distinguish planned tournament schedule configuration, generated tournament estimates, generated match estimates, and actual lifecycle timestamps.

#### Scenario: Visitor views tournament overview metadata
- **WHEN** the tournament overview section renders schedule metadata
- **THEN** planned tournament start time is labeled as planned or scheduled timing
- **AND** average game duration and round break duration are not shown in public schedule or registration metadata because they are internal estimation inputs
- **AND** estimated tournament end time is labeled as estimated when present

#### Scenario: Tournament has actual lifecycle timestamps
- **WHEN** actual start or completion timestamps are displayed for an in-progress or completed tournament
- **THEN** those values are labeled separately from planned and estimated timing
- **AND** the UI does not overwrite explicit planned or estimated schedule fields with actual lifecycle values

#### Scenario: Schedule fields are partially unavailable
- **WHEN** some planned or estimated schedule fields are missing from the loaded game detail response
- **THEN** unavailable values are omitted or rendered as unavailable
- **AND** the page does not fall back to misleading `StartTime` or `EndTime` values for planned or estimated labels

### Requirement: Game detail page isolates the full-width bracket section

The game detail page SHALL present the bracket as a full-width section that is visually and spatially isolated from participant and schedule content.

#### Scenario: Visitor scrolls from participants to bracket
- **WHEN** the visitor reaches the bracket section after viewing the overview, schedule, and participants
- **THEN** participant content is no longer sticky or positioned in a way that overlaps the bracket area
- **AND** the bracket section starts as its own full-width section in the page flow
- **AND** the bracket remains clearly separated from preceding content through spacing or section boundaries

#### Scenario: Bracket is wider than the available viewport
- **WHEN** a single-elimination or double-elimination bracket requires more horizontal space than the page width
- **THEN** the bracket keeps its full-width section placement
- **AND** horizontal overflow is handled inside the bracket viewport
- **AND** the bracket does not cause lateral page overflow or collide with participant content

#### Scenario: Visitor views the page on a narrow screen
- **WHEN** the game detail page is rendered on a tablet or mobile viewport
- **THEN** overview, schedule, participant, bracket, and result sections reflow into a single readable column
- **AND** dense schedule, participant, and bracket content remains contained within its own section

### Requirement: Game detail page supports focused tournament progression controls

The game detail page SHALL provide focused controls for large tournament schedules and double-elimination bracket views so visitors can inspect the relevant subset without scanning the full tournament at once.

#### Scenario: Visitor views a double-elimination bracket
- **WHEN** a tournament uses a double-elimination bracket with upper bracket, lower bracket, and grand final data
- **THEN** the bracket section provides selectable views for upper bracket, lower bracket, and grand final
- **AND** the active bracket view is visually obvious in the selector
- **AND** the bracket panel does not repeat a redundant title or subtitle already communicated by the active selector

#### Scenario: Visitor filters the match schedule
- **WHEN** a tournament schedule contains matches across multiple bracket groups and rounds
- **THEN** the schedule section provides a bracket filter derived from the available scheduled matches
- **AND** the round filter options update based on the selected bracket
- **AND** the schedule list updates to show only matches matching the selected bracket and round filters

### Requirement: Game detail page removes redundant summary surfaces

The game detail page SHALL remove redundant summary surfaces that repeat tournament information without improving visitor understanding or actionability.

#### Scenario: Visitor scans the page after the hero
- **WHEN** the tournament detail page is rendered
- **THEN** the page does not show a stat banner for values such as loaded matches, decided matches, or rounds between the hero and the main content sections
- **AND** the page does not show an `At a glance` style quick-facts summary that repeats participant, match, or result counts elsewhere on the page

#### Scenario: Visitor navigates between standalone sections
- **WHEN** a visitor reads the overview, schedule, bracket, participants, and results sections
- **THEN** each section owns its own dataset or metadata domain
- **AND** the page does not repeat detailed participant or match preview cards ahead of the primary sections that already own that content

### Requirement: Game detail page highlights at most one visitor-facing tournament partner

The game detail page SHALL present at most one visitor-facing sponsor or partner highlight on the public tournament page from the live singular sponsor placement response.

#### Scenario: Tournament has a sponsor placement
- **WHEN** the tournament detail page receives a non-null sponsor placement
- **THEN** the page displays a single featured tournament partner surface
- **AND** the page does not render multiple separate sponsor context sections for the same tournament page

#### Scenario: Tournament has no featured partner
- **WHEN** the tournament detail page receives no sponsor placement
- **THEN** the featured partner surface is omitted
- **AND** the rest of the tournament page continues to render normally

#### Scenario: Mock mode returns sponsor placement data
- **WHEN** mock backend mode provides tournament sponsor data for the game detail page
- **THEN** the mock game detail response exposes no more than one sponsor placement for visitor-facing rendering
- **AND** the page behavior remains consistent with the live singular sponsor placement contract

### Requirement: Game detail page hero aligns with shared non-homepage hero patterns

The game detail page SHALL use a hero treatment aligned with the site's shared non-homepage hero language instead of a one-off spotlight-style layout.

#### Scenario: Visitor lands at the top of a tournament detail page
- **WHEN** the game detail hero is rendered
- **THEN** it presents the tournament identity, concise supporting summary, and primary call to action in a hero surface consistent with the site's other non-homepage hero treatments
- **AND** the hero does not rely on a bespoke `Tournament spotlight` framing that visually separates it from the rest of the site's page heroes

#### Scenario: Hero renders uploaded tournament artwork
- **WHEN** a tournament has uploaded banner artwork
- **THEN** the hero presents the artwork in a 16:9 media frame without cropping the uploaded image
- **AND** the media frame does not add inner spacing that visually detaches the banner from its container

#### Scenario: Registration is closed
- **WHEN** registration is no longer available for the tournament
- **THEN** the hero registration action is rendered as a disabled state instead of an inviting primary call to action

#### Scenario: Hero includes tournament partner context
- **WHEN** a featured tournament partner is available for the hero
- **THEN** the hero may incorporate that single partner as part of its supported content
- **AND** the partner treatment remains subordinate to the tournament identity and primary visitor actions

### Requirement: Game detail page renders privacy-safe public participants
The game detail page SHALL render participant lists, bracket entries, match detail dialogs, and placements from privacy-safe participant data included in the loaded game detail response.

#### Scenario: Visitor views participant list with username-only data
- **WHEN** a public game detail response includes participants with only IDs, usernames, and display labels
- **THEN** the participant section renders useful participant cards using the returned public labels
- **AND** usernames link to `/users/{username}` when present
- **AND** the section does not render email or missing public profile field placeholders

#### Scenario: Visitor views participant list with public profile fields
- **WHEN** a public game detail response includes first name, last name, Discord ID, Steam ID, or Riot ID fields for participants
- **THEN** the participant section renders those returned public profile fields
- **AND** the section does not call current-user, admin-user, or full team endpoints to enrich missing public profile fields

#### Scenario: Visitor views team participants with public team data
- **WHEN** a public game detail response includes team participants with team names and public member usernames
- **THEN** team names link to `/teams/{teamName}` when present
- **AND** member usernames link to `/users/{username}` when present
- **AND** captain status is shown only when the loaded public response identifies the captain

#### Scenario: Visitor opens match detail dialog
- **WHEN** a visitor opens a match detail dialog from the schedule or bracket
- **THEN** the dialog resolves participant cards from the loaded game response data
- **AND** the dialog does not call admin/current-user endpoints to enrich public participant cards
- **AND** private account fields are not shown for public participants

#### Scenario: Visitor views placements or results
- **WHEN** a public game detail page renders placements or results
- **THEN** each placement renders participant labels from the loaded placement or game response data
- **AND** the results display works when participants do not include full user DTO fields
- **AND** no additional per-participant API calls are introduced for placement display

#### Scenario: Optional public fields are missing
- **WHEN** a participant, member, or placement omits optional username, first name, last name, linked identity, or team-name values
- **THEN** the game detail page omits the corresponding link or optional field cleanly
- **AND** the page continues to render remaining tournament content without broken labels or private-field placeholders
