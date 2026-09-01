# Design

The match dialog owns a small lifecycle view model over the shared match DTO. It renders a state badge, ended-confirmation progress, server countdowns, and participant-specific actions. Actions are disabled while loading or mutating and are followed by a fresh match read plus the existing bracket reload callback. Unauthorized, not-found, expired, and invalid-state responses are presented as actionable messages.

The API client exposes explicit lifecycle methods (`confirm-ended`, `score`, `forfeit`, `resolve`, and `reverse`). The mock store mirrors these methods and uses the same transition rules, including consensus, correction deadlines, and downstream reversal guards. The existing admin score method remains available as a compatibility path for existing callers.

Responsive styling reuses the match dialog's green/navy brand variables and allows action rows to wrap on narrow screens.
