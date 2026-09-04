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
- [x] 2.3 Preserve the validated target in the mock direct redirect and in the
      live provider callback's encoded `returnUrl` query without changing
      login, registration, or Auth0 callback behavior.
- [x] 2.4 Revalidate the callback query target and redirect locally, falling
      back to `/` for missing, protected, unsafe, or malformed values.

## Validation

- [x] 3.1 Retain the existing focused contract coverage for public URLs with
      query/fragment, protected routes and descendants, canonicalization
      variants, and malicious helper inputs.
- [x] 3.2 Run strict OpenSpec validation, focused helper tests, the full
      contract suite, app build, diff checks, and a direct query round-trip
      and callback-revalidation probe; document skipped app/browser/live
      Auth0 smoke testing.
- [x] 3.3 Document the exact development and production Auth0 Allowed Logout
      URL entries and the provider's query/hash validation behavior.
