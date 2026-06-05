## Context

`NavMenu` currently derives the user-menu label from `ClaimsPrincipal.Identity.Name`, and mock-login sets `ClaimTypes.Name` to `DisplayName`. For real Auth0 sessions, the `name` claim can also be a full name. The application profile username lives in the current-user profile response, so claims alone are not a reliable source for the Mercurius username.

## Goals / Non-Goals

**Goals:**
- Render the Mercurius username in the authenticated header menu trigger when available.
- Avoid repeatedly calling the current-user endpoint on every render.
- Keep fallback behavior for incomplete profiles, unauthorized responses, and provider-only sessions.

**Non-Goals:**
- Change public profile display names.
- Change Auth0 account metadata.
- Redesign the header action button.

## Decisions

- Load the current user profile once per authenticated identity in `NavMenu` and cache the username in component state.
- Add a username claim for mock-login so mock sessions render correctly immediately and still work if profile loading is unavailable.
- Keep claim-based fallbacks in order of username-like claims, then display-name/email, then `Account`.

## Risks / Trade-offs

- [Risk] The header may briefly show a provider fallback before the profile request completes. -> Mitigation: the loaded username replaces it as soon as the current profile returns.
- [Risk] Current-user lookup may fail for an incomplete or expired session. -> Mitigation: the menu keeps existing fallback label behavior and does not block navigation.
