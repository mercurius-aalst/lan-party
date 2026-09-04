# Redirect safely after logout

## Why

Logout currently always redirects to the home page, even when the user leaves
a public page. The logout flow should preserve useful public context without
replaying a protected or attacker-controlled destination after the session is
cleared.

## What changes

- Capture the current relative URL when the authenticated navigation starts
  logout.
- Preserve that URL through the mock logout endpoint and the Auth0 provider
  round trip when it is a safe public application destination.
- Send live Auth0 logout through the fixed `/account/logout/callback` URI and
  carry the validated destination in short-lived, cryptographically protected
  browser state.
- Limit the validated logout target to 1024 characters and fall back to `/`
  before protection when it exceeds that limit; cap the protected state at
  3072 characters as a cookie-size guard with the same fallback.
- Fall back to home (or an equivalent safe public location) for protected,
  malformed, external, absolute, protocol-relative, backslash-prefixed, or
  control-character-containing targets.
- Keep login, registration, and Auth0 callback redirect behavior unchanged.
- Document the exact Auth0 Allowed Logout URLs required for the fixed callback.
- Add focused contract coverage for public, protected, and malicious targets.

## Non-goals

- Do not add a backend logout endpoint or put a dynamic local destination in an
  Auth0 allowlisted URL. Auth0 provider configuration is limited to the fixed
  callback URLs documented in `docs/auth0.md`.
- Do not add server-side logout state, nonce, locking, or replay storage. The
  browser cookie MUST be deleted on callback read on a best-effort basis, while
  the protected payload MUST enforce a five-minute cryptographic expiry.
- Do not change authenticated authorization or route protection behavior.
