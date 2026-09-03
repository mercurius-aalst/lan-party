# Account access Specification

## Purpose

Defines anonymous login and registration entry points and the authenticated
profile-onboarding handoff for the Mercurius LAN front end.

## ADDED Requirements

### Requirement: Signed-out navigation distinguishes account actions

The signed-out header navigation MUST expose separate Login and Register actions
on mobile and desktop layouts.

#### Scenario: Existing user chooses login

- **WHEN** an anonymous visitor selects Login
- **THEN** the application MUST start the existing-user authentication challenge
- **AND** the login entry point MUST remain focused on signing in rather than
  collecting profile-registration details

#### Scenario: New user chooses registration

- **WHEN** an anonymous visitor selects Register
- **THEN** the application MUST start the registration authentication challenge
- **AND** the challenge MUST request account creation semantics from the identity
  provider
- **AND** the current safe local return destination MUST be preserved

### Requirement: Registration reuses authenticated profile onboarding

After a successful registration challenge, the application MUST reuse the
existing authenticated profile-completion contract to collect profile data.

#### Scenario: Registration challenge completes

- **WHEN** the identity provider returns an authenticated new user
- **THEN** the application MUST present guided profile onboarding when the
  profile is incomplete
- **AND** required and optional profile fields MUST be distinguishable
- **AND** field validation errors MUST be shown inline where the existing form
  validation supports them

#### Scenario: Registration returns an already-complete profile

- **WHEN** the authenticated registration flow loads a profile that is already
  complete
- **THEN** the application MUST redirect to the validated local return
  destination
- **AND** it MUST NOT display registration completion before a successful
  profile completion request

#### Scenario: Profile onboarding completes

- **WHEN** the user submits valid required profile data and the profile request
  succeeds
- **THEN** the application MUST show a clear completion or authenticated state
- **AND** the user MUST be returned to the validated local destination

### Requirement: Registration navigation is recoverable

The registration flow MUST provide clean back and cancellation behavior without
discarding a safe return destination or leaving a stale onboarding state.

#### Scenario: User cancels registration

- **WHEN** a user cancels before completing profile onboarding
- **THEN** the application MUST return to a safe local destination or home
- **AND** it MUST NOT submit incomplete profile data

#### Scenario: User goes back from onboarding

- **WHEN** a user chooses Back during registration onboarding
- **THEN** the application MUST return to the previous registration step or
  entry destination without losing already entered values unnecessarily

#### Scenario: Registration or profile request fails

- **WHEN** the identity-provider challenge or profile request fails
- **THEN** the application MUST show a recoverable, understandable error state
- **AND** it MUST NOT treat the user as authenticated solely because a client
  navigation action was attempted

### Requirement: Account redirects are safe and parity-preserving

Login and registration redirects MUST accept only local application destinations
and MUST preserve mock-login behavior when mock backend mode is enabled.

#### Scenario: Unsafe return URL is supplied

- **WHEN** login or registration receives an absolute, protocol-relative,
  backslash-prefixed, or control-character-containing return URL
- **THEN** the application MUST fall back to home
- **AND** it MUST NOT redirect to an external destination

#### Scenario: Mock mode is enabled

- **WHEN** an anonymous visitor uses login or registration while mock backend
  mode is enabled
- **THEN** the existing mock authentication path MUST remain usable
- **AND** the resulting authenticated navigation MUST retain its current
  behavior

### Requirement: Authenticated navigation remains stable

Authenticated users MUST continue to receive the existing profile, team,
notification, admin, logout, and ticket navigation without a registration action
being shown as an alternative authenticated state.

#### Scenario: Authenticated visitor views the header

- **WHEN** a visitor is authenticated
- **THEN** the header MUST show the existing authenticated navigation
- **AND** signed-out Login and Register actions MUST NOT be shown
