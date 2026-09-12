## 1. OpenSpec and localization foundation

- [x] 1.1 Create the issue-50 proposal, design, and localization delta spec with RFC 2119 requirements.
- [x] 1.2 Add the English and Dutch JSON resource files with stable keys for all translated application-owned copy.
  - [x] Merge the completed General EN/NL resource packet (316 matching keys) into the canonical locale files.
  - [x] Merge the Feature EN/NL resource packet and complete final resource parity validation.
  - [x] Merge the final form-validation EN/NL resource packet (5 matching keys).
- [x] 1.3 Add the scoped localization service, validated language model, resource loading, missing-key diagnostics, and locale-aware date/time helpers.
- [x] 1.4 Register localization in dependency injection and add browser storage/document-language bootstrap helpers while preserving theme behavior.

## 2. Shell and shared UI

- [x] 2.1 Add the persistent footer language switcher and translate the main layout, navigation, routing status, auth labels, and accessibility copy.
- [x] 2.2 Translate shared loading, status, participant, search, profile-summary, upload, and dialog components.

## 3. Public and authenticated pages

- [x] 3.1 Translate Home, Info, Sponsors, Privacy Policy, tournament overview/detail/tabs, match views, and locale-aware schedule/score display.
- [x] 3.2 Translate profile, complete-profile, public profile, team management, team dialogs, invitations, and validation/action copy.

## 4. Administration and validation

- [x] 4.1 Translate sponsor/tournament administration forms and their UI-owned validation and action messages without changing auth behavior.
- [x] 4.2 Preserve raw API/exception messages and invariant serialization; scan for remaining application-owned literals and resource gaps.
- [x] 4.3 Build the Blazor project and run focused resource/source checks; document any backend/config assumptions and update this checklist.
  - [x] Blazor build, focused/full tests, resource/source checks, strict OpenSpec validation, and diff validation passed.
  - [x] Browser/app execution was not performed because it was not requested; Tailwind rebuild was not needed because its inputs, configuration, and generated output were unchanged.
