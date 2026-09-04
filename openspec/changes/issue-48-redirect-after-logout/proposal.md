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
- Send live Auth0 logout through the fixed `/account/logout/callback` URI with
  the already validated destination in an encoded `returnUrl` query
  parameter.
- Revalidate the callback's decoded `returnUrl` with the existing local logout
  policy and fall back to `/` for missing, protected, unsafe, or malformed
  values.
- Fall back to home (or an equivalent safe public location) for protected,
  malformed, external, absolute, protocol-relative, backslash-prefixed, or
  control-character-containing targets.
- Keep login, registration, and Auth0 callback redirect behavior unchanged.
- Document the exact Auth0 Allowed Logout URLs required for the fixed callback.
- Add focused contract coverage for public, protected, and malicious targets.

## Non-goals

- Do not add a backend logout endpoint or put a dynamic local destination in an
  Auth0 allowlisted path. Auth0 provider configuration is limited to the fixed
  callback URLs documented in `docs/auth0.md`; the destination travels in the
  callback query string.
- Do not change authenticated authorization or route protection behavior.
