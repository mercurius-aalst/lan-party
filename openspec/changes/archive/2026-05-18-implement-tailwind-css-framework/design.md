## Context

`src/Mercurius.LAN.Web` is a Blazor Web App that currently loads three styling layers in [`src/Mercurius.LAN.Web/Components/App.razor`](D:/Github%20Repositories/lan-party/src/Mercurius.LAN.Web/Components/App.razor:11): Bootstrap CSS from `wwwroot/lib/bootstrap`, the shared stylesheet `wwwroot/app.css`, and the generated CSS-isolation bundle `Mercurius.LAN.Web.styles.css`. The shared stylesheet already defines the site's design tokens, typography, buttons, cards, form controls, and some responsive behavior, while component-scoped `.razor.css` files provide page-specific layout and interaction styling.

The markup shows broad use of Bootstrap-oriented classes such as `btn`, `form-control`, `badge`, and related variants across admin, profile, game, and dialog screens. That means a direct framework swap would create unnecessary visual and behavioral risk. This change needs a migration design that introduces Tailwind as the structured styling foundation, preserves the current brand identity, and permits targeted mobile improvements without forcing a redesign of every screen in one pass.

## Goals / Non-Goals

**Goals:**
- Introduce Tailwind CSS into the frontend build so Razor components can use utility classes consistently.
- Preserve the current visual identity by keeping existing design tokens, colors, typography, radii, and major component styling semantics.
- Allow responsive improvements on layouts that currently degrade on smaller screens, especially navigation, forms, dialogs, and card grids.
- Define a migration path that lets Tailwind coexist with existing Bootstrap and custom CSS while the codebase transitions.

**Non-Goals:**
- Removing every Bootstrap class or every `.razor.css` file in this change.
- Rebranding the application or redesigning existing desktop layouts from scratch.
- Replacing Bootstrap Icons or unrelated JavaScript behavior.
- Requiring a frontend framework rewrite or changing the Blazor rendering model.

## Decisions

### 1. Add Tailwind as a repo-local build dependency for the web project

Tailwind will be added as a local frontend build dependency scoped to `src/Mercurius.LAN.Web`, with its own configuration and input stylesheet. The generated output should be a dedicated static asset under `wwwroot`, rather than replacing `app.css`.

Rationale:
- The project currently has no frontend package pipeline, so the Tailwind toolchain needs an explicit home.
- A dedicated generated asset keeps the migration incremental and avoids mixing generated output with hand-authored CSS.
- A version-pinned local toolchain is more reproducible than a CDN or machine-specific global install.

Alternatives considered:
- Use a CDN build of Tailwind. Rejected because it is not suitable for controlled production styling and makes purge/content scanning unreliable.
- Use a globally installed Tailwind binary. Rejected because it creates setup drift between contributors and CI.
- Replace `app.css` entirely with Tailwind-generated CSS. Rejected because the current shared stylesheet still contains important brand and compatibility rules.

### 2. Keep Bootstrap and existing CSS during the first migration phase

Bootstrap CSS will remain in place during this change, and existing `app.css` plus component-scoped `.razor.css` files will continue to style legacy or partially migrated surfaces.

Rationale:
- Current Razor markup uses Bootstrap-style classes extensively for forms, buttons, spacing, and badges.
- Keeping Bootstrap avoids a forced one-shot rewrite and reduces regression risk while Tailwind adoption begins.
- This preserves user-visible consistency while enabling targeted layout improvements.

Alternatives considered:
- Remove Bootstrap as part of the Tailwind introduction. Rejected because the class footprint is too broad for a low-risk migration.
- Freeze all existing CSS and use Tailwind only for new pages. Rejected because the stated goal includes improving structure across the existing application and addressing mobile pain points now.

### 3. Disable Tailwind's base reset to prevent unintentional visual drift

Tailwind should be configured without its default global reset behavior, and the generated Tailwind stylesheet should focus on utility generation rather than redefining element defaults.

Rationale:
- The application already relies on global element styling in `app.css` for typography, links, focus states, and page background.
- Tailwind's reset layer would likely alter those defaults even before any utility classes are added.
- Disabling the reset makes Tailwind additive, which matches the migration goal of preserving the current visual identity.

Alternatives considered:
- Enable Tailwind's full base layer and restyle the entire application to match. Rejected because it would create broad visual change outside the approved scope.
- Try to override reset-related regressions after the fact. Rejected because it creates unnecessary churn and makes regressions harder to reason about.

### 4. Load the generated Tailwind stylesheet after existing shared styles

The new generated Tailwind asset should be linked after Bootstrap and `app.css`, so explicit utility classes can be used for responsive and structural adjustments without rewriting the entire shared stylesheet.

