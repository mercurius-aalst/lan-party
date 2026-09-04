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
