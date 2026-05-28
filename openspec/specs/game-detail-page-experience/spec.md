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

The game detail page SHALL present match scheduling information separately from bracket visualization so visitors can inspect timing and tournament progression independently.

#### Scenario: Tournament has generated match data
- **WHEN** a tournament detail page renders match information
- **THEN** the page shows a dedicated schedule-oriented section for match timing or chronological match listings
- **AND** the page shows a separate bracket-oriented section for tournament progression and pairings

#### Scenario: Tournament lacks one of the match views
- **WHEN** schedule data or bracket data is unavailable for the current tournament state
- **THEN** the unavailable view renders an appropriate empty or unavailable state in its own section
- **AND** the other match-related section continues to render independently when its data is available

### Requirement: Game detail page contains dense tournament sections

The game detail page SHALL keep match schedule and participant sections scannable when many matches or participants are present.

#### Scenario: Visitor views a large match schedule
- **WHEN** the match schedule contains enough matches to create a long list
- **THEN** the schedule section keeps its header and filter controls separate from the dense match list
- **AND** the match list remains visually contained within the schedule section
- **AND** the section continues to communicate the current visible match count or filtered result state

#### Scenario: Visitor views a large participant roster
- **WHEN** the participant section contains enough registered participants to create a long roster
- **THEN** the participant list uses a compact, bounded presentation that preserves the surrounding page overview
- **AND** participant cards do not resize or spill outside the participant section in a way that overlaps adjacent sections
- **AND** selecting a participant still provides access to the participant detail content

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

The game detail page SHALL present at most one visitor-facing sponsor or partner highlight on the public tournament page.

#### Scenario: Tournament has sponsor placements
- **WHEN** the tournament detail page renders sponsor or partner context for visitors
- **THEN** the page selects and displays a single featured tournament partner surface
- **AND** the page does not render multiple separate sponsor context sections for the same tournament page

#### Scenario: Tournament has no featured partner
- **WHEN** no eligible sponsor or partner is available for the tournament detail page
- **THEN** the featured partner surface is omitted
- **AND** the rest of the tournament page continues to render normally

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
