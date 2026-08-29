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
- Consume the authenticated current-team registration context for any roster status so a confirmed
  member remains attached to a still-pending team while preserving the legacy active-only field.
- Preserve anonymous, live, and mock behavior with loading, empty, unauthorized, unavailable, and
  recoverable error states.
- Treat route identity and the backend's latest state as prerequisites immediately before every
  mutation, including after a confirmation dialog has been open.
- Validate the selected exact roster separately from per-candidate eligibility, provide retryable
  loading/error states, and refresh public/current-user projections after realtime invalidation
  without silently discarding an unsaved roster draft.

## Baseline Coverage From PR #53

PR #53, targeted as the dependency baseline, already supplies the versioned Refit registration
routes, registration DTOs, tournament `TeamSize`, public active-registration projection, service
wrappers, mock registration transitions, and the public tournament registration panel. This change
completes the remaining interaction and state requirements and consumes the companion backend
`CurrentTeamRegistration` field from issue #42's backend contract change. The FE keeps a fallback to
the legacy active-only field while the companion backend PR is rolling out.

The current backend contract has two important boundaries that the UI handles explicitly: read-side
team and roster eligibility reports an existing registration as a duplicate, while the submit route
revalidates the current registration as an editable replacement. The UI preserves the current
captain roster for editing only when the conflict belongs to that registration, relies on the submit
response as the authority, and consumes the authenticated current-team context without exposing it
through public projections.

## Non-Goals

- Do not add or change backend registration routes, persistence, authorization, or validation in
  this front-end change; the companion backend read-model field is tracked separately.
- Do not add a client-side decline action because the verified backend contract exposes only the
  authenticated `Confirmed` roster-member action.
- Do not expose pending roster data or private account fields in public participant surfaces.
- Do not invent team context for pending confirmations; consume `CurrentTeamRegistration` when
  supplied and retain the legacy fallback when an older backend response omits it.
- Do not claim realtime invalidation is complete for backend mutations that currently publish no
  roster-confirmation event; provide an explicit refresh path and document the event gap.
- Do not run browser/manual smoke testing as part of this implementation unless explicitly requested.
