# game-detail-page-experience Specification Delta

## MODIFIED Requirements

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
