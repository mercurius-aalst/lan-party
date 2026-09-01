# user-owned-team-management Specification

## Purpose
TBD - created by archiving change issue-41-user-owned-team-management-ui. Update Purpose after archive.
## Requirements
### Requirement: Authenticated users can view their team management summary
The front-end SHALL provide an authenticated team management area that shows the current user's captain-owned teams, member teams, and received pending team invites from backend-authoritative current-user team data, using branded native Blazor/HTML controls styled to match the existing Mercurius branding.

#### Scenario: Authenticated user opens team management
- **WHEN** an authenticated user opens the team management area
- **THEN** the page loads the user's captain-owned teams
- **AND** the page loads the teams where the user is a member
- **AND** each team MUST be displayed only once even if the backend summary includes it in multiple relationship buckets
- **AND** the page loads pending team invites received by the user
- **AND** the page shows loading, empty, and error states without blocking unrelated navigation
- **AND** interactive controls use branded native Blazor/HTML primitives without appearing visually disconnected from the existing site styling

#### Scenario: Anonymous visitor opens team management
- **WHEN** an anonymous visitor opens the team management area
- **THEN** the page requires authentication before showing team management data
- **AND** the page does not call authenticated team management endpoints as an anonymous user

#### Scenario: Summary request fails
- **WHEN** the team management summary cannot be loaded
- **THEN** the page shows a recoverable error state
- **AND** previously loaded private team management data is not shown as fresh state

### Requirement: Users can create teams
The front-end SHALL let authenticated users create a team through the backend create-team contract and update management state after successful creation, using branded native form and feedback components.

#### Scenario: User creates a team successfully
- **WHEN** an authenticated user submits a valid team name
- **THEN** the front-end sends a create-team request to the authenticated team endpoint
- **AND** the created team appears in the user's captain-owned team list after the action succeeds
- **AND** the create form returns to a clear post-submit state
- **AND** the success confirmation uses the existing application toast framework

#### Scenario: Team creation is rejected by backend rules
- **WHEN** the backend rejects team creation because the captain limit, name validation, authorization, or another backend rule blocks the action
- **THEN** the page shows a clear user-facing error message
- **AND** the entered team name remains available for correction
- **AND** the team is not added optimistically as saved state

### Requirement: Captains can manage team invites
The front-end SHALL let team captains invite users, view pending sent invites, and cancel pending invites for teams they captain, using branded native search, list, menu, dialog, and feedback components.

#### Scenario: Captain invites a user
- **WHEN** a captain selects a user through an approved privacy-safe lookup or identifier flow and submits an invite for a team they captain
- **THEN** the front-end sends the invite request for that team
- **AND** the pending invite appears in the team's sent invite list after the action succeeds

#### Scenario: Duplicate invite is blocked
- **WHEN** the backend rejects an invite because the user is already invited, already a member, or otherwise cannot receive the invite
- **THEN** the page shows the backend-authoritative blocked-action message
- **AND** the sent invite list does not add a duplicate pending invite

#### Scenario: Invite cooldown is active
- **WHEN** the backend rejects an invite because the invited user recently declined and an invite cooldown applies
- **THEN** the page explains that the invite cannot be sent yet
- **AND** the captain can continue managing the team without losing other page state

#### Scenario: Captain cancels a pending invite
- **WHEN** a captain cancels a pending invite for a team they captain
- **THEN** the front-end sends the cancel request for that invite
- **AND** the invite is removed from the team's sent invite list after the action succeeds

### Requirement: Invited users can respond to received invites
The front-end SHALL let authenticated users accept or decline received team invites and keep invite and membership state current after each response.

#### Scenario: User accepts a team invite
- **WHEN** an authenticated user accepts a pending team invite they received
- **THEN** the front-end sends the invite response request for that invite
- **AND** the invite is removed from the received pending invites list
- **AND** the joined team appears in the user's member team list after the action succeeds

#### Scenario: User declines a team invite
- **WHEN** an authenticated user declines a pending team invite they received
- **THEN** the front-end sends the invite response request for that invite
- **AND** the invite is removed from the received pending invites list after the action succeeds
- **AND** the declined team is not added to the member team list

#### Scenario: Invite response fails
- **WHEN** the backend rejects an accept or decline action because the invite is stale, unauthorized, or otherwise invalid
- **THEN** the page shows a clear error message
- **AND** the invite lists are refreshed or left unchanged so the user is not shown a false membership state

### Requirement: Members can leave teams when backend rules allow it
The front-end SHALL let authenticated team members leave teams while treating backend membership and tournament roster rules as authoritative.

#### Scenario: Member leaves a team successfully
- **WHEN** an authenticated member chooses to leave a team and confirms the action
- **THEN** the front-end sends the leave-team request
- **AND** the team is removed from the user's member team list after the action succeeds

#### Scenario: Leave is blocked by tournament roster rules
- **WHEN** the backend rejects a leave action because the user is in an ongoing tournament roster or another backend rule blocks leaving
- **THEN** the page shows a clear blocked-action message
- **AND** the team remains visible in the user's team list

