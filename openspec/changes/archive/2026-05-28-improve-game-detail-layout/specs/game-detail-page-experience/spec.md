## ADDED Requirements

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
