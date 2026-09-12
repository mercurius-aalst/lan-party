## Context

Live API calls currently inherit the default `HttpClient` timeout and the primary client retries transient failures once after a delay. Several components await API calls directly during initialization, sometimes through a shared `Task.WhenAll`, so one transport failure can delay or fail the whole page or Blazor circuit. Optional navigation profile/notification enrichment and realtime startup have the same risk even though the shell can function without them.

## Goals / Non-Goals

**Goals:**

- Bound the time spent on an unavailable API and eliminate automatic retry delay.
- Isolate independent page requests and keep successful content visible.
- Catch transport failures at component lifecycle boundaries and render recoverable local states.
- Keep shared navigation usable while optional API and realtime enrichment fails.

**Non-Goals:**

- Change backend endpoints or response contracts.
- Add offline persistence, global caching, background synchronization, or a new resilience package.
- Change authentication or authorization rules.

## Decisions

1. Configure both live Refit clients with the same short timeout and remove the Polly retry policy. A single request gives the application a prompt, deterministic failure signal; callers may offer an explicit retry where the page already supports one. Retaining automatic retries was rejected because it makes a known outage block the user for longer.
2. Keep failure state local to each independently useful section. Home and tournament overview calls will start independently after the initial render, clear their own loading state, and request a render as each section completes rather than using `Task.WhenAll` as a shared rendering barrier. A broad global exception wrapper was rejected because it cannot preserve partial success precisely.
3. Catch expected transport and API exceptions in component lifecycle work before they escape into the interactive circuit. Authenticated pages clear stale private state and show an unavailable message with an explicit retry where practical. Detached work is owned by the component lifecycle with cancellation, version, route, and disposal guards so it cannot start realtime work or render after teardown. Swallowing all exceptions was rejected; cancellation caused by component disposal remains non-error flow while unexpected programmer failures should remain diagnosable.
4. Treat navigation enrichment and realtime connection as optional. The identity-claim fallback renders immediately, while failed API enrichment or hub startup leaves core navigation functional and can be retried by later refresh/reconnect behavior.

## Risks / Trade-offs

- [Risk] A short timeout may fail requests during a temporarily slow response. → Keep it long enough for normal LAN latency and expose clear retry/recovery UI.
- [Risk] Component-specific handling can drift. → Use the existing page error-state patterns and focused tests without introducing a new abstraction.
- [Risk] Realtime failure may delay notification freshness. → Preserve REST-backed refreshes and make the hub non-blocking rather than treating it as authoritative page data.

## Migration Plan

Deploy as a front-end-only change. Roll back the Refit registration and component state changes together if timeout behavior proves too aggressive; no data migration is required.

## Open Questions

None.
