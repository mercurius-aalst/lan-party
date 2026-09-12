## MODIFIED Requirements

### Requirement: Authenticated users can view their team management summary

The front end SHALL provide an authenticated, goal-oriented team hub that shows
the current user's captain-owned teams, member teams, and received pending team
invites from backend-authoritative current-user team data, using branded native
Blazor/HTML controls styled to match the existing Mercurius branding.

#### Scenario: Authenticated user opens team management

- **WHEN** an authenticated user opens the team management area
- **THEN** the page MUST make the next available team task clear before dense
  secondary detail
- **AND** the page MUST load the user's captain-owned teams
- **AND** the page MUST load the teams where the user is a member
- **AND** each team MUST be displayed only once even if the backend summary
  includes it in multiple relationship buckets
- **AND** the page MUST load pending team invites received by the user
- **AND** the page MUST show loading, empty, and error states without blocking
  unrelated navigation
- **AND** interactive controls MUST use branded native Blazor/HTML primitives
  without appearing visually disconnected from the existing site styling

#### Scenario: Anonymous visitor opens team management

- **WHEN** an anonymous visitor opens the team management area
- **THEN** the page MUST require authentication before showing team management
  data
- **AND** the page MUST NOT call authenticated team management endpoints as an
  anonymous user

#### Scenario: Summary request fails

- **WHEN** the team management summary cannot be loaded
- **THEN** the page MUST show a recoverable error state
- **AND** previously loaded private team management data MUST NOT be shown as
  fresh state

#### Scenario: User selects a team task

- **WHEN** a user selects create, invite, roster, captain, leave, logo, or
  received-invite work from the team hub
- **THEN** the application MUST retain the current ownership rules and service
  callback for that task
- **AND** a successful mutation MUST refresh the confirmed team summary before
  the hub reports the new state

## ADDED Requirements

### Requirement: Team hub separates ownership and membership context

The team hub SHALL distinguish captain-owned actions, member actions, and
received/sent invite state without duplicating a team or exposing another user's
private account data.

#### Scenario: Member views a team they do not captain

- **WHEN** a member opens a team they do not captain
- **THEN** captain-only actions MUST remain hidden or disabled according to the
  current authorization rules
- **AND** member actions and ownership explanation MUST remain visible

### Requirement: Team management is recomposed around tasks without losing actions

The team-management surface MUST be fully recomposable into a task-oriented
layout while retaining create-team, invite, accept, decline, roster, captain,
leave, delete, and logo actions with their existing authorization and service
callbacks.

#### Scenario: User works from the recomposed team hub

- **WHEN** an authenticated user opens team management
- **THEN** captain work, member work, and received or sent invite work MUST be
  visually distinct and easy to reach
- **AND** every action available in the current flow MUST remain reachable
- **AND** successful mutations MUST still refresh the backend-authoritative
  summary before the new state is shown

### Requirement: Team-management user selection opens a privacy-safe dialog

Selecting a user in team management MUST open a labelled information dialog
rather than navigating to that user's public profile.

#### Scenario: User opens a team member's details

- **WHEN** an authenticated user selects a member or captain identity in the
  team-management surface
- **THEN** the dialog MUST show only returned first name, last name, username,
  and available public linked IDs
- **AND** missing linked IDs MUST be omitted without placeholders
- **AND** email, Auth0 identifiers, roles, timestamps, and other private fields
  MUST not be shown
- **AND** the interaction MUST not issue a private enrichment request or
  navigate away from team management
- **AND** focus, Escape, close, and focus restoration MUST remain keyboard usable
