# team-identity-display Specification

## Purpose
TBD - created by archiving change standardize-team-identity-display. Update Purpose after archive.
## Requirements
### Requirement: Team identity surfaces use shared logo display
The front-end SHALL render comparable team identity surfaces through a shared component when the loaded page data includes a team name and optional logo URL.

#### Scenario: Team has a saved logo
- **WHEN** a team identity surface renders a team with a non-empty logo URL
- **THEN** the surface MUST show the saved logo as the team avatar
- **AND** transparent logo pixels MUST reveal the parent surface background instead of a fallback avatar fill
- **AND** the logo MUST include accessible alt text identifying the team

#### Scenario: Team has no saved logo
- **WHEN** a team identity surface renders a team without a logo URL
- **THEN** the surface MUST show an initials fallback derived from the team name
- **AND** the fallback MUST remain visually consistent with the team management page fallback treatment

#### Scenario: Team profile link is available
- **WHEN** a public or tournament team identity surface has a route-safe team name
- **THEN** the shared display MAY render the team name as a link to `/teams/{teamName}`
- **AND** the surface MUST NOT call additional team endpoints to discover missing logo or route data

#### Scenario: Team logo URL is backend-relative
- **WHEN** a team identity surface is built from a team, public team, team summary, invite, game detail, placement, or game mutation response with a backend-relative logo URL
- **THEN** the front-end service boundary MUST resolve that logo URL against the configured backend base address before the shared display renders it
- **AND** comparable team identity surfaces MUST use the same centralized logo URL resolution path

