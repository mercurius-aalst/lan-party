## ADDED Requirements

### Requirement: Tournament browsing recovers from unavailable data

The games overview MUST remain navigable when its tournament request fails and MUST distinguish the unavailable state from loading and empty states.

#### Scenario: Tournament collection is unavailable
- **WHEN** the tournament collection request fails because the API is unavailable
- **THEN** the page shows a friendly recoverable unavailable state
- **AND** independently loaded sponsor content and site navigation remain usable
