## ADDED Requirements

### Requirement: Authenticated user menu shows username
The site navigation SHALL identify an authenticated user-menu trigger by the user's Mercurius username when a current profile username is available.

#### Scenario: Current profile has username
- **WHEN** an authenticated visitor has a current user profile with username `sampleplayer`
- **THEN** the header user-menu trigger displays `sampleplayer`
- **AND** the trigger does not display the user's first and last name instead

#### Scenario: Username is unavailable
- **WHEN** the current user profile username cannot be loaded or is empty
- **THEN** the header user-menu trigger falls back to username-like identity claims when present
- **AND** otherwise keeps a non-blocking account label so navigation remains usable
