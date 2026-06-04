## Context

The back end now exposes `GET /v1/lan/public/users/{username}` as an anonymous public profile endpoint. Its response contains username, first name, last name, and optional linked identity fields, while omitting private account fields. The front-end public user profile spec and implementation still reflect an older privacy model where anonymous visitors see only username and authenticated visitors may see linked IDs.

The front end already has a dedicated public profile route, DTO, service abstraction, mock service, and branded not-found state. The change is an alignment pass rather than a new feature area.

## Goals / Non-Goals

**Goals:**

- Align the front-end public user profile spec with the back-end public profile contract.
- Extend the front-end public profile DTO to consume first name and last name from the public API response.
- Render first name, last name, username, and returned linked IDs on `/users/{username}`.
- Keep linked IDs optional and avoid blank placeholders when values are absent.
- Preserve existing route, service-level 404 handling, and branded not-found behavior.
- Align mock public profile behavior with the live public contract.

**Non-Goals:**

- Do not add profile enrichment from current-user or admin user endpoints.
- Do not expose private account fields such as email, Auth0 ID, roles, deletion state, or timestamps.
- Do not change team profile behavior or tournament participant popup behavior except where shared public profile DTO behavior requires it.
- Do not change the back-end API contract.

## Decisions

1. Follow the back-end public DTO as the source of truth.

   The front end will treat username, first name, last name, Discord ID, Steam ID, and Riot ID as the complete public profile response shape. This avoids maintaining a separate caller-dependent privacy model in the UI.

   Alternative considered: preserve anonymous/authenticated differences on the front end. That conflicts with the back-end contract and would make mock/live behavior diverge.

2. Keep username as the route and navigation identity.

   The profile page will display the person's name as profile content while keeping username visible in the breadcrumb and detail surface. Links remain `/users/{username}`.

   Alternative considered: use the full name as the primary route label everywhere. That would blur the stable public identifier used by search, team rosters, and participant links.

3. Treat linked identities as optional fields.

   The page will render Discord, Steam, and Riot IDs only when non-empty values are returned. Missing identities will not create placeholder rows.

   Alternative considered: render all linked identity labels with empty or "not provided" values. That adds noise and makes absent optional fields feel like data errors.

4. Keep not-found behavior at the public profile service boundary.

   The existing production service converts API 404 responses into `null`, which lets the page show the branded not-found state. That remains the right boundary for missing, deleted, or incomplete users.

   Alternative considered: handle Refit exceptions directly in the component. That would leak transport concerns into the UI and duplicate existing service behavior.

## Risks / Trade-offs

- Privacy expectations may differ from the older front-end spec -> Mitigation: the updated spec explicitly follows the back-end public contract and still lists private fields that MUST remain absent.
- Mock data can accidentally diverge from live behavior -> Mitigation: update mock public profile responses to always use the same public field visibility as the live endpoint.
- Full names may be missing in malformed local data -> Mitigation: treat missing or incomplete public profiles as not found where practical, and keep UI rendering resilient to empty optional linked IDs.

## Migration Plan

No data migration is required. The implementation can ship as a front-end-only contract alignment once the DTO, UI, mock behavior, and specs are updated. Rollback is limited to reverting the front-end change because the back-end contract is already established.

## Open Questions

None.
