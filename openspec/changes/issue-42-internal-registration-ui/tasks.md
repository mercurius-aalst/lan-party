## 1. OpenSpec and contract baseline

- [x] 1.1 Verify issue #42 requirements against PR #53 and record the current backend registration
      routes, DTOs, auth boundary, no-content mutations, and supported roster confirmation action.
- [x] 1.2 Confirm PR #53 already removes `RegisterFormUrl` from live tournament models, forms,
      payload construction, and mock registration behavior.

## 2. Registration interaction UI

- [x] 2.1 Add a three-step MudBlazor Stepper for captain team selection, exact roster selection, and
      review/submit, with mobile-safe layout and navigation guards.
- [x] 2.2 Require explicit confirmation before individual registration, individual unregister, and
      captain team unregister mutations.
- [x] 2.3 Show pending, active, and captain-editable registration state and explain why confirmed
      non-captain members cannot self-unregister.
- [x] 2.4 Show team and per-member eligibility reasons, keep the captain in the roster, and prevent
      invalid team-size submissions before calling the backend.
- [x] 2.5 Refresh the public tournament and current-user state after successful mutations and keep
      recoverable backend errors visible without optimistic state.

## 3. Privacy and parity

- [x] 3.1 Keep public participant rendering limited to active privacy-safe registrations and avoid
      rendering current-user-only pending confirmation metadata publicly.
- [x] 3.2 Keep anonymous flows from calling authenticated registration endpoints and retain loading,
      empty, unauthorized, not-found, and API-error states.
- [x] 3.3 Keep mock behavior aligned with the live interaction states and remove any active copy that
      directs tournament registration to Google Forms or another external form.

## 4. Validation

- [x] 4.1 Run the relevant contract tests and build the Blazor project from the PR #53 baseline.
- [x] 4.2 Run `openspec validate issue-42-internal-registration-ui` and complete this checklist.
- [x] 4.3 Do not claim browser/manual smoke coverage; document it as intentionally skipped.
