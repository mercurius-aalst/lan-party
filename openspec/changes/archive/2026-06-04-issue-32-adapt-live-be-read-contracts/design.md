## Context

The redesigned front-end currently uses Refit contracts and models that were shaped around local mock data. The live back-end now exposes versioned `v{version}/lan/...` route groups and returns DTOs that differ in two important read paths: global search returns a response wrapper with `results`, `nextCursor`, and `hasMore`, while game detail returns a nullable singular `sponsorPlacement`.

The front-end Refit attributes intentionally begin with `/lan/...`. API versioning is kept centralized by normalizing the configured Mercurius API base address before registering Refit clients.

## Goals / Non-Goals

**Goals:**

- Preserve `/v1` in all live Refit calls to the back-end.
- Represent every live back-end endpoint in the front-end Refit client boundary, even when a UI flow does not yet consume the method.
- Keep request and response DTOs at the Refit boundary explicit and aligned with the live back-end DTO names/shapes where practical.
- Deserialize the live global search response wrapper and expose bounded result records to the navigation search UI.
- Align game detail sponsor data with the live singular `sponsorPlacement` response while preserving the visitor-facing rule that at most one partner is shown.
- Keep mock backend mode useful and close to live read-contract behavior.
- Keep the change small and integration-focused with no new package dependencies.

**Non-Goals:**

- Add infinite scrolling or explicit cursor pagination UI to header search.
- Change the back-end search API or sponsor placement API.
- Redesign navigation search, game detail layout, sponsor management, or tournament administration flows.
- Resolve public user profile field alignment covered by `issue-34-align-public-user-profile-contract`.

## Decisions

### Preserve API versioning while keeping leading-slash Refit paths

Refit route attributes should start with `/lan/...` to match the project's client convention. The configured Mercurius API base address should be normalized with a `/v1` suffix before registering the live Refit clients.

Alternative considered: move `/v1` into every Refit attribute. That would work, but it duplicates versioning across every client method and makes future version changes broader.

### Treat Refit clients as the full live API boundary

`ILANClient` and `IUserClient` should expose every live endpoint in the local back-end route groups: games, matches, search, sponsors, teams, public teams, public users, current-user account actions, and admin-user actions. Service wrappers may continue exposing only the workflows currently used by the UI, but the Refit contracts should not omit available live endpoints.

Alternative considered: add only methods used by current pages. That keeps the diff smaller but repeats the integration blind spot that caused the issue examples to be incomplete.

### Keep navigation search service result-oriented

`ILANClient.SearchAsync` should deserialize the live wrapper DTO, while `IGlobalSearchService.SearchAsync` may continue returning the bounded result collection used by `NavMenu`. This keeps pagination metadata available at the contract boundary without forcing the current dropdown to grow pagination behavior.

Alternative considered: expose the wrapper all the way to `NavMenu`. That adds UI state that the issue does not require and increases the blast radius.

### Model game detail sponsor placement as singular

The live game response should be represented with a nullable singular sponsor placement. UI code can derive `FeaturedPartner` directly from that property. Mock mode should also enforce zero-or-one returned placement so local development matches live behavior.

Alternative considered: keep plural `SponsorPlacements` in the front-end model and map live `sponsorPlacement` into a one-item collection. That preserves old call sites but keeps the front-end contract misleading and makes future integration harder to reason about.

### Keep write DTO compatibility for sponsor replacement

The back-end still accepts `ReplaceGameSponsorsDTO` with a `sponsorPlacements` list but validates that no more than one placement is provided. The front-end should keep the write DTO shape unless the back-end contract changes, while ensuring its UI only sends zero or one placement.

## Risks / Trade-offs

- [Risk] Changing Refit attributes and adding missing methods touches API methods outside the read-only acceptance checks. -> Mitigation: make endpoint additions explicit, keep service wrapper behavior stable unless required, build the Blazor project, and smoke the listed read pages.
- [Risk] Search type casing can fail enum deserialization if live values are `user`, `team`, and `game`. -> Mitigation: ensure the front-end DTO can deserialize the live string values without losing type information.
- [Risk] Mock data may still contain multiple sponsor placements. -> Mitigation: clamp mock replacement and fixture projection to zero or one placement for game detail responses.
- [Risk] Header search ignores `hasMore`, so visitors only see the first bounded result page. -> Mitigation: document this as intentional for the compact dropdown; future pagination can extend the service contract if needed.
