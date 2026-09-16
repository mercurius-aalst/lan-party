## 1. OpenSpec and contract alignment

- [x] 1.1 Validate this change strictly before source implementation and keep the front-end DTO and
      `ILANClient` routes aligned with the approved companion backend contract.
- [x] 1.2 Record the paginated pending-roster response, member-only `DELETE` decline semantics, and
      existing `PATCH` confirmation semantics in the OpenSpec proposal and implementation handoff
      used by both repos.

## 2. Registration invitation experience

- [x] 2.1 Surface a pending roster selection before the captain workflow with clear copy naming the
      tournament and team, state that the player has been selected, and explain the single next
      decision in user language.
- [x] 2.2 Add accessible Accept and Decline actions for the selected member. Accept MUST use the
      existing `Confirmed` mutation; Decline MUST use the member-only `DELETE` route and neither
      action may be shown for a missing, withdrawn, confirmed, or non-owned selection.
- [x] 2.3 Refresh the current-user registration state and relevant tournament state after either
      action, keep pending state visible while a request is in flight, and show recoverable backend
      errors without optimistic confirmation or decline.
- [x] 2.4 Remove technical captain-ownership filler from the selected-member state while retaining
      concise captain-managed wording only where it explains why a confirmed member has no edit or
      leave action.

## 3. Reload-safe notification flow

- [x] 3.1 Add the paginated pending-roster DTOs and API client method, loading every available page
      (or an explicit accessible continuation) without silently dropping items beyond page one.
- [x] 3.2 Extend `TeamNotificationService` to merge server-derived roster-selection items with team
      invites using stable, type-specific identifiers and tournament/team context, preserving read,
      dismiss, ordering, and existing team-invite behavior.
- [x] 3.3 Add notification-bell quick Accept and Decline actions for roster selections, with pending
      action state, accessible labels, authoritative refresh, and navigation to the tournament detail
      when the item itself is selected.
- [x] 3.4 Refresh notifications after roster realtime invalidation and reconnect, while ensuring a
      disconnected client can discover pending selections on the next authenticated load.
- [x] 3.5 Keep roster notification data privacy-safe: render only fields supplied by the dedicated
      authenticated endpoint and never expose pending selections in public participant surfaces.

## 4. Localization, parity, and validation

- [x] 4.1 Add en-US and nl-BE strings for selected-state copy, Accept/Decline labels, notification
      titles/messages, loading and error feedback, and accessible names without raw placeholders or
      backend reason codes.
- [x] 4.2 Extend mock services/data to exercise pending selection, Accept, Decline, reload discovery,
      and notification quick actions with the same state transitions as live mode.
- [x] 4.3 Add focused contract/service regression coverage for pagination, stable notification merge,
      action routing, privacy boundaries, refresh-after-action, and localization placeholders.
- [x] 4.4 Build and run the focused front-end contract tests, then strictly validate this OpenSpec
      change again and complete the checklist after implementation and review.

## Validation evidence

- Strict pre-implementation validation passed: `openspec validate roster-confirmation-notification-ux --strict`.
- Front-end Debug contract suite passed: 216/216.
- Front-end default Release contract suite passed: 188/188.
- Focused mock parity suite passed: 17/17.
- Independent Release verification passed: 185 tests, including the production publish-artifact
  guard.
- The combined reviewer passed the batch. Browser rendering/manual verification was unavailable and
  is documented as such; no browser result is claimed here.
