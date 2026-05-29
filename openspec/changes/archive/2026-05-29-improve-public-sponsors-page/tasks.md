## 1. Page Structure and Content

- [x] 1.1 Update `Sponsors.razor` to replace the current plain intro with a stronger sponsor-focused `brand-page-hero` intro.
- [x] 1.2 Add hero actions for browsing current partners and becoming a sponsor using existing button classes and destinations.
- [x] 1.3 Add a concise "why sponsors matter" section using existing brand/card section classes and static visitor-facing copy.
- [x] 1.4 Ensure the page keeps using `SponsorService` data and the existing `Sponsor` fields without backend/API/model changes.

## 2. Sponsor Tier Showcase

- [x] 2.1 Update tier section markup so each populated tier has a distinct header, summary, and visual treatment.
- [x] 2.2 Give Presenting sponsors the most prominent layout and card emphasis while keeping Gold, Silver, and Bronze progressively denser.
- [x] 2.3 Keep empty sponsor tiers omitted while preserving Presenting, Gold, Silver, and Bronze ordering for populated tiers.
- [x] 2.4 Improve sponsor card markup so cards show logo, name, tier label, link affordance, and optional description without breaking external link behavior.

## 3. Supporting Page Logic

- [x] 3.1 Add or adjust helper methods in `Sponsors.razor.cs` for tier summaries, tier descriptions, section/card classes, and sponsor count copy.
- [x] 3.2 Keep sorting and grouping based on `SponsorTier.GetDisplayOrder()` and existing extension labels.
- [x] 3.3 Ensure helpers render cleanly when descriptions are missing, when only lower tiers exist, or when the sponsor list is empty.

## 4. Scoped Styling

- [x] 4.1 Update `Sponsors.razor.css` to style the hero, action layout, partner panel, sponsor-value section, tier sections, and empty state.
- [x] 4.2 Add tier-specific responsive grids using page-scoped overrides on shared `brand-sponsor-card` variables where possible.
- [x] 4.3 Ensure Presenting, Gold, Silver, and Bronze visual treatments remain distinct on desktop and collapse cleanly on tablet/mobile.
- [x] 4.4 Prefer existing shared classes and CSS custom properties over duplicating shared brand/card/button styling.

## 5. Verification

- [x] 5.1 Build the solution with `dotnet build Mercurius.LAN.sln -p:UseAppHost=false`.
- [ ] 5.2 Verify the sponsors page with available sponsor data shows the hero, sponsor-value section, tier sections, improved cards, and CTAs.
- [ ] 5.3 Verify the page omits empty individual tiers and preserves populated tier order.
- [ ] 5.4 Verify the no-sponsors state shows the improved branded empty state and no empty tier grids.
- [ ] 5.5 Verify sponsor cards link to configured `InfoUrl` values with safe external link behavior and accessible logo text.
- [ ] 5.6 Verify responsive layouts for desktop, tablet, and mobile widths.
