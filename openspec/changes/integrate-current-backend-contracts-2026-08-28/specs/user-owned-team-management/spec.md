## ADDED Requirements

### Requirement: Team management uses the current backend action routes and bodies
The authenticated team management service SHALL use the current versioned team resource contracts,
including their HTTP verbs, route parameters, request bodies, and multipart field names.

#### Scenario: User leaves a team
- **WHEN** an authenticated member confirms leaving a team
- **THEN** the front-end sends `DELETE /v1/lan/teams/{teamId}/members/me`
- **AND** it refreshes the confirmed team summary after success

#### Scenario: Captain sends or cancels an invite
- **WHEN** a captain sends an invite or cancels a pending sent invite
- **THEN** the send request uses `POST /v1/lan/teams/{teamId}/invites` with a `{UserId}` body
- **AND** cancellation uses `DELETE /v1/lan/teams/{teamId}/invites/{inviteId}`
- **AND** the UI does not use a user-id path segment or add a duplicate optimistic invite

#### Scenario: Invited user responds
- **WHEN** an invited user accepts or declines a pending invite
- **THEN** the response uses `PATCH /v1/lan/team-invites/{inviteId}` with an `{Accept}` body
- **AND** received invites and membership summary are refreshed from the confirmed response

#### Scenario: Captain manages a logo
- **WHEN** a captain uploads, replaces, or removes a team logo
- **THEN** upload uses `PUT /v1/lan/teams/{teamId}/logo` with the `logo` multipart field
- **AND** removal uses `DELETE /v1/lan/teams/{teamId}/logo`
- **AND** the saved logo URL is rendered only after the backend accepts the mutation

### Requirement: Team management maps privacy-safe current response shapes
The team management UI SHALL map current backend summaries and public team response fields without
exposing private account data or depending on legacy invite and team DTO names.

#### Scenario: Management summary is loaded
- **WHEN** the current-user team summary is loaded
- **THEN** the service maps captain-owned teams, member teams, members, and public identifiers from
  the backend response
- **AND** the UI continues to distinguish loading, empty, unauthorized, and error states
- **AND** invite rows expose only the returned team, user, status, creation, and expiry values
