## ADDED Requirements

### Requirement: Games overview preserves admin tournament creation access
The games overview page SHALL preserve the admin-only add tournament card whenever an authenticated admin can view the tournament browse page.

#### Scenario: Admin sees add tournament card with matching tournaments
- **WHEN** an authenticated admin opens the games overview page and tournaments match the current filters
- **THEN** the add tournament card is visible in the tournament grid

#### Scenario: Admin sees add tournament card with no matching tournaments
- **WHEN** an authenticated admin opens the games overview page and no tournaments match the current filters or search
- **THEN** the add tournament card remains visible
- **AND** the empty state remains visible

#### Scenario: Non-admin does not see add tournament card
- **WHEN** a visitor without the admin role opens the games overview page
- **THEN** the add tournament card is not visible