#### Scenario: Captain cannot leave without backend-approved transition
- **WHEN** a captain attempts an action that would leave a captained team without satisfying backend captain rules
- **THEN** the page shows the backend-authoritative blocked-action message
- **AND** the captain-owned team remains manageable from the page

### Requirement: Captains can remove team members when backend rules allow it
The front-end SHALL let team captains remove non-captain team members through the backend member-removal contract and SHALL treat backend membership and tournament roster rules as authoritative.

#### Scenario: Captain removes a member successfully
- **WHEN** a captain confirms removal for a non-captain member of a team they captain
- **THEN** the front-end sends the remove-member request for that team and user
- **AND** the removed member no longer appears in the team's member list after the action succeeds
- **AND** any stale captain-transfer selection for the removed member is cleared

#### Scenario: Member removal is blocked
- **WHEN** the backend rejects member removal because the user is unauthorized, the target is the captain, the target is not a member, tournament roster rules block removal, or another backend rule blocks the action
- **THEN** the page shows the backend-authoritative blocked-action message
- **AND** the team member list remains based on the last confirmed backend state

### Requirement: Captains can transfer captainship
The front-end SHALL let captains transfer captainship to another current team member and refresh management state after the transfer.

#### Scenario: Captain transfers ownership successfully
- **WHEN** a captain selects another member of the team and submits captain transfer
- **THEN** the front-end sends the captain transfer request with the selected member ID
- **AND** the team shows the new captain after the action succeeds
- **AND** the current user's management permissions are refreshed

#### Scenario: Captain transfer is rejected
- **WHEN** the backend rejects captain transfer because the target is not a member, the user is unauthorized, or another backend rule blocks the action
- **THEN** the page shows a clear error message
- **AND** captain labels and available actions remain based on the last confirmed backend state

### Requirement: Captains can manage team logos
The front-end SHALL let captains upload, replace, preview, and remove team logos with backend-authoritative validation and saved-state handling, using branded native upload, button, progress, dialog, and feedback components.

#### Scenario: Captain previews and uploads a logo
- **WHEN** a captain selects a local logo file for a team they captain
- **THEN** the page shows a preview before upload
- **AND** the front-end sends the logo as multipart form data when submitted
- **AND** the saved team logo updates only after the backend accepts the upload
- **AND** team participant cards MUST use the saved logo as the team avatar instead of the team's initial
- **AND** transparent saved logos MUST reveal the parent component background instead of the no-logo fallback avatar color
- **AND** backend-relative saved logo paths MUST resolve against the configured backend base address before rendering

#### Scenario: Logo file is invalid
- **WHEN** a selected or submitted logo violates client-side checks or backend validation
- **THEN** the page shows validation feedback explaining that the logo cannot be used
- **AND** the previously saved logo remains visible as the confirmed team logo

#### Scenario: Captain removes a logo
- **WHEN** a captain removes a team's saved logo
- **THEN** the front-end sends the remove-logo request
- **AND** the team returns to its no-logo visual state after the action succeeds

### Requirement: Captains can remove teams
The front-end SHALL let captains remove teams they captain through a confirmed, backend-authoritative action and SHALL remove deleted teams from visible management state after the action succeeds.

#### Scenario: Captain removes a team successfully
- **WHEN** a captain confirms removal for a team they captain
- **THEN** the front-end sends the remove-team request for that team
- **AND** the team is removed from the user's captained team list after the action succeeds
- **AND** selected team state moves to another available team or to the empty team state

#### Scenario: Team removal is rejected
- **WHEN** the backend rejects team removal because the user is unauthorized, the team is missing, or another backend rule blocks deletion
- **THEN** the page shows the backend-authoritative blocked-action message
- **AND** the team remains visible based on the last confirmed backend state

### Requirement: Team management preserves privacy boundaries
The front-end SHALL keep authenticated team management data separate from public team profile data and SHALL avoid exposing private account fields in invite lookup, team lists, and public navigation surfaces.

#### Scenario: Public team profile remains invite-free
- **WHEN** a visitor opens a public team profile
- **THEN** the public profile does not show pending invites, sent invites, captain management actions, or private management metadata
- **AND** the public profile remains accessible according to the existing public team profile rules

#### Scenario: Captain searches for invite recipients
- **WHEN** a captain searches or selects a user to invite
- **THEN** the UI uses authenticated user search results with invite target IDs for invite selection
- **AND** invite search results render as a dropdown anchored to the search field instead of expanding the form layout
- **AND** the invite search dropdown MUST layer over the dialog surface without being clipped by the modal shell
- **AND** invite search result rows reuse the same result presentation as the global navigation search
- **AND** the UI displays only privacy-safe identifiers needed to choose the user
- **AND** the UI does not render email, Auth0 ID, roles, deletion state, or other private account fields

#### Scenario: Invite search includes existing team members
- **WHEN** a captain searches for invite recipients and the search results include users who are already members of the selected team
- **THEN** those existing team members MUST remain visible in the invite search results
- **AND** those existing team members MUST be disabled for invite selection
- **AND** the UI SHOULD label those disabled results as already being in the team

