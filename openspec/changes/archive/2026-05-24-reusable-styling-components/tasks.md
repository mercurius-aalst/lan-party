## 1. Shared Styling Foundation

- [x] 1.1 Add reusable shared surface classes and documented override hooks in `src/Mercurius.LAN.Web/wwwroot/app.css` for branded section shells, sponsor cards, tournament cards, and non-homepage hero surfaces.
- [x] 1.2 Extend the shared button styling layer in `src/Mercurius.LAN.Web/wwwroot/app.css` so centralized button treatments cover the currently repeated primary and supporting button states without changing the rendered UI.
- [x] 1.3 Keep the shared surface selectors low-specificity and expose the agreed custom-property override points needed for page-scoped spacing, sizing, alignment, and positioning adjustments.

## 2. Page Adoption

- [x] 2.1 Update the tournaments page markup and scoped CSS to adopt the shared non-homepage hero foundation while preserving the current user-facing hero or lead presentation.
- [x] 2.2 Update the tournaments page card markup and scoped CSS to adopt the shared tournament-card surface family while preserving the current card appearance and behavior.
- [x] 2.3 Update the sponsors page markup and scoped CSS to adopt the shared sponsor-card and branded section-shell foundations while preserving the current card hierarchy and section presentation.
- [x] 2.4 Update any other affected non-home pages that already use the same branded section or non-homepage hero patterns to consume the shared surface classes only where the current UI can remain visually equivalent.
- [x] 2.5 Confirm the home page hero remains on its separate styling path and does not adopt the shared non-homepage hero foundation.

## 3. Cleanup And Verification

- [x] 3.1 Remove duplicated page-local visual rules that are superseded by the new shared styling foundation while keeping page-specific layout overrides in scoped CSS.
- [x] 3.2 Verify the affected pages before and after the refactor to confirm sponsor cards, tournament cards, buttons, section shells, and non-homepage heroes remain visually unchanged to visitors.
- [x] 3.3 Verify responsive behavior and scoped override behavior on the affected pages so shared surfaces remain reusable without layout regressions.
- [x] 3.4 Build and test the web project to confirm the shared styling refactor compiles cleanly and does not break the affected pages.
