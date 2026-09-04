# Redirect safely after logout

## Why

Logout currently always redirects to the home page, even when the user leaves
a public page. The logout flow should preserve useful public context without
replaying a protected or attacker-controlled destination after the session is
cleared.

## What changes

- Capture the current relative URL when the authenticated navigation starts
  logout.
- Preserve that URL through the mock and Auth0 logout endpoints when it is a
  safe public application destination.
- Fall back to home (or an equivalent safe public location) for protected,
  malformed, external, absolute, protocol-relative, backslash-prefixed, or
  control-character-containing targets.
- Keep login, registration, and Auth0 callback redirect behavior unchanged.
- Add focused contract coverage for public, protected, and malicious targets.

## Non-goals

- Do not add a backend logout endpoint or change Auth0 provider configuration.
- Do not persist return URLs beyond the current logout request.
- Do not change authenticated authorization or route protection behavior.
