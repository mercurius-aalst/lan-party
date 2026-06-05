## Why

The authenticated header user-menu trigger currently displays the signed-in user's full display name, which can wrap awkwardly and differs from the public username used elsewhere in the site. The header should identify the account by username when a complete profile is available.

## What Changes

- Show the current user's Mercurius username in the authenticated header menu trigger.
- Prefer a loaded current-user profile username over provider display-name claims.
- Preserve sensible fallbacks for incomplete profiles or failed profile lookups.

## Capabilities

### New Capabilities

### Modified Capabilities
- `site-navigation`: Updates authenticated header user-menu label behavior.

## Impact

- `src/Mercurius.LAN.Web/Components/Layout/NavMenu.razor.cs`
- `src/Mercurius.LAN.Web/Extensions/DependencyExtensions.cs`
- Authenticated header display only; no API contract change.
