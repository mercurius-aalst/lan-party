## 1. Backend Contract And DTO Alignment

- [x] 1.1 Verify backend `TeamEndpoints`, DTO response shapes, route versions, logo upload constraints, and validation/error payload formats in `mercurius-aalst/mercurius-aalst-back-end`.
- [x] 1.2 Verify backend team SignalR hub route, authentication behavior, event names, payload shapes, reconnect expectations, and events for invites, invite responses, membership changes, captain transfer, and logo/team updates.
- [x] 1.3 Add front-end DTOs for current-user team summary, received invites, sent invites, team members, captain transfer, invite responses, logo responses, and team hub event payloads as needed by the verified backend contract.
- [x] 1.4 Update `ILANClient` with explicit Refit methods for current-user summary, received invites, sent invites, leave team, remove member, create invite, cancel invite, respond to invite, transfer captain, upload logo, and remove logo routes.
- [x] 1.5 Update `ITeamService` and `TeamService` to expose feature-facing methods for all user-owned team management actions, including captain member removal, and normalize API failures into user-facing messages where existing service patterns allow.
- [x] 1.5.1 Replace generic team service mutation wrappers with action-specific exception handling that preserves failed action context, HTTP status, and API response content.
- [x] 1.6 Ensure authenticated create-team flow uses the backend current-user authorization model and does not require the UI to choose or trust the acting user's ID.

## 2. Remove Admin Team Management

- [x] 2.1 Identify existing admin team management pages, routes, navigation entries, services, DTO usages, and mock/admin-only affordances.
- [x] 2.2 Remove admin team management navigation entries and route registrations without disrupting remaining admin destinations.
- [x] 2.3 Remove or replace deprecated admin team management pages/components so direct route access no longer renders admin team workflows.
- [x] 2.4 Remove admin-only team lifecycle controls for create, edit, delete, invite, remove member, captain transfer, and logo management.
- [x] 2.5 Clean up now-unused admin team management service methods, DTO references, and mock affordances when they are not needed by user-owned or public team flows.

## 3. Mock Backend Support

- [x] 3.1 Extend mock data with captain-owned teams, member teams, received invites, sent invites, captain IDs, member lists, and logo/no-logo examples.
- [x] 3.2 Update mock services/store behavior for create team, invite user, cancel invite, accept invite, decline invite, leave team, transfer captainship, upload logo, and remove logo actions.
- [x] 3.3 Add representative mock blocked-action responses for duplicate invites, invite cooldowns, leave restrictions, captain transfer errors, and invalid logo files where practical.
- [x] 3.4 Add mock user-owned team deletion behavior restricted to the current captain.
- [x] 3.5 Add mock captain member-removal behavior with representative blocked-action states.

## 4. Team Management UI

