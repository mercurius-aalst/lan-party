## Why

The public sponsors page currently functions as a tiered list, but it does not yet give partners the same polished, branded presentation as the rest of the redesigned public site. Improving this page creates a better visitor impression, gives sponsors clearer value, and makes the sponsor call to action more intentional without changing sponsor data or backend behavior.

## What Changes

- Replace the plain sponsors intro with a stronger partner-oriented hero/intro that still fits the existing non-homepage brand system.
- Add clear sponsorship calls to action for browsing current partners and becoming a sponsor.
- Add a concise section explaining why sponsors matter to the LAN event experience.
- Present sponsor tiers as visually distinct sections, with Presenting sponsors receiving the strongest emphasis.
- Improve sponsor cards using existing sponsor fields, tier labels, shared brand/card/button classes, and responsive tier-specific grids.
- Add a more helpful empty state that invites prospective partners instead of only saying no sponsor data is available.
- Keep the scope limited to the public sponsors page, its code-behind helpers, and scoped styles unless a tiny shared-style adjustment is required.
- Avoid backend, API, persistence, and sponsor service changes.

## Capabilities

### New Capabilities
- `public-sponsor-showcase`: Public sponsors page showcase behavior, including partner intro, sponsorship CTA, sponsor-value content, tier hierarchy, responsive sponsor cards, and empty states.

### Modified Capabilities
- `page-hero-experience`: Updates the sponsors page intro contract from a minimal intro toward a stronger branded partner intro while preserving a visitor-facing, non-dashboard presentation.

## Impact

- Public page: `src/Mercurius.LAN.Web/Components/Pages/Sponsors.razor`.
- Supporting page logic: `src/Mercurius.LAN.Web/Components/Pages/Sponsors.razor.cs`.
- Scoped styling: `src/Mercurius.LAN.Web/Components/Pages/Sponsors.razor.css`.
- Shared styling: existing `brand-section`, `brand-section-shell`, `brand-card`, `brand-sponsor-card`, `section-eyebrow`, `button-primary`, and `button-secondary` classes should be reused; shared CSS changes should only be made if page-scoped overrides cannot support the design cleanly.
- Verification: build the solution and manually/browser-check loaded sponsors, missing tier groups, empty sponsor data, external sponsor links, CTA links, and responsive layouts.
