## Why

The integrated front end already consumes the backend's internal tournament-registration contract,
but the tournament detail registration surface still behaves like an early integration panel. Team
captains do not receive the requested three-step MudBlazor flow, registration mutations happen
without an explicit confirmation, and pending/editable states do not explain all eligibility and
ownership rules. This leaves the internal registration experience incomplete even though the
external registration URL has already been removed from the live tournament model and forms.

## What Changes

- Keep tournament registration entirely inside the application and ensure active UI/legal copy does
  not direct users to an external registration form.
- Add explicit confirmation before individual registration, individual unregister, and captain team
  unregister actions.
- Replace the team registration controls with a three-step MudBlazor Stepper for captain-owned team
  selection, exact roster selection, and review/submit.
- Surface backend team and per-member eligibility reasons, keep the captain selected, and prevent
  progressing or submitting an invalid roster locally while retaining backend authority.
- Make pending, active, and editable captain-managed team states visible, including the rule that
  confirmed roster members cannot self-unregister.
- Refresh tournament and current-user registration state after every successful mutation and keep
  the last confirmed state after errors.
- Preserve anonymous, live, and mock behavior with loading, empty, unauthorized, unavailable, and
  recoverable error states.

## Baseline Coverage From PR #53

PR #53, targeted as the dependency baseline, already supplies the versioned Refit registration
routes, registration DTOs, tournament `TeamSize`, public active-registration projection, service
wrappers, mock registration transitions, and the public tournament registration panel. This change
completes the remaining interaction and state requirements without adding backend endpoints.

The current backend contract has two important boundaries that the UI handles explicitly: read-side
team and roster eligibility reports an existing registration as a duplicate, while the submit route
revalidates the current registration as an editable replacement; and the pending roster-member
response does not include team identity or name. The UI preserves the current captain roster for
editing only when the conflict belongs to that registration, relies on the submit response as the
authority, and labels the pending context limitation instead of inventing data or an endpoint.

## Non-Goals

- Do not add or change backend registration routes, persistence, authorization, or validation.
- Do not add a client-side decline action because the verified backend contract exposes only the
  authenticated `Confirmed` roster-member action.
- Do not expose pending roster data or private account fields in public participant surfaces.
- Do not invent team context for pending confirmations that is not present in the current backend
  DTO; show the available tournament/member context and identify the contract limitation.
- Do not run browser/manual smoke testing as part of this implementation unless explicitly requested.
