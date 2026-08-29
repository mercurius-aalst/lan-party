## OpenSpec-first implementation

- [x] Add lifecycle DTOs, client methods, service wrappers, and mock state parity.
- [x] Replace the match details admin-only editor with state-aware participant/captain/admin actions.
- [x] Render confirmation, score, correction, forfeit, resolution, reversal, loading, empty, unauthorized, not-found, and error states.
- [x] Keep anonymous bracket/match projections privacy-safe and refresh after every mutation.
- [x] Keep responsive styling aligned with the existing match dialog brand.

## Tests and validation

- [x] Add DTO/mock transition and lifecycle state regression coverage.
- [x] Add client state coverage for action gating, deadlines, stale refresh, and error presentation within the existing contract test stack.
- [x] Run OpenSpec validation, restore, build, and focused/full tests; rebuild Tailwind only if the source entry changes.
