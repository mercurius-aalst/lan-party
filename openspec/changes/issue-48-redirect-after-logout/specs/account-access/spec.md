# Account access Specification

## Purpose

Defines safe post-logout navigation for the Mercurius LAN front end while
preserving existing login, registration, mock authentication, and Auth0
callback behavior.

## ADDED Requirements

### Requirement: Logout preserves safe public context

The application MUST capture the current relative URL before an authenticated
user logs out and MUST return to that URL after logout when it is a public,
local application destination.

#### Scenario: User logs out from a public page

- **WHEN** an authenticated user logs out from a public page with a path,
  query, or fragment
- **THEN** the logout request MUST carry the current relative URL as its
  return target
- **AND** the user MUST be returned to the same public path, query, and
  fragment after the session is cleared

### Requirement: Logout never replays protected destinations

The application MUST treat authenticated-only routes as unsafe logout return
targets and MUST use a safe public fallback such as `/` after logout.

#### Scenario: User logs out from a protected page

- **WHEN** an authenticated user logs out from a protected route such as
  `/profile`, `/complete-profile`, `/teams/manage`, or `/admin/sponsors`
- **THEN** the logout flow MUST return to a safe public location
- **AND** it MUST NOT retain or replay the protected route, query, or fragment

### Requirement: Logout return targets resist URL manipulation

The logout return-target policy MUST reject external, absolute,
protocol-relative, backslash-prefixed, and control-character-containing
targets, including when those values are supplied through query-string
encoding. It MUST also reject ambiguous local paths containing dot segments,
encoded dot segments, internal or encoded backslashes, encoded separators,
encoded control characters, malformed escapes, or nested percent encoding.
Protected descendant paths and `/account/logout` itself MUST be treated as
unsafe.

#### Scenario: Attacker supplies a malicious logout target

- **WHEN** a logout request contains an unsafe return target
- **THEN** the application MUST use the safe public fallback
- **AND** it MUST NOT redirect the browser outside the application

#### Scenario: Ambiguous target attempts canonicalization

- **WHEN** a logout request contains a local target that can resolve through
  dot-segment, backslash, encoded-separator, control-character, malformed
  escape, or repeated-decoding behavior
- **THEN** the application MUST use the safe public fallback
- **AND** it MUST NOT normalize the target before deciding whether it is safe

#### Scenario: Public target contains query and fragment data

- **WHEN** a logout request contains a valid local public target with query and
  fragment components
- **THEN** validation MUST preserve those components exactly
- **AND** validation MUST evaluate the route path separately from its query
  and fragment

### Requirement: Existing account redirects remain compatible

The logout redirect change MUST NOT alter the existing login, registration,
mock-login, Auth0 challenge, or Auth0 callback redirect behavior.

#### Scenario: User authenticates through login or registration

- **WHEN** login or registration completes or returns through an Auth0 callback
- **THEN** its existing validated local return destination behavior MUST remain
  unchanged
- **AND** the logout return-target policy MUST NOT be used to discard a valid
  account onboarding or callback destination

### Requirement: Live provider logout uses a fixed callback and time-limited protected state

The live Auth0 logout flow MUST send the provider to the fixed local
`/account/logout/callback` URI. It MUST NOT reflect the requested local target
in that provider callback URI. Before provider sign-out, the application MUST
store the already validated logout target in authenticated and confidential
state protected by a five-minute cryptographic expiry. Validated logout
targets MUST be limited to 1024 characters; longer targets MUST fall back to
`/` before protection. The state MUST use a purpose-specific, HttpOnly cookie
and the protected value MUST be limited to 3072 characters as a cookie-size
guard, with `/` as the fallback if that limit is exceeded. The cookie MUST
have no Domain, Path `/account/logout`, SameSite `Lax`, and IsEssential set.
The cookie MUST have a short Max-Age and Expires value, and MUST be Secure
outside Development (and for HTTPS Development requests).

#### Scenario: Auth0 logout returns through the fixed callback

- **WHEN** an authenticated user logs out from a public page in live mode
- **THEN** the provider logout request MUST use the fixed
  `/account/logout/callback` callback
- **AND** the validated public path, query, and fragment MUST be carried in
  protected logout state rather than in the provider callback URI
- **AND** the user's cookie session MUST be cleared before the provider round
  trip completes

#### Scenario: Callback consumes valid state

- **WHEN** the provider returns to `/account/logout/callback` with valid,
  unexpired logout state
- **THEN** the callback MUST delete the state cookie before or while reading it
- **AND** it MUST revalidate the recovered target through the logout
  return-target policy
- **AND** it MUST issue a local redirect to the resulting safe target

#### Scenario: Callback receives missing, expired, or tampered state

- **WHEN** the provider returns without usable logout state
- **THEN** the callback MUST delete any presented state cookie on read
- **AND** it MUST redirect locally to `/`
- **AND** it MUST NOT trust a target supplied directly in the callback query

The browser cookie is a best-effort navigation hint: callback processing MUST
delete it on read, but the application MUST NOT add a server-side nonce,
replay store, or cryptographic single-use guarantee.
