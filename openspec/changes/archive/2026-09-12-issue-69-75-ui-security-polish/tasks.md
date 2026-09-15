## 1. Info ticket hierarchy (#69)

- [x] 1.1 Update the Info ticket presentation to give each ticket kind a clear
      semantic accent and hierarchy while preserving the existing names,
      descriptions, prices, order, and accessible text.
- [x] 1.2 Verify the ticket treatment remains readable in light and dark themes
      and stays inside its container at supported mobile and desktop widths.

## 2. Sponsors page priority and CTA placement (#70)

- [x] 2.1 Reduce the Sponsors page intro/hero footprint so current sponsors
      enter the viewport earlier and read as the primary content.
- [x] 2.2 Remove only the redundant `Why sponsors matter` description, keep the
      useful value content, and place the existing become-a-sponsor CTA in that
      section with a coherent mobile layout.
- [x] 2.3 Preserve SponsorService loading, tier order, sponsor links, empty and
      error states, and the existing sponsorship destination.

## 3. Adaptive sponsor carousel (#71)

- [x] 3.1 Update the shared SponsorScroller to render bounded, centered static
      tiles when all available sponsors fit the active responsive capacity,
      including an intentional single-sponsor state.
- [x] 3.2 Keep up to four compact tiles across where the viewport allows and
      enable marquee movement only when the sponsor set exceeds that capacity.
- [x] 3.3 Preserve sponsor links, accessible labels, keyboard focus behavior,
      hover pause, and reduced-motion static behavior for every consumer.

## 4. Theme-independent sponsor logo contrast (#72)

- [x] 4.1 Apply the existing semantic surface/token system to sponsor cards and
      carousel logo frames so light and dark transparent logos remain visible.
- [x] 4.2 Confirm the supplied logo artwork, colors, transparency, alt text, and
      responsive sizing are preserved without blanket recoloring or inversion.

## 5. Tournaments process hierarchy (#73)

- [x] 5.1 Render the existing localized description for each `How it works`
      step and style heading/description spacing and typography for scanning.
- [x] 5.2 Verify the process section remains readable in both themes and at
      supported responsive breakpoints without changing browse behavior.

## 6. Home proportions (#74)

- [x] 6.1 Tune the featured tournament grid and surface width so the lead
      presentation is narrower and less boxed while the adjacent tournament
      list remains usable.
- [x] 6.2 Give Contact more horizontal allocation than Join the LAN at
      desktop/tablet widths and keep the stacked narrow layout balanced.
- [x] 6.3 Preserve Home section order, anchors, actions, loading/empty/error
      branches, image resolution, and page-wide overflow behavior.

## 7. Production mock mode safety (#75)

- [x] 7.1 Add the explicit MSBuild `IncludeMockBackend` boundary with a default
      of true for Debug and false for Release/non-Debug builds; allow a
      deliberate `-p:IncludeMockBackend=true` override for local development or
      test variants.
- [x] 7.2 With the switch false, exclude mock implementation types, mock
      authentication/persona routes, mock DI/service registrations, mock-only
      UI branches, `MockData.Local`, and `appsettings.Local.json` from normal
      production/Release artifacts wherever the existing build supports it.
- [x] 7.3 Retain a minimal non-mock startup/configuration guard that fails closed
      when `MockBackend:Enabled=true`; client state, query parameters, local
      storage, cookies, and other browser-controlled values MUST not enable
      mock mode in production.
- [x] 7.4 Preserve explicit development/test mock login, personas, services,
      fixtures, and parity behavior, including the deliberate build opt-in.
- [x] 7.5 Add focused automated coverage that builds/publishes the default
      production variant, proves mock types/routes/registrations/UI and local
      assets are absent, exercises the non-mock startup guard, and verifies the
      opted-in Debug/dev-test variant remains functional. Debug (202 tests),
      default Release (177 tests, including the nine-case publish artifact
      guard), and explicit Release opt-in (202 tests) all passed; the release
      CI lane covers the default production variant.

## 8. Validation and handoff

- [x] 8.1 Run `openspec validate issue-69-75-ui-security-polish --strict`
      before source implementation and record the result. It passed with exit
      code 0 before implementation.
- [x] 8.2 Run focused contract tests, the web project build, `git diff --check`,
      and responsive/theme checks appropriate to the changed surfaces. Builds
      and diff checks passed; the UI review verified final CSS geometry and
      shared tile fit at 340, 500, and 800 pixels, including corrected focus
      and hover-pause behavior. Browser rendering was unavailable because
      uncached Playwright and WSL were denied and no CUA surface was available;
      no screenshot or browser-rendering success is claimed.
- [x] 8.3 Re-run strict OpenSpec validation after implementation and synchronize
      this checklist before the change is archived. The final pre-archive
      validation passed with exit code 0.
