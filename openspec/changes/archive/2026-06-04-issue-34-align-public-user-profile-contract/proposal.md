## Why

The redesigned front-end public user profile page is out of sync with the back-end public profile contract. The front-end spec and DTO still treat first name, last name, and linked IDs as partially hidden, while the back end now exposes them as privacy-approved public profile fields for anonymous and authenticated callers.

## What Changes

- Update the public user profile requirements so `/users/{username}` follows the back-end `GET /v1/lan/public/users/{username}` response shape.
- Display first name and last name from the public profile response while keeping username visible as the route and navigation identifier.
- Display Discord ID, Steam ID, and Riot ID only when those optional values are returned by the API.
- Preserve branded not-found handling for missing, deleted, or incomplete users.
- Preserve privacy boundaries by continuing to omit email, email verification state, Auth0 ID, deletion state, roles, timestamps, and admin/internal fields.
- Align mock public profile behavior with the same public response shape used by the live API.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `public-participant-profiles`: Public user profile rendering changes from username-only/authorization-dependent visibility to the back-end public profile contract for all callers.

## Impact

- OpenSpec public participant profile requirements.
- Front-end public user profile DTO and `/users/{username}` rendering.
- Public profile mock service/store and local mock data behavior.
- No new package dependencies are expected.
