# Tasks

## OpenSpec and contract

- [x] 1.1 Inspect the existing Auth0, mock-login, profile-completion, return-url,
      and navigation seams and confirm no backend contract gap is required.
- [x] 1.2 Add the account-access specification delta for separate login and
      registration behavior.

## Account access UI and flow

- [x] 2.1 Add separate Login and Register actions to signed-out desktop and
      mobile navigation while preserving authenticated navigation.
- [x] 2.2 Keep the login challenge direct and existing-user focused; preserve
      safe local return-url handling and mock-login parity.
- [x] 2.3 Add a registration onboarding route that guides account/profile
      completion through logical steps with required/optional guidance and
      inline validation.
- [x] 2.4 Handle registration back/cancel navigation and completion or error
      states without losing safe return context.
- [x] 2.5 Reuse existing profile DTOs, validation, Auth0 claims, API calls, and
      styling patterns without adding dependencies.

## Validation

- [x] 3.1 No suitable existing component/auth-navigation test seam is available;
      no test framework was added.
- [x] 3.2 Build the Blazor project and document skipped app/browser smoke tests.
