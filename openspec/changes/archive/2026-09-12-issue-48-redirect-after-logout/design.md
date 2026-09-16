## Context

The navigation captures a validated local return target and the live logout endpoint carries it through a fixed Auth0 callback. The endpoint previously initiated Auth0 sign-out before clearing the application cookie. If remote sign-out failed or short-circuited the response, local cookie clearing could be skipped; returning to a protected route could then preserve or silently recreate an authenticated session.

## Goals / Non-Goals

**Goals:**

- End the local application session independently of Auth0 availability.
- Preserve the centralized public-only logout return-target policy.
- Keep the fixed callback and mock logout behavior compatible.

**Non-Goals:**

- Add federated logout, change Auth0 SSO-session policy, or call a backend logout endpoint.
- Change login, registration, authorization, or profile-completion rules.

## Decisions

1. Clear the cookie authentication scheme before invoking the Auth0 sign-out handler. If the remote handler fails, log the failure and complete the response with a redirect to the validated public return target. Handling the failure inside the endpoint preserves the cookie deletion header instead of allowing production exception handling to clear the unstarted response.
2. Continue to validate the requested return target before building the Auth0 callback, and revalidate it at the callback boundary. Protected or malformed targets fall back to `/`, preventing post-logout navigation from immediately challenging and silently reauthenticating against an Auth0 SSO session.
3. Keep mock logout as a direct cookie sign-out followed by a validated local redirect. It has no remote provider step and therefore needs no callback.

## Risks / Trade-offs

- [Risk] Auth0 may still retain its SSO session when provider logout fails. → The application is locally signed out; a later explicit login may reuse Auth0 SSO, which is intentionally outside this change's non-federated scope.
- [Risk] Provider callback configuration can reject an otherwise valid flow. → Keep the callback URI fixed and document its exact allowlist entries.

## Migration Plan

Deploy the front-end endpoint ordering and callback policy together. No data migration is required. Rollback restores the previous endpoint implementation but reintroduces the local-session risk.

## Open Questions

None.
