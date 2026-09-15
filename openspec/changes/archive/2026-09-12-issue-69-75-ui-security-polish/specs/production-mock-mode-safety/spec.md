## ADDED Requirements

### Requirement: Normal production artifacts exclude mock implementation

The web project MUST expose an explicit `IncludeMockBackend` build switch. The
switch MUST default to enabled for Debug builds and disabled for Release and
other non-Debug builds. With the switch disabled, normal production/Release
artifacts MUST exclude mock implementation types, mock authentication and
persona routes, mock dependency registrations, mock-only UI, local mock
fixtures, and local mock configuration wherever the existing build can enforce
that boundary. A deliberate local development/test build MAY opt in with
`-p:IncludeMockBackend=true`.

#### Scenario: Default Release output is built for production

- **WHEN** the web project is built or published in Release without an explicit
  `IncludeMockBackend` override
- **THEN** the build MUST use `IncludeMockBackend=false`
- **AND** mock implementation source/types, mock authentication/persona route
  branches, mock service registrations, and mock-only UI branches MUST not be
  present in the compiled production path
- **AND** `MockData.Local` and `appsettings.Local.json` MUST not be included in
  the production/publish output when the existing project packaging supports
  that exclusion

#### Scenario: Local development or test explicitly opts in

- **WHEN** a Debug or approved development/test build explicitly enables
  `IncludeMockBackend`
- **THEN** the existing mock login/persona routes, services, fixture, and
  local UI validation workflow MUST remain buildable and usable
- **AND** the opt-in MUST be deliberate rather than inferred from client state

#### Scenario: Non-mock artifact receives an unsafe mock configuration

- **WHEN** a build with `IncludeMockBackend=false` starts with
  `MockBackend:Enabled=true`
- **THEN** the minimal startup/configuration guard MUST fail closed before the
  application serves requests
- **AND** the application MUST report the invalid production configuration to
  deployment diagnostics without activating a partial mock path

### Requirement: Production runtime fails closed for mock mode

The application MUST treat mock backend mode as an explicitly approved
development/test capability. A production runtime MUST reject an enabled mock
configuration before mock authentication routes, mock services, mock data, or
mock-only UI are available.

#### Scenario: Production configuration enables mock mode

- **WHEN** the application starts in a production environment with
  `MockBackend:Enabled` set to `true`
- **THEN** startup MUST fail closed before the application serves mock routes
  or registers mock services
- **AND** the application MUST not silently fall back to mock authentication or
  mock data
- **AND** the failure MUST be observable to deployment diagnostics without
  exposing mock controls to visitors

#### Scenario: A production client attempts to enable mock mode

- **WHEN** a production visitor changes a query parameter, URL, local-storage
  value, cookie, or other client-controlled state associated with mock mode
- **THEN** the application MUST continue using the production configuration
- **AND** no mock login, persona switch, mock service, or mock data path MUST
  become reachable

#### Scenario: Development or test mock mode is explicitly enabled

- **WHEN** the application runs in an approved development/test environment and
  mock mode is explicitly enabled
- **THEN** the existing mock login/persona, fixture, service, and local UI
  validation workflows MUST remain available
- **AND** existing mock/live service parity and authentication boundaries MUST
  remain unchanged

#### Scenario: Production packaging is checked

- **WHEN** the production build or publish output is validated
- **THEN** an automated guard MUST prove that mock mode cannot be enabled by
  production configuration or client state
- **AND** mock-only served assets SHOULD be excluded from production output
  where the existing build supports that separation
