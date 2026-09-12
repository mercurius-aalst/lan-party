## ADDED Requirements

### Requirement: Optional authenticated navigation enrichment is non-blocking

The site navigation MUST remain usable while current-profile, notification, or realtime enrichment is loading or unavailable and MUST fall back to available identity claims where possible. Optional enrichment MUST run after interactive rendering and MUST not start realtime work or request a render after the navigation component is disposed.

#### Scenario: Navigation enrichment fails
- **WHEN** an authenticated visitor's profile, notification, or realtime enrichment request fails
- **THEN** the shared navigation remains rendered and interactive
- **AND** the user menu retains a non-blocking identity fallback
