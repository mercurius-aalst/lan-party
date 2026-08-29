## OpenSpec-first implementation

- [x] Add lifecycle DTOs, client methods, service wrappers, and mock state parity.
- [x] Replace the match details admin-only editor with state-aware participant/captain/admin actions.
- [x] Gate participant and administrator mutations on a fresh authoritative action projection and require confirmations for all destructive actions.
- [x] Render confirmation, score, correction, forfeit, resolution, reversal, loading, empty, unauthorized, not-found, and error states.
- [x] Keep anonymous bracket/match projections privacy-safe and refresh after every mutation.
- [x] Keep responsive styling aligned with the existing match dialog brand.

## Tests and validation

- [x] Add DTO/mock transition and lifecycle state regression coverage.
- [x] Cover public fallback, private dispute reports, explicit admin capabilities, and downstream reversal parity in mock/contract tests.
- [ ] Add client state coverage for action gating, deadlines, stale refresh, and error presentation. Limitation: the existing contract-test project has no component test harness; the component guards are validated by compilation and the DTO/mock transition tests above.
- [x] Run OpenSpec validation, restore, build, and focused/full tests; rebuild Tailwind only if the source entry changes.
