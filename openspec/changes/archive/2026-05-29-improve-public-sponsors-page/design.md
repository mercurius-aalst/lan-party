## Context

The public sponsors page already loads sponsor data through `ISponsorService.GetSponsorsAsync()`, sorts by `SponsorTier.GetDisplayOrder()`, and renders tiers in Presenting, Gold, Silver, and Bronze order. The page also already uses shared brand primitives such as `brand-section`, `brand-section-shell`, `brand-card`, `brand-sponsor-card`, `section-eyebrow`, `button-primary`, and `button-secondary`.

The current presentation is still mostly a functional tier list: the intro is short, sponsor value is not explained, tier sections are visually similar, and the empty state is generic. The redesign should improve the visitor-facing page without expanding the data contract or creating backend/API work.

## Goals / Non-Goals

**Goals:**
- Turn the public sponsors page into a polished partner showcase that fits the existing redesign language.
- Keep all sponsor content sourced from the existing `Sponsor` model fields: `Name`, `SponsorTier`, `LogoUrl`, `InfoUrl`, and `Description`.
- Preserve tier ordering and tier labels from `SponsorExtensions`.
- Give Presenting sponsors the strongest visual emphasis while keeping lower tiers attractive and scannable.
- Add a partner CTA, sponsor-value explanation, responsive tier grids, improved cards, and a helpful empty state.
- Keep implementation limited to `Sponsors.razor`, `Sponsors.razor.cs`, and `Sponsors.razor.css` unless a narrowly scoped shared-style hook is required.

**Non-Goals:**
- Backend endpoint, API client, service, persistence, or sponsor model changes.
- Admin sponsor management changes.
- New sponsor fields, package metadata, pricing, lead forms, or uploaded assets.
- Changes to the home page sponsor slider, games sponsor marquee, or tournament sponsor placements.
- A full landing-page hero comparable to the home page hero.

## Decisions

1. Use a stronger shared page hero, not a bespoke fullscreen hero.

   The sponsors page should adopt the shared `brand-page-hero` foundation with sponsors-specific page-scoped overrides, actions, and a compact partner panel. This creates a stronger first impression while remaining consistent with non-homepage hero treatment and avoiding a home-page-style fullscreen showcase.

   Alternative considered: keep the current `brand-section-shell` intro and only adjust copy. That would be low-risk but would not satisfy the requested polish or visual hierarchy.

2. Keep sponsor data service and model usage unchanged.

   The page should continue using `ISponsorService.GetSponsorsAsync()` and existing sponsor fields. Any partner counts, tier summaries, or card states should be derived client-side from the loaded sponsor list.

   Alternative considered: add backend-provided page metadata or sponsor package details. That would create unnecessary API scope and slow down a focused frontend improvement.

3. Add static sponsor-value content on the page.

   The "why sponsors matter" section should use short editorial copy about event quality, tournament support, and community experience rather than requiring new CMS/backend data. This gives the page needed context without expanding data ownership.

   Alternative considered: infer value copy from sponsor descriptions. Sponsor descriptions are partner-specific and optional, so they are not reliable for explaining the event-level sponsorship story.

4. Use tier-specific composition around shared sponsor cards.

   The shared `brand-sponsor-card` family should remain the base card treatment. Page-scoped classes should tune tier section backgrounds, grid density, card sizing, logo space, and description visibility. Presenting cards should appear larger and may sit inside a more prominent tier panel; Gold, Silver, and Bronze should progressively increase grid density.

   Alternative considered: create entirely new sponsor card styling inside the page stylesheet. That would duplicate shared brand-surface work and make future cross-page consistency harder.

5. Make the empty state a sponsorship invitation.

   When no sponsors are available, the page should still explain the sponsor opportunity and provide a clear route to become a sponsor. It should avoid dead-end messaging like "No sponsors available" as the primary experience.

   Alternative considered: hide the sponsor content area when empty. That would leave the page feeling broken and remove the conversion opportunity.

## Risks / Trade-offs

- Hero/spec conflict risk -> Update the `page-hero-experience` sponsors intro requirement so the stronger intro is intentional, still visitor-facing, and not a dashboard-style metric summary.
- Overly decorative page risk -> Reuse shared brand primitives and keep page-scoped CSS focused on composition, responsive layout, and tier emphasis.
- Empty tier confusion -> Render only tiers that contain sponsors and keep the full empty state for the no-sponsors case.
- Sparse sponsor descriptions -> Cards must render cleanly when `Description` is missing and should not require fallback copy that pretends to be partner-provided.
- External link/accessibility regressions -> Keep sponsor cards as links to `InfoUrl` with `target="_blank"`, `rel="noopener noreferrer"`, clear `aria-label`, and logo alt text.
- Responsive density issues -> Use tier-specific CSS grids with `auto-fit`/`minmax` and mobile breakpoints so Presenting cards do not crowd and Bronze cards do not become too tiny.
