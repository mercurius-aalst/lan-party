# AGENTS.md

## Scope
These instructions apply to the whole front-end repository.

## Project
Mercurius LAN party front-end. The active integration target is the redesigned Blazor front-end merged from PR #31. The app source lives in `src/Mercurius.LAN.Web`.

## Stack
- ASP.NET Core Blazor / Razor Components with interactive server rendering.
- Target framework: `net9.0`.
- Auth: Auth0 + cookie authentication, with mock-login support when mock backend mode is enabled.
- API client layer: Refit contracts in `APIClients/`, service wrappers in `Services/`, DTOs in `DTOs/`.
- Styling: Tailwind entry at `Styles/tailwind.css`, generated output at `wwwroot/tailwind.css`, plus component-scoped CSS.
- Local mock backend support: `Mock/` and `MockData.Local/backend.json`.

## OpenSpec requirements
This repository uses OpenSpec. Any functional behavior change must be accompanied by an OpenSpec change unless the task is explicitly limited to investigation, refactoring with no behavior change, documentation, formatting, or mechanical integration plumbing.

Before implementing a functional change:
1. Inspect existing specs in `openspec/specs/`.
2. If no suitable active change exists, create a new OpenSpec change under `openspec/changes/<change-id>/`.
3. Include at minimum:
   - `proposal.md`
   - `tasks.md`
   - spec deltas under `openspec/changes/<change-id>/specs/<capability>/spec.md`
4. Use RFC 2119 language in specs: MUST, SHOULD, MAY.
5. Keep implementation aligned with the OpenSpec tasks.
6. When completing work, update the OpenSpec task checklist.

Integration-analysis-only work does not need a new OpenSpec change. Actual functional changes discovered during integration do need one.

## Common commands
Run commands from the repository root unless noted.

```bash
dotnet restore src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj
dotnet build src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj
cd src/Mercurius.LAN.Web && npm install && npm run build:tailwind
dotnet run --project src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj
```

## Important files and directories
- `src/Mercurius.LAN.Web/Program.cs`: app composition, auth setup, mock backend routing, Razor component mapping.
- `src/Mercurius.LAN.Web/APIClients/ILANClient.cs`: backend API contract surface used by the front-end.
- `src/Mercurius.LAN.Web/Services/`: feature-facing service wrappers around the API client and mock services.
- `src/Mercurius.LAN.Web/DTOs/`: front-end request/response DTOs; keep these aligned with the backend.
- `src/Mercurius.LAN.Web/Components/Pages/`: page-level UI and route behavior.
- `src/Mercurius.LAN.Web/Options/`: configuration option classes.
- `openspec/`: requirements/specification source. Functional behavior changes require spec coverage.

## Code design constraints
- Keep implementations straightforward and avoid unnecessary abstractions, wrapper methods, wrapper classes, and indirection that do not add clear readability, testability, performance, or security value.
- Avoid code duplication, but also avoid unnecessary de-duplication that creates brittle shared abstractions or hides feature-specific behavior.
- Reuse existing components, services, DTOs, options, and styling patterns where possible, but verify behavior to avoid regression failures.
- Keep one primary class, record, component model, or DTO per file unless the additional type is a small private nested implementation detail.
- Apply industry-standard design patterns only where they are appropriate and necessary for code cleanliness, performance, security, or long-term maintainability.
- Avoid N+1 API calls, repeated render-triggering work, redundant service invocations, and other performance bottlenecks caused by method or component call patterns.
- Keep dependencies between feature domains minimal. Prefer explicit contracts at boundaries rather than cross-domain coupling or shared mutable state.

## Integration rules
- Do not assume backend endpoints exist. Verify against `mercurius-aalst/mercurius-aalst-back-end` before wiring live calls.
- Treat `ILANClient` as the front-end API contract boundary. Keep request/response names and JSON shapes explicit.
- Preserve mock backend behavior while adding live API integration. Mock data should stay useful for local UI development.
- Pay special attention to public/privacy-safe data: users, teams, search results, sponsors, participants, and match details.
- Keep loading, empty, error, unauthorized, and not-found states visible in UI changes.
- Do not introduce new packages unless the existing stack cannot reasonably solve the task.
- Prefer small, reviewable changes by feature area.
- If a required integration change alters user-visible behavior, validation rules, auth behavior, API expectations, privacy behavior, or error handling, add or update OpenSpec coverage first.

## Before completing work
- Build the Blazor project.
- Rebuild Tailwind output when CSS utility classes or `Styles/tailwind.css` change.
- Verify the relevant OpenSpec change exists for functional behavior changes.
- Update the relevant OpenSpec task checklist.
- Manually note any backend contract gaps, config gaps, or assumptions that could not be verified.
