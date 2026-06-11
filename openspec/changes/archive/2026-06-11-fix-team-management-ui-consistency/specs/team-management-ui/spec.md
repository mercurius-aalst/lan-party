## ADDED Requirements

### Requirement: Team management action feedback uses shared toasts

The teams/manage page MUST display user-facing action feedback through the application's shared toast display.

#### Scenario: Team management action succeeds
- **WHEN** an authenticated user completes a teams/manage action such as inviting a user, canceling an invite, responding to an invite, leaving a team, deleting a team, transferring captainship, saving a logo, or removing a logo
- **THEN** the page MUST show the result through the shared toast display
- **AND** the page MUST NOT render a separate page-local action message banner for that result

#### Scenario: Team management action is blocked or fails
- **WHEN** a teams/manage action is blocked by validation or fails with backend-authoritative feedback
- **THEN** the page MUST show that feedback through the shared toast display
- **AND** the page MUST preserve the message text supplied by the existing team service error handling

#### Scenario: Team logo selection is invalid
- **WHEN** an authenticated user selects an unsupported team logo file or one larger than the allowed limit
- **THEN** the page MUST show the warning through the shared toast display
- **AND** the page MUST leave the saved team logo state unchanged
