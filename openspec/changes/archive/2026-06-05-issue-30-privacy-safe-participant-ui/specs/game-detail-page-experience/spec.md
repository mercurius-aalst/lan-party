## ADDED Requirements

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