#### Scenario: Team member card is selected
- **WHEN** a user selects a team member card with a public username in team management
- **THEN** the front-end SHALL navigate to that member's public user profile
- **AND** the navigation target SHALL use the same `/users/{username}` route as other public user profile links

### Requirement: Team management state updates predictably after mutations
The front-end SHALL update or refresh affected team, invite, membership, captain, logo, and navigation invite-count state after successful team management actions.

#### Scenario: Successful mutation refreshes affected state
- **WHEN** a create, invite, cancel invite, accept, decline, leave, remove member, captain transfer, upload logo, remove logo, or remove team action succeeds
- **THEN** the affected visible list, detail panel, and pending invite affordance reflect the confirmed backend state
- **AND** stale actions for the completed mutation are no longer presented

#### Scenario: Mutation fails
- **WHEN** a team management mutation fails
- **THEN** the page shows the failure message
- **AND** visible team management state remains based on confirmed backend data rather than optimistic changes
- **AND** service-layer exceptions MUST preserve the failed team action, HTTP status, and available API response content for diagnostics

### Requirement: Team SignalR notifications update user notifications and team management state
The front-end SHALL connect authenticated users to the backend team SignalR hub and use received team notifications to update the user-level notification surface and visible team management state.

#### Scenario: User receives a team invite notification
- **WHEN** an authenticated user receives a team SignalR notification for a new team invite
- **THEN** the front-end surfaces a clear user-level notification for the team invite
- **AND** the authenticated notification count reflects the new actionable notification
- **AND** the received invites state is refreshed from backend-authoritative invite data

#### Scenario: Team membership notification arrives
- **WHEN** an authenticated user receives a team SignalR notification for an accepted invite, declined invite, canceled invite, member leave, captain transfer, or team update
- **THEN** the front-end refreshes affected team summary, sent invite, received invite, and user notification state as needed
- **AND** the visible UI remains based on confirmed backend data rather than trusting hub payloads as the full source of truth

#### Scenario: Team hub connection is unavailable
- **WHEN** the team SignalR hub connection is disconnected, reconnecting, or unavailable
- **THEN** the team management page and navigation remain usable through REST-backed state
- **AND** the front-end refreshes team management state when the connection recovers or the user opens the management area

### Requirement: Mock mode supports user-owned team workflows
Mock backend mode SHALL provide representative user-owned team management data and actions for local UI development.

#### Scenario: Mock mode loads team management
- **WHEN** the app runs in mock backend mode and an authenticated mock user opens team management
- **THEN** mock data includes captain-owned teams, member teams, received invites, sent invites, captain labels, members, and logo states

#### Scenario: Mock mode performs team management actions
- **WHEN** a mock user creates a team, responds to an invite, cancels an invite, leaves a team, removes a member, transfers captainship, manages a logo, or removes a captained team
- **THEN** mock state updates in a way that exercises the same visible UI states as live API-backed mode
- **AND** representative blocked actions can be surfaced for validation and error-state development

### Requirement: Admin team management is removed
The front-end SHALL remove admin team management UI routes, navigation entries, and workflows so team creation, membership, invites, captain transfer, and logos are managed through user-owned team management flows.

#### Scenario: Admin navigation is rendered
- **WHEN** an authenticated admin opens the admin navigation
- **THEN** admin team management is not shown as an admin destination
- **AND** team management remains available only through the authenticated user-owned team management destination when appropriate

#### Scenario: Removed admin team management route is requested
- **WHEN** a user attempts to navigate to a removed admin team management route
- **THEN** the application does not render the deprecated admin team management workflow
- **AND** the user is routed to an appropriate not-found, replacement, or recovery destination

#### Scenario: Admin attempts team lifecycle actions
- **WHEN** an admin wants to create, edit, delete, invite, remove members, transfer captainship, or manage logos for teams
- **THEN** the front-end provides only the same user-owned team management actions available for the admin's own account
- **AND** the front-end does not provide admin-only controls to modify teams on behalf of other users

### Requirement: Team management controls use branded native components
The front-end SHALL prefer native Blazor/HTML components for interactive team management controls and SHALL style those components to fit the existing Mercurius visual language.

#### Scenario: Team management form controls render
- **WHEN** team creation, invite, captain transfer, leave, logo upload, or confirmation controls are shown
- **THEN** suitable native Blazor/HTML controls are used for inputs, buttons, dialogs, menus, lists, badges, progress, and validation feedback
- **AND** component colors, spacing, borders, typography, and states fit the existing branded UI rather than default library styling

#### Scenario: Team management visual surfaces render
- **WHEN** the team roster hero, members grid, management sections, logo upload field, captain badge, create dialog, invite dialog, or remove-team section renders
- **THEN** card sizing, spacing, badges, upload controls, modal structure, and background colors SHALL remain visually consistent with the existing branded UI
- **AND** the captain badge SHALL NOT overlap participant avatars or initials
- **AND** the remove-team section SHALL NOT use a red faded background treatment

#### Scenario: Global search result rows are reused
- **WHEN** global navigation search or invite-user search results are rendered
- **THEN** both surfaces SHALL use the shared global search result item component for icon, label, supporting text, and type badge presentation
- **AND** invite-user search results SHALL use dropdown behavior consistent with global navigation search

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
