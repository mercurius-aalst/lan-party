# Tasks

## OpenSpec and return-target policy

- [x] 1.1 Inspect the existing logout endpoints, navigation URL capture,
      local-return helper, and account redirect flows; confirm no backend
      contract change is required.
- [x] 1.2 Add RFC 2119 requirements for safe post-logout redirects and public
      versus protected destinations.

## Logout redirect implementation

- [x] 2.1 Capture the current relative path, query, and fragment before the
      navigation logout action and pass it as a URL-encoded return target.
- [x] 2.2 Reuse the centralized local return policy to reject external,
      absolute, protocol-relative, backslash-prefixed, control-character,
      dot-segment, encoded-separator, malformed-escape, and nested-encoding
      targets; fall back from protected descendants and `/account/logout`.
- [x] 2.3 Apply the validated target to the mock-cookie logout flow and stage
      it for live provider state without changing login, registration, or
      callback behavior.
- [x] 2.4 Route live provider logout through the fixed
      `/account/logout/callback` URI and carry the validated target in
      a purpose-specific, cryptographically time-limited logout cookie with
      bounded target and protected-state sizes.
- [x] 2.5 Consume and clear logout state at the anonymous callback, fall back
      to `/` for missing, expired, or tampered state, revalidate the recovered
      target, and preserve the existing mock flow. Cookie deletion is
      best-effort; no server-side replay store is required.

## Validation

- [x] 3.1 Add contract tests covering public URLs with query/fragment,
      protected routes and descendants, canonicalization variants, and
      malicious targets at both single- and double-decoded helper inputs.
- [x] 3.2 Run OpenSpec validation, restore/build, and focused contract tests;
      document skipped app/browser smoke testing. Endpoint-level logout
      coverage remains unavailable because this contract test project does not
      reference a TestServer/WebApplicationFactory harness; adding that
      package or harness is outside this focused correction.
- [x] 3.3 Add focused platform-state tests for protection, genuine expiry,
      tamper/missing-state fallback, bounded targets, delete-on-read, cookie
      settings, independent contexts, and fixed callback invariance.
- [x] 3.4 Document exact development and production Auth0 Allowed Logout URL
      entries for the fixed callback and rerun the required validation.
