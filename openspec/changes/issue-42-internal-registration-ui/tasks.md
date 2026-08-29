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
- [x] 2.6 Revalidate the captured tournament route immediately before every mutation and abort a
      stale confirmation/dialog action when navigation or parameters changed.
- [x] 2.7 Validate the selected exact roster separately from candidate-reason discovery, including
      oversized teams and existing registrations with removed members.
- [x] 2.8 Show immediate loading states, disable stale registration actions during refresh, and add
      retry actions for team-summary, team-eligibility, and roster-eligibility failures.
- [x] 2.9 Refresh both public and authenticated projections after realtime invalidation, preserve or
      warn about dirty roster drafts, and provide manual refresh coverage for backend event gaps.
- [x] 2.10 Consume the authenticated current-team registration context for pending and active
       roster states, retain active-field compatibility fallback, and show non-captain roster status
       without self-unregister controls.
- [x] 2.11 Bound per-candidate roster eligibility discovery to 50 user ids per backend request and
       merge chunk results deterministically, including former roster members.
- [x] 2.12 Reconcile an existing roster after captain transfer by selecting the current captain,
       removing the former captain when exact size requires it, and warning the captain to review.

## 3. Privacy and parity

- [x] 3.1 Keep public participant rendering limited to active privacy-safe registrations and avoid
      rendering current-user-only pending confirmation metadata publicly.
- [x] 3.2 Keep anonymous flows from calling authenticated registration endpoints and retain loading,
      empty, unauthorized, not-found, and API-error states.
- [x] 3.3 Keep mock behavior aligned with the live interaction states and remove any active copy that
      directs tournament registration to Google Forms or another external form.
- [x] 3.4 Seed a default mock captain team with the configured roster size and a pending member
      persona so the internal workflow is exercisable locally.

## 4. Validation

- [x] 4.1 Run the relevant contract tests and build the Blazor project from the PR #53 baseline.
- [x] 4.2 Run `openspec validate issue-42-internal-registration-ui` and complete this checklist.
- [x] 4.3 Do not claim browser/manual smoke coverage; document it as intentionally skipped.
- [x] 4.4 Add focused regression coverage for exact roster validation, schedule and ownership gates,
      pending-confirmation revalidation, and mock fixture transitions. The existing test project has
      no component-rendering harness, so route-dialog and retry rendering coverage remains covered by
      guarded component code plus the successful build.
- [x] 4.5 Validate same-tournament parameter changes and current-team registration deserialization/
       mock parity without exposing pending data through public participant projections.
- [x] 4.6 Add regression coverage for saved-but-unrefreshed mutation state, 51-member candidate
       discovery batching, and captain-transfer reconciliation including TeamSize=1.
- [x] 4.7 Guard public/authenticated refresh commits with tournament route and registration
       generation snapshots, and preserve/warn on dirty drafts when their selected team disappears.
