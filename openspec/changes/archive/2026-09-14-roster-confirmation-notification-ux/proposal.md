## Why

Selected roster members currently have to discover a pending confirmation inside the captain's
registration workflow. The page does not offer a decline action, and roster selection events do not
produce a reload-safe notification, so a player can miss an invitation entirely.

## What changes

- Make a pending roster selection the primary player-facing state in the registration surface, with
  plain-language tournament and team context and direct Accept and Decline actions.
- Keep the existing confirmation mutation for Accept and add the authenticated member-only decline
  mutation exposed by the backend.
- Extend the notification service and account bell to consume the paginated pending-roster
  confirmation endpoint, merge those items with team invitations, and provide quick actions for
  either type.
- Refresh actionable notification and registration state after each action and after supported
  realtime invalidation, while retaining the server as the source of truth after reloads.
- Localize the new state, action, and feedback copy in en-US and nl-BE and preserve mock/live parity.

## Backend contract consumed by the front end

The front end consumes these authenticated routes from the companion backend change:

| Purpose | Route and result |
| --- | --- |
| Read pending roster selections | `GET /v1/lan/tournament-roster-confirmations/me?page&pageSize`, returning `{ items, totalCount, page, pageSize }`; pages default to 1/20 and are capped at 50. Each item contains `rosterMemberId`, `tournamentId`, `tournamentName`, `teamId`, `teamName`, optional `teamLogoUrl`, and `selectedAtUtc`. |
| Accept a selection | Existing `PATCH /v1/lan/tournaments/{tournamentId}/registrations/roster-members/{rosterMemberId}` with `confirmationStatus: Confirmed`; returns the authoritative registration DTO. |
| Decline a selection | `DELETE /v1/lan/tournaments/{tournamentId}/registrations/roster-members/{rosterMemberId}`; an owned pending selection is removed and the pending team registration remains for the captain and other members. The route returns `204` for a valid, missing, repeated, or non-owned selection. |

The front end MUST treat the pending-roster list as a server-derived actionable projection. It MUST
not persist a private roster invitation in browser state or infer one from a realtime event alone.

## Non-goals

- Change public tournament participant projections or expose pending roster data to anonymous users.
- Merge roster confirmations into team-membership invites or change existing team-invite semantics.
- Let a player edit another member's roster, confirm a selection they do not own, or bypass the
  backend's scheduled-tournament and eligibility checks.
- Add a notification database to the front end; the pending-roster endpoint remains the source of
  truth.
