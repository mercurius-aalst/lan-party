## Context

The Blazor front-end currently has user-facing English copy embedded in Razor
markup and component code. It already uses an inline browser bootstrap for
theme persistence and has a scoped-service registration point, but it has no
language state, resource loading, or locale-aware display helper. The change
must cover both prerendered layout content and interactive components without
changing API contracts, authentication, authorization, mock behavior, or
server-side JSON serialization.

## Goals / Non-Goals

**Goals:**

- Provide English (`en-US`) and Dutch (`nl-BE`) resources with English as the
  fallback and default.
- Centralize translation lookup, formatted messages, locale-aware date/time
  rendering, and missing-key diagnostics in a scoped front-end service.
- Persist the selected language in browser-local storage and synchronize the
  document language for accessibility.
- Translate application-owned shell, page, shared, form, validation, auth,
  and admin copy while preserving values supplied by the API or exceptions.

**Non-Goals:**

- No backend localization, API message translation, URL path localization, or
  authentication/security behavior changes.
- No new NuGet or npm dependency; JSON parsing and browser storage use the
  existing .NET and Blazor infrastructure.

## Decisions

### Use flat JSON resources loaded by a scoped service

Resources live in `wwwroot/locales/translations.en-US.json` and
`translations.nl-BE.json`, keyed by stable dot-separated identifiers. A scoped
`ILocalizationService` loads the English resource and the selected resource,
returns English for missing localized entries, and records a development
diagnostic for missing keys. Flat keys keep call sites explicit and avoid
coupling translation files to Razor component structure.

An ASP.NET resource satellite assembly was considered, but JSON is required by
the issue’s suggested approach and keeps resource content editable without
introducing package or build tooling changes.

### Keep language state in the browser and reload the current route on change

The existing `App.razor` bootstrap gains `lanLocalization` helpers for
`localStorage`, a small cookie used to make the selected language available
during server prerender, and the document `lang` attribute. The footer switcher
stores a validated language code and reloads the current route so all pages and
components render from the new resource in one consistent lifecycle.

An in-memory-only state event was considered, but it would require every
component with a custom disposal path to subscribe correctly and would leave
prerendered content in the previous language. A route reload is simple,
deterministic, and preserves the existing interactive server model.

### Translate application-owned copy at the component boundary

Razor components use the injected service for labels, headings, empty/loading
states, validation text, dialog actions, and accessibility labels. Dynamic
values such as tournament names, usernames, sponsor descriptions, and backend
responses remain data values. Known front-end-owned status/enum labels use
stable keys; raw API error and exception message accessors stay unchanged.

### Use explicit locale helpers for dates and times

The service exposes date/time formatting methods backed by the selected
`CultureInfo`. Existing invariant formatting remains in request/response
serialization and machine-readable URLs; only user-facing date/time output is
changed.

## Risks / Trade-offs

- [Some user-facing copy may be missed during the broad component migration]
  → Keep all UI-owned text behind resource keys, use development missing-key
  diagnostics, and scan Razor/C# sources for remaining English literals before
  validation.
- [Browser storage or cookies may be unavailable] → Fall back to English and
  continue rendering; language selection remains usable for the current
  circuit until the next reload.
- [A resource key may be absent from Dutch JSON] → Fall back to English and
  log the missing key rather than displaying a blank label or translating API
  data.

## Migration Plan

1. Add resources, service registration, and browser bootstrap helpers.
2. Replace application-owned copy and display date formatting across shell,
   shared components, public flows, authenticated flows, and admin forms.
3. Build the Blazor project and run focused source/resource validation.
4. Rollback is deleting the localization service/resource changes and restoring
   the original literals; no data migration or backend deployment step exists.

## Open Questions

- URL segment localization is intentionally omitted because it is optional and
  would change route/link contracts without improving the core requirement.
