## Why

When the live API is slow or unavailable, transport failures currently delay rendering and can escape component lifecycle methods, making otherwise usable navigation and already available content inaccessible. The application needs bounded, isolated failure handling so one unavailable data source does not take down the interactive shell or unrelated page sections.

## What Changes

- Remove automatic retry delays from live read requests and apply a bounded API timeout.
- Load independent page sections independently so successful content remains visible when another request fails.
- Convert transport failures in public and authenticated page lifecycle work into recoverable, user-friendly unavailable states.
- Ensure navigation profile enrichment and team realtime startup cannot prevent initial browsing.
- Preserve mock backend behavior and existing API contracts.

## Capabilities

### New Capabilities

- `api-unavailable-resilience`: Defines bounded live API requests, isolated page-section failures, and non-fatal navigation/profile/team behavior when the API or realtime endpoint is unavailable.

### Modified Capabilities

- `games-overview-browse-experience`: Tournament browsing must remain navigable and show a recoverable unavailable state when its API request fails.
- `home-sponsor-showcase`: Independent home content sections must preserve successful content when another API-backed section is unavailable.
- `user-owned-team-management`: Authenticated team management must surface API/realtime unavailability without terminating the interactive circuit or blocking navigation.
- `site-navigation`: Optional authenticated profile enrichment must not block or break the shared navigation shell.

## Impact

- Live Refit client registration and timeout/retry policy.
- Home, tournaments overview, profile, team management, profile redirect, and shared navigation component lifecycle handling.
- Focused tests for partial failures, transport failures, and logout/session behavior.
- No backend contract, package, or database change.
