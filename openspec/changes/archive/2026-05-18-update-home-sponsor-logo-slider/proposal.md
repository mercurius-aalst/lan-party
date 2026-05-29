## Why

The homepage sponsor logo slider is visually close, but the current implementation still feels unfinished. Its placement is still too low on the page, the section spacing does not align with nearby homepage blocks, the supporting copy and CTA take horizontal space away from the logos, and the marquee edges still expose a harder transition than intended.

## What Changes

- Move the homepage sponsor logo slider higher in the page flow so it appears before the feature cards section
- Merge the sponsor copy and CTA into the same sponsor panel as the slider so the logo marquee gets more horizontal space
- Refine the sponsor section layout and spacing so it aligns with the rest of the homepage rhythm
- Adjust logo frame sizing and spacing so sponsor logos fit comfortably within their containers
- Improve the marquee edge treatment so the slider transitions feel softer and the hard track boundary is no longer obvious
- Preserve the animated infinite-slider approach and existing sponsor link behavior

## Capabilities

### New Capabilities

### Modified Capabilities

- `home-sponsor-showcase`: Refine the homepage sponsor slider layout, sizing, placement, integrated copy/CTA, and edge transitions while keeping the animated logo-slider presentation

## Impact

- `src/Mercurius.LAN.Web/Components/Pages/Home.razor`: move the sponsor section and update its structure as needed
- `src/Mercurius.LAN.Web/Components/Pages/Home.razor.css`: refine section sizing, logo frame dimensions, animation presentation, and edge fade treatment
- `openspec/specs/home-sponsor-showcase/spec.md`: add or update requirements for placement and visual behavior of the homepage sponsor slider
