# Add English and Dutch Localization

## Why

The front-end currently presents its interface in one language, which limits
the experience for Dutch-speaking LAN visitors and leaves no consistent place
to manage language-sensitive display formatting. Issue #50 adds an explicit
English default and a user-controlled Dutch option while keeping server/API
messages intact.

## What Changes

- Add English (`en-US`) and Dutch (`nl-BE`) front-end translations with English
  as the default and a centralized translation lookup.
- Add a footer language switcher that persists the selected language locally
  and applies it across navigation, pages, shared components, forms, auth
  surfaces, and admin UI.
- Format user-facing dates and times with the selected locale where the UI
  currently formats them.
- Keep API-provided messages and exception/error payloads displayed one-to-one;
  only stable, front-end-owned messages are translated.
- Keep live API, mock backend, authentication, and authorization behavior
  unchanged.

## Capabilities

### New Capabilities

- `localization`: Defines the supported UI languages, persistence, translated
  interface copy, missing-key behavior, and locale-aware display formatting.

### Modified Capabilities

- None.

## Impact

- Front-end localization state, JSON resources, dependency registration, and
  browser storage integration.
- Razor layouts, navigation, pages, dialogs, validation messages, shared
  components, and formatted date/time output.
- No backend endpoints, DTOs, API contracts, authentication rules, or new
  packages.
