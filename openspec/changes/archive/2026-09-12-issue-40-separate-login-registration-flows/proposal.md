# Separate login and registration flows

## Why

The signed-out header currently offers only a login-style action, so new users
cannot tell how to create an account. Login and registration should have clear,
separate entry points while continuing to use the existing Auth0 and profile
completion flow.

## What changes

- Add distinct Login and Register actions to signed-out navigation.
- Keep login focused on an existing-user Auth0 challenge.
- Add a registration onboarding page with logical profile steps, required and
  optional field guidance, inline validation, back/cancel navigation, and a
  clear completion state.
- Preserve mock-login parity and validate all return URLs as local destinations
  before redirecting.
- Keep authenticated navigation and the existing profile-completion behavior
  unchanged outside the new registration path.

## Non-goals

- Do not add backend endpoints or change Auth0 provider configuration.
- Do not add a new UI package or require a specific stepper component.
- Do not duplicate profile validation or weaken server-side validation.
