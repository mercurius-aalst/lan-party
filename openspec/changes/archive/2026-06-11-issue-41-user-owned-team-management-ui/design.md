## Context

The front-end currently exposes team API methods for public listing/detail, create/update/delete, and older invite endpoints that take explicit user IDs in paths. The backend companion repository now exposes authenticated user-owned team routes under `v1/lan/teams`, including current-user summary and invite endpoints, team leave, invite/cancel, invite response by invite ID, captain transfer, and multipart logo management. The backend also provides a team SignalR hub for real-time team notifications. These endpoints and hub connections derive the acting user from Auth0 claims, so the front-end should not model user-owned actions as admin actions or require the current user's ID to authorize them.

The redesigned Blazor front-end already has authenticated profile/menu flows, public team profiles, Refit API contracts, service wrappers, mock backend mode, Tailwind styling, component-scoped CSS, and branded native component patterns in use. The team management UI needs to preserve those patterns while adding a larger authenticated workflow with loading, empty, error, unauthorized, and validation states.

## Goals / Non-Goals

**Goals:**

- Provide a self-service `/teams/manage` style area where authenticated users can see teams they captain, teams they belong to, and received pending invites.
- Let users create teams and then manage the resulting captain-owned team state without admin intervention.
- Let captains invite users, view and cancel pending invites, transfer captainship to a member, and upload, replace, or remove a logo.
- Let invited users accept or decline invites, and let members leave teams when backend rules allow it.
- Keep the front-end API contract aligned with the backend's authenticated current-user team routes and response DTOs.
- Surface backend validation and authorization failures clearly while preserving local form context after failed actions.
- Keep public team profile and public participant privacy behavior separate from authenticated team management data.
- Provide a user-level notification surface, such as a bell icon with unread count and notification list, for team invites and team-related updates.
- Connect to the authenticated team SignalR hub so pending invites, invite responses, captain transfer, membership, and logo/team update notifications can populate notifications and refresh relevant UI state.
- Keep mock backend mode useful with representative ownership, invite, transfer, leave, and logo states.
- Remove admin team management screens and affordances entirely so team lifecycle actions are not split across admin and self-service surfaces.
- Use branded native Blazor/HTML controls while styling them to match the existing Mercurius branding, layout density, typography, and component-scoped/Tailwind visual language.

**Non-Goals:**

- Preserving admin team management or moderation workflows.
- Making the front-end the source of truth for membership, captain limits, invite cooldowns, tournament roster restrictions, or logo validation.
- Adding public exposure of pending invites, private account fields, or captain management metadata.
- Implementing real-time push notifications for invites unless an existing notification infrastructure already supports them.
- Adding new packages for upload, modal, state, or validation behavior unless existing Blazor, Refit, Tailwind, and component patterns cannot reasonably handle the flow.

## Decisions

### 1. Use authenticated current-user team endpoints for management state

The management page should load state from current-user endpoints such as `/lan/teams/me/summary`, `/lan/teams/me/invites`, and `/lan/teams/me/sent-invites`, then perform actions through authenticated team endpoints. This avoids passing the current user's ID through the UI and matches the backend's Auth0-claim authorization model.

Alternative considered: reuse public team list/detail endpoints and stitch in current-user profile data. Rejected because public endpoints intentionally exclude invites and private management state, and enrichment would risk privacy leaks and stale authorization assumptions.

### 2. Keep service wrappers as the feature boundary

`ILANClient` should define explicit Refit methods for the backend routes, while `TeamService`/`ITeamService` should expose feature-facing methods with typed DTOs and clear error handling. Page components should not manually construct route URLs or parse raw API exceptions beyond displaying service-provided failure messages.

Alternative considered: call `ILANClient` directly from pages. Rejected because existing front-end patterns use services to preserve API/mock symmetry and keep page code focused on UI state.

### 3. Treat backend failures as authoritative UI feedback

The UI may perform convenience validation such as required team names and local logo preview checks, but it must render backend responses for captain limits, duplicate invites, invite cooldowns, tournament roster restrictions, invalid logo files, authorization failures, and stale state. After successful mutations, the page should refresh or reconcile affected summary/invite data so lists do not show stale actions.

Alternative considered: prevent most blocked actions only through client-side checks. Rejected because the backend owns membership rules and tournament roster state, and client-only checks can become stale.

### 4. Separate received invites from captain sent invites

Received invites should be visible in the user-owned management page and surfaced through a user-level notification surface. Captain sent invites should appear inside captain/team management context so users can cancel invites for a specific team without mixing them with invites they personally need to answer.

Alternative considered: one flat invite list for all invite directions. Rejected because accept/decline and cancel are different permissions and would make error states harder to understand.

### 5. Use multipart upload with local preview and rollback-safe state

Logo upload should use multipart form data through the API client, show a local preview before submit, validate obvious file issues for fast feedback, and still defer final validation to the backend. If upload, replace, or remove fails, the UI should keep the current saved logo visible and show the backend error rather than pretending the optimistic logo is saved.

Alternative considered: immediately swap the saved logo to a local object URL before the backend responds. Rejected because failed uploads would create misleading team identity state.

### 6. Preserve mock mode with the same workflow vocabulary

Mock backend data should include owned teams, member teams, received invites, sent invites, captain identity, and logo URLs or absence. Mock services should mimic successful and blocked actions enough for local development, including duplicate invite and invalid leave/logo-style failures where practical.