Rationale:
- With the reset disabled, loading Tailwind later does not introduce broad element-level restyling.
- Utilities loaded later are easier to use intentionally in Razor markup for breakpoint-specific fixes.
- Existing shared and isolated CSS remains the baseline until a component is deliberately migrated.

Alternatives considered:
- Load Tailwind before `app.css` so existing custom CSS always wins. Rejected because it weakens Tailwind's usefulness for targeted responsive overrides.
- Merge Tailwind output into the isolated CSS bundle. Rejected because CSS isolation is meant for component-scoped authored styles, not a generated utility framework.

### 5. Preserve existing brand tokens and map Tailwind usage onto them

The existing CSS custom properties in `wwwroot/app.css` remain the source of truth for colors, spacing primitives that are already encoded there, shadows, and radii. Tailwind usage should align with those values rather than introducing a second visual language.

Rationale:
- The site already has a defined visual system in `:root`.
- Preserving those tokens keeps the current look intact while making utility-based styling more structured.
- This allows component markup to express layout and responsive behavior with Tailwind while shared visual identity continues to come from established tokens.

Alternatives considered:
- Replace the current variable system with a new Tailwind-first theme. Rejected because it increases redesign risk and migration scope.
- Ignore the existing tokens and use Tailwind defaults. Rejected because it would change the site's visual identity unnecessarily.

### 6. Use Tailwind first for structure, layout, and mobile responsiveness

The first application of Tailwind should focus on cross-cutting structural concerns: container widths, stacking behavior, spacing, breakpoint handling, navigation layout, form action layout, dialog sizing, and grid/card responsiveness. Detailed component cosmetics can remain in existing CSS until they need migration.

Rationale:
- The user goal is more structured CSS, not a purely cosmetic rewrite.
- Layout and breakpoint logic benefit most from utility classes and are the highest-value place to improve mobile experience.
- This lets the team gain consistency quickly while preserving current component visuals.

Alternatives considered:
- Rewrite all component appearance rules into utilities immediately. Rejected because it is too broad for one change.
- Restrict Tailwind to cosmetic tweaks only. Rejected because it would not solve the structural styling problem that motivated the change.

## Risks / Trade-offs

- [Dual-framework complexity] Tailwind, Bootstrap, shared CSS, and isolated CSS will coexist for a period. -> Mitigation: treat Tailwind as the preferred choice for new structural styling, forbid new Bootstrap utility usage where Tailwind is available, and document the migration path.
- [Unexpected specificity conflicts] Existing selectors in `app.css` and `.razor.css` may override some utilities or vice versa. -> Mitigation: keep Tailwind reset disabled, load the Tailwind asset deliberately, and adjust only the components being migrated instead of relying on accidental precedence.
- [Mobile fixes expanding into redesign] Responsive work can drift into visual rework if not constrained. -> Mitigation: target mobile issues that improve readability, spacing, stacking, and touch usability while keeping existing desktop composition and brand tokens intact.
- [Build/tooling friction] Introducing Tailwind adds a frontend dependency path to a project that is currently mostly .NET-centric. -> Mitigation: keep the tooling local to the web project, provide simple scripts or documented commands, and avoid coupling unrelated backend workflows to frontend styling work.
- [Incomplete content scanning] Tailwind may omit utilities used in Razor files or dynamically assembled classes if configuration is too narrow. -> Mitigation: include `.razor`, `.cshtml`, and relevant C# sources in the content scan, and safelist any known dynamic class patterns if needed.

## Migration Plan

1. Add Tailwind configuration, input CSS, and a generated output file under `src/Mercurius.LAN.Web`.
2. Wire the generated stylesheet into `Components/App.razor` without removing Bootstrap or existing shared styles.
3. Preserve the current global token system in `wwwroot/app.css`, and make Tailwind utilities additive rather than reset-driven.
4. Migrate shared structural patterns first: shell/layout containers, navigation responsiveness, form action layouts, modal/dialog sizing, and card/grid breakpoints.
5. Keep existing Bootstrap-backed component styles working while replacing targeted layout concerns with Tailwind utilities.
6. Validate key mobile and desktop screens for parity, then defer Bootstrap removal and deeper CSS cleanup to a follow-up change once dependency coverage is low enough.

Rollback strategy:
- Remove the Tailwind asset reference from `App.razor`.
- Remove the Tailwind configuration and generated asset.
- Leave Bootstrap, `app.css`, and isolated CSS as the active styling system.

## Open Questions

- Should the generated Tailwind CSS be checked into source control as a committed asset, or regenerated as part of local/CI build steps once the implementation details are finalized?
- Which screens currently have the highest-priority mobile issues and should therefore be used as the first migration targets during implementation?
