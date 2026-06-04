## 1. Contract Alignment

- [x] 1.1 Extend `PublicUserProfileDTO` with `Firstname` and `Lastname` fields matching the back-end public profile response.
- [x] 1.2 Confirm the live public profile client continues to call `/v1/lan/public/users/{username}` through the existing Refit base address and does not call private user endpoints.
- [x] 1.3 Keep `PublicProfileService` 404 handling mapped to `null` for branded not-found rendering.

## 2. Public Profile Rendering

- [x] 2.1 Update `/users/{username}` to display first name, last name, and username from the public profile response.
- [x] 2.2 Keep username visible as the route/navigation identifier in the profile layout.
- [x] 2.3 Render Discord ID, Steam ID, and Riot ID only when non-empty values are returned.
- [x] 2.4 Ensure missing linked identities do not render empty labels, placeholder rows, or broken layout.
- [x] 2.5 Preserve loading, error, and branded not-found states for missing, deleted, or incomplete users.

## 3. Mock Backend Behavior

- [x] 3.1 Update mock public profile lookup to return first name and last name for public profiles.
- [x] 3.2 Align mock linked identity visibility with the live public contract for anonymous and authenticated callers.
- [x] 3.3 Ensure local mock data still includes useful users with complete names and optional linked identity coverage.

## 4. Verification

- [x] 4.1 Build the Blazor project with `dotnet build src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj`.
- [ ] 4.2 Run or manually verify `/users/{username}` for a user with linked IDs, a user missing one or more linked IDs, and an unknown user.
- [x] 4.3 Note any backend contract assumptions or gaps that could not be verified locally.

Verification note: local build passed. Browser and route-level verification could not be completed because the in-app browser setup failed with a Windows sandbox startup error, and subsequent shell cleanup/check commands were blocked by tool approval usage limits. The back-end contract was verified from the local back-end OpenSpec spec and implementation, but this front-end run did not exercise a live back-end process.
