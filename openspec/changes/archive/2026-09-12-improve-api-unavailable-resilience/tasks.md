## 1. Live client resilience

- [x] 1.1 Remove the automatic transient retry policy and configure a bounded timeout consistently for live Refit clients.

## 2. Public page partial availability

- [x] 2.1 Load home tournaments and sponsors independently, preserving successful content and section-specific friendly unavailable states.
- [x] 2.2 Load tournament overview data independently so tournament failure does not hide sponsors or break navigation.

## 3. Authenticated and shared shell resilience

- [x] 3.1 Contain expected API transport failures in Profile, ProfileRedirect, and Manage Teams lifecycle work with recoverable UI state and no stale private data; defer ProfileRedirect API checks until interactive rendering and guard route/disposal races.
- [x] 3.2 Make shared navigation profile/notification enrichment and team realtime startup non-blocking and failure-tolerant, with cancellation and disposal guards preventing post-teardown realtime work or renders.

## 4. Validation

- [x] 4.1 Add or update focused behavioral coverage for request policy, independent section completion with pending tasks, disposal races, deferred profile checks, and local-cookie clearing before a simulated remote logout failure.
- [x] 4.2 Run strict OpenSpec validation, focused tests, project build, and diff checks; document any manual or live-backend validation limitations. `openspec validate improve-api-unavailable-resilience`, the focused lifecycle tests, the full contract suite (192 tests), `dotnet build src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj --no-restore`, and `git diff --check` passed. Browser, live API outage, and live Auth0 smoke validation were not run; logout ordering is covered with a local HTTP context and failing remote-auth handler.