Alternative considered: mock only happy-path creation. Rejected because most of this feature's user experience quality is in state transitions and blocked-action messaging.

### 7. Treat SignalR events as invalidation signals, not as the full source of truth

The front-end should connect authenticated users to the backend team SignalR hub and translate received team events into user-level notifications plus targeted refreshes of team summary, invite lists, and notification count state. Hub payloads should not become the only source of membership or invite truth; REST summary/invite endpoints remain the authoritative refresh path after real-time events and local mutations.

Alternative considered: fully update all team state from hub payloads. Rejected because hub payload shape can be narrower than the REST DTOs, and REST refreshes reduce stale permission risks after captain transfer, invite expiry, or roster-rule changes.

### 8. Introduce a reusable user notification surface before team-specific notification copy spreads

The header should expose a user notification control, preferably a bell icon with an unread count, that can show team invite and team update notifications now while leaving room for other user-level notification sources later. Team notifications should carry enough type/action metadata to route users to the relevant team management section or invite action.

Alternative considered: add only a "pending invites" badge in the team menu. Rejected because the desired UX is a user-level notification system, and a dedicated bell/count scales better for future user notifications without hiding team events inside one feature page.

### 9. Remove admin team management instead of maintaining parallel workflows

Existing admin team management pages, routes, navigation entries, and admin-only service affordances should be removed or made unreachable as part of this change. Admins may still see public team profiles and user-owned team management for their own account, but they should not retain a separate UI for creating, editing, deleting, inviting, or modifying teams on behalf of users.

Alternative considered: leave admin team management in place while adding user-owned management. Rejected because it creates parallel sources for ownership and membership changes, conflicts with the backend's user-owned team model, and keeps deprecated admin behavior visible.

### 10. Prefer branded native primitives for interaction-heavy controls

Team management should use native Blazor/HTML controls and reusable project components for dialogs, menus, icon buttons, badges, tabs/segmented sections, lists, form fields, validation messages, progress indicators, alerts, file upload affordances, and confirmation flows. Component-scoped CSS and Tailwind utility classes should adapt spacing, color, borders, and typography so the result reads as Mercurius UI rather than default library UI.

Alternative considered: keep the earlier library-based controls. Rejected because the team management surface looked visually disconnected from the redesigned Mercurius UI, and the current implementation now favors native controls plus reusable project components.

## Risks / Trade-offs

- Backend response DTO names or fields may change while the backend issue is still active -> verify endpoint shapes against `mercurius-aalst-back-end` immediately before implementation and keep DTO names explicit.
- Error payloads may be unstructured strings or validation problem details -> normalize display at the service boundary and preserve raw meaningful messages when field mapping is unavailable.
- Invite user selection can leak private data if it uses admin/full-user endpoints -> use privacy-safe search or a backend-approved invite lookup surface and display only public identifiers needed to choose a user.
- Logo upload size/type limits may differ between client assumptions and backend rules -> keep client checks conservative and always render backend validation as final authority.
- Generic library default styling can clash with the existing brand -> use native controls, reusable project components, component-scoped CSS, theme tokens, and Tailwind-compatible layout wrappers so controls fit the current visual system.
- Notification counts can become stale after invite actions -> refresh current-user team summary or notification count after mutations and on authenticated navigation initialization.
- SignalR connections can disconnect or miss events -> show graceful reconnect behavior and refresh current-user team state after reconnect or when the user opens the management page.
- Captain transfer may remove the current user's ability to manage the team immediately -> refresh management state after transfer and move the team from captain-owned to member-only context when applicable.

## Migration Plan

1. Add OpenSpec requirements for authenticated team management, invite workflows, captain actions, logo management, error handling, privacy boundaries, mock mode, and navigation discoverability.
2. Add or update team management DTOs and Refit methods for the backend authenticated team routes.
3. Extend `TeamService` and mock team services/stores to expose summary, invite, leave, invite response, captain transfer, and logo actions.
4. Build authenticated team management pages/components with responsive desktop/mobile layouts and clear loading, empty, error, unauthorized, success, and blocked-action states.
5. Add navigation entry and user notification bell/count, refreshing notification and invite state after relevant mutations and team SignalR hub events.
6. Add authenticated team SignalR hub client wiring with event handlers, reconnect handling, disposal, and state refresh integration.
7. Verify mock mode and API-backed behavior, then build the Blazor project and rebuild Tailwind output only if Tailwind source changes.

## Open Questions

- What exact DTO shape does `/lan/teams/me/summary` return for captain-owned teams, member teams, pending invite counts, captain IDs, logo URLs, and member lists? Answer during implementation by inspecting the linked backend.
- Captains should use general search for invite lookup for now, filtering results in the front-end to eligible user results where possible while still letting the backend enforce final invite rules.
- The team SignalR hub route, authentication requirement, and event payload contract should be answered during implementation by inspecting the linked backend.
- Should notification read/unread state be persisted by the backend, inferred from pending team invite state, or maintained locally until a backend notification endpoint exists?
- Which logo file extensions, MIME types, dimensions, and size limits should the UI pre-validate before upload?
- Are backend validation responses returned as structured validation problem details, domain exception messages, or another format?
- Pending invite counts should appear in the top-level notification badge as well as any expanded notification/menu surface.
