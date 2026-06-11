## Why

Users currently need admin intervention for team ownership and membership workflows even though the backend is adding authenticated user-owned team endpoints. The redesigned front-end needs a self-service team management experience that lets users create teams, manage invites, respond to received invites, transfer captainship, and maintain team logos while keeping backend validation authoritative.

## What Changes

- Add an authenticated user-facing team management area for teams the current user captains, teams they belong to, and pending invites they received.
- Add team creation, team detail management, member leave actions, received invite accept/decline actions, captain invite/cancel invite actions, captain transfer, and team logo upload/replace/remove flows.
- Align the front-end API contract with backend authenticated team routes such as `/lan/teams/me/summary`, `/lan/teams/me/invites`, `/lan/teams/me/sent-invites`, `/lan/teams/{id}/leave`, `/lan/teams/{id}/invites/{userId}`, `/lan/teams/{id}/captain`, and `/lan/teams/{id}/logo`.
- Surface backend-authoritative blocked action feedback for captain limits, duplicate or cooldown-limited invites, ongoing tournament roster restrictions, invalid logo files, and other validation failures without relying on client-only checks.
- Preserve mock backend behavior for local UI development with representative team ownership, invite, membership, captain transfer, and logo states.
- Add a clear authenticated navigation path for team management.
- Add a user-level notification system, such as a bell icon with an unread count, that surfaces team invites and team-related updates.
- Remove admin team management UI entry points and workflows so team ownership and membership management is handled through user-owned self-service flows instead of admin screens.
- Connect authenticated team notifications to the backend team SignalR hub so received invites and team membership changes can feed the user-level notification system and update the UI without requiring a full page reload.
- Use branded native Blazor/HTML controls for forms, dialogs, menus, badges, upload controls, feedback states, and responsive interaction patterns, styled to fit the existing Mercurius branding and Tailwind/component-scoped visual language.

## Capabilities

### New Capabilities
- `user-owned-team-management`: Authenticated users can create and manage their own teams, invitations, membership actions, captain transfer, and team logos through backend-authoritative self-service flows.

### Modified Capabilities
- `site-navigation`: Authenticated navigation exposes a discoverable team management destination and user-level notification bell/count without disrupting existing header, user, admin, search, or ticket controls.

## Impact

- Affects front-end team DTOs, API client contracts, and team services under `src/Mercurius.LAN.Web/APIClients`, `DTOs`, `Models`, and `Services`.
- Affects authenticated page-level UI under `src/Mercurius.LAN.Web/Components/Pages` and reusable components for team cards, invite lists, member controls, logo upload, and validation feedback.
- Affects native Blazor/HTML UI composition and component-scoped styling for notification, team management, form, dialog, list, badge, and upload interactions.
- Removes existing admin team management pages, routes, navigation items, service methods, and mock/admin-only affordances that are no longer part of the redesigned team workflow.
- Affects header/user navigation components so the team management area and user-level notification bell/count are easy to find.
- Affects notification/real-time client wiring for the backend team SignalR hub, including authenticated connection setup, event handling, reconnect behavior, and state refresh after team events.
- Affects mock backend store and `MockData.Local/backend.json` so mock mode exercises the same self-service workflows.
- Requires backend contract verification against `mercurius-aalst/mercurius-aalst-back-end`, especially authenticated team endpoints, team SignalR hub route/events, response shapes, validation/error payloads, logo upload constraints, and privacy-safe user search/invite data.