- [x] 4.1 Add an authenticated team management page route with responsive desktop and mobile layouts.
- [x] 4.2 Render captain-owned teams, member teams, and received pending invites with loading, empty, error, unauthorized, and not-found/recovery states, with teams deduplicated by ID in the overview.
- [x] 4.3 Use branded native Blazor/HTML controls for team management inputs, dialogs, menus, lists, badges, progress indicators, alerts, upload affordances, and confirmations.
- [x] 4.4 Style team management controls with component-scoped CSS, theme-compatible values, and existing layout patterns so they match Mercurius branding.
- [x] 4.5 Add team creation form behavior with validation, backend error preservation, post-success summary refresh, and shared toast-framework confirmation.
- [x] 4.6 Add captain team detail controls for inviting users through authenticated user search results with invite target IDs, reusing the global search result item presentation in an anchored dropdown that layers over the invite dialog.
- [x] 4.7 Add captain sent-invite list and cancel-invite behavior with post-success state refresh.
- [x] 4.8 Add received invite accept and decline actions with predictable invite and membership list updates.
- [x] 4.9 Add member leave behavior with confirmation, backend blocked-action feedback, and post-success state refresh.
- [x] 4.10 Add captain transfer controls restricted to current team members with post-transfer permission/state refresh.
- [x] 4.11 Add team logo upload, replace, preview, invalid-file feedback, saved-state preservation, and remove-logo behavior.
- [x] 4.12 Keep public team profile rendering separate from authenticated management state and avoid rendering private account fields in invite lookup or team management lists.
- [x] 4.13 Add captain-only remove-team behavior with confirmation, service-backed feedback, and post-success summary refresh.
- [x] 4.14 Normalize team management card sizing, spacing, captain badge placement, logo upload affordances, create/invite dialogs, and hero/danger-section styling.
- [x] 4.15 Navigate to public user profiles when member participant cards are selected.
- [x] 4.16 Render saved team logos as team participant card avatars with initials only as the no-logo fallback.
- [x] 4.17 Resolve backend-relative team logo paths against the configured backend base address before rendering.
- [x] 4.18 Keep logo avatar backgrounds transparent so transparent logo pixels reveal the parent surface.
- [x] 4.19 Add captain-only remove-member behavior for non-captain members with confirmation, backend blocked-action feedback, post-success summary refresh, and stale transfer-selection cleanup.

## 5. Navigation And Invite Discoverability

- [x] 5.1 Add a Blazor-native authenticated navigation entry for the team management area in desktop and mobile navigation surfaces.
- [x] 5.2 Add a branded user-level notification control with count in authenticated desktop and mobile navigation without exposing private notification details to anonymous visitors.
- [x] 5.3 Show pending invite counts in the top-level notification badge as well as the expanded notification/menu surface.
- [x] 5.4 Add a compact notification list/dropdown that can show team invite and team update notifications with actions or links to the team management area.
- [x] 5.5 Refresh or reconcile notification state after accept, decline, read, dismiss, and team summary refresh actions.
- [x] 5.6 Refresh or reconcile notification state after relevant team SignalR hub notifications.
- [x] 5.7 Preserve existing menu close behavior, active-state styling, user menu behavior, remaining admin navigation, search, and ticket CTA hierarchy.
- [x] 5.8 Keep the notification bell/count visually integrated with the authenticated user widget while opening notifications in a separate dropdown wide enough to prevent action overlap.
- [x] 5.9 Close authenticated user and notification dropdowns when the user clicks outside the widget/dropdown surface.

## 6. Team SignalR Notifications

- [x] 6.1 Add authenticated team SignalR hub client setup using the existing auth/access-token patterns.
- [x] 6.2 Handle hub events for new received invites, invite responses, canceled invites, membership changes, captain transfer, and logo/team updates as user-level notification inputs.
- [x] 6.3 Treat hub events as invalidation signals that refresh backend-authoritative summary, invite, and notification state instead of trusting hub payloads as complete state.
- [x] 6.4 Add reconnect/disconnect handling so navigation and team management remain usable through REST-backed refreshes when the hub is unavailable.
- [x] 6.5 Dispose hub subscriptions/connections from Blazor components or scoped services to avoid duplicate notifications.

## 7. Verification

- [x] 7.1 Build the Blazor project with `dotnet build src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj`.
- [x] 7.2 Rebuild Tailwind output if `Styles/tailwind.css` or utility-class generation inputs change.
- [ ] 7.3 Verify admin team management routes/navigation no longer expose deprecated admin workflows.
- [ ] 7.4 Verify team management flows in mock mode for create, invite, cancel invite, accept, decline, leave, captain transfer, upload logo, remove logo, and blocked-action states.
- [ ] 7.5 Verify API-backed behavior against the backend team endpoints and team SignalR hub or document any backend contract gaps that cannot be exercised.
- [ ] 7.6 Verify responsive desktop/mobile layout, top-level notification bell/count behavior, notification list behavior, and live team notification updates.
- [x] 7.7 Update this task checklist as implementation work is completed.
