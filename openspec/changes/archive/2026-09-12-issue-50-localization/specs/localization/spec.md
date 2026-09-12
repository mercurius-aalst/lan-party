## ADDED Requirements

### Requirement: The front-end supports English and Dutch with an English default
The front-end MUST support `en-US` and `nl-BE` user-interface languages, and MUST
use `en-US` when no valid language preference has been selected.

#### Scenario: First-time visitor sees English
- **WHEN** a visitor has no stored language preference
- **THEN** the application renders its user-facing interface in English
- **AND** the document language is `en-US`

#### Scenario: Visitor selects Dutch
- **WHEN** a visitor selects Dutch in the language control
- **THEN** the application renders application-owned user-facing copy in Dutch
- **AND** the document language is `nl-BE`

#### Scenario: Invalid stored language is ignored
- **WHEN** browser storage contains a value other than `en-US` or `nl-BE`
- **THEN** the application falls back to English
- **AND** it does not attempt to load an untrusted resource path

### Requirement: Language selection is available in the footer and persists locally
The application MUST provide a language switcher in the footer, and MUST persist
the selected language in browser-local storage so it survives navigation and a
page refresh.

#### Scenario: Visitor changes language from the footer
- **WHEN** a visitor chooses a supported language in the footer switcher
- **THEN** the selected language is applied to the current application route
- **AND** the selected value is stored locally

#### Scenario: Visitor refreshes after selecting Dutch
- **WHEN** a visitor refreshes the application after selecting Dutch
- **THEN** Dutch remains selected and the UI renders from the Dutch resources

### Requirement: Application-owned copy is translated without translating API messages
The front-end MUST translate application-owned shell, page, shared-component,
form, validation, authentication, and administration copy. It MUST preserve API
responses, backend error messages, exception messages, and other explicitly
data-provided content exactly as received when displaying them one-to-one.

#### Scenario: UI-owned status is localized
- **WHEN** a page displays a front-end-owned loading, empty, validation, or
  action status
- **THEN** the status is selected from the active language resource

#### Scenario: API message is displayed
- **WHEN** an API call returns a message that the front end displays directly
- **THEN** the displayed message remains the returned message without a
  localization lookup or mutation

#### Scenario: Exception message is displayed
- **WHEN** an exception message is surfaced through an existing error helper
- **THEN** the helper output remains unchanged by the localization service

### Requirement: User-facing date and time output follows the selected locale
The front-end MUST use the active language culture for user-facing date and
time output where the existing UI formats a date or time, while preserving
invariant machine-readable serialization for API requests and responses.

#### Scenario: Tournament schedule is rendered
- **WHEN** a tournament start or end value is shown to a visitor
- **THEN** the value uses the active language’s practical date/time format

#### Scenario: API serialization occurs
- **WHEN** a date/time is serialized for an API request or parsed from an API
  response
- **THEN** the existing invariant wire format remains unchanged

### Requirement: Missing translation keys remain visible during development
The localization service MUST fall back safely when a resource key is missing
and MUST emit a development diagnostic that identifies the missing key.

#### Scenario: Dutch key is missing
- **WHEN** the active language is Dutch and a requested key is absent from the
  Dutch resource
- **THEN** the English value is used when available
- **AND** a development diagnostic identifies the key

#### Scenario: Unknown key is requested
- **WHEN** a requested key is absent from both resources
- **THEN** the service returns a visible key fallback rather than an empty value
- **AND** a development diagnostic identifies the key
