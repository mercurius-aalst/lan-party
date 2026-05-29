## ADDED Requirements

### Requirement: Header action buttons preserve clear action hierarchy
The site SHALL style supporting header actions such as login, user, and admin controls as lower-emphasis utility actions while preserving the tickets action as the visually dominant header call to action.

#### Scenario: Visitor views signed-out header actions
- **WHEN** a visitor views the header while signed out
- **THEN** the login action is presented as a supporting utility control
- **AND** the tickets action remains more visually prominent than the login action

#### Scenario: Visitor views authenticated header actions
- **WHEN** an authenticated visitor views the header with the user menu trigger and tickets action
- **THEN** the user menu trigger is presented as a supporting utility control
- **AND** the tickets action remains the most visually prominent call to action in the header

### Requirement: Open header menu triggers use stable selected styling
The site SHALL apply explicit selected styling to open admin and user menu triggers without making them appear faded, washed out, or inactive.

#### Scenario: Visitor opens the user menu
- **WHEN** the user dropdown menu is open
- **THEN** the user menu trigger shows a selected treatment that is more prominent than its resting state
- **AND** the trigger does not rely on a faint translucent-only fill to indicate selection
- **AND** the selected state remains visually distinct from the tickets CTA treatment

#### Scenario: Visitor opens the admin menu
- **WHEN** the admin dropdown menu is open
- **THEN** the admin menu trigger uses the same selected-state language as other supporting header actions
- **AND** the open state remains visually distinct from the tickets CTA treatment
