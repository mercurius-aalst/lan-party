## Why

The home page currently presents sponsors as tier-specific card grids, which makes the sponsor section visually heavy for a landing page and ties the layout to detailed tier presentation. Replacing it with an animated infinite logo slider will create a lighter homepage showcase while keeping the dedicated sponsors page as the place for full sponsor detail.

## What Changes

- Replace the tiered sponsor card groups on the home page with a unified animated sponsor logo slider
- Remove homepage-specific sponsor presentation based on sponsor tiers
- Keep sponsor data loading on the home page unchanged
- Preserve the link from the home page to the full sponsors page

## Capabilities

### New Capabilities

- `home-sponsor-showcase`: Display homepage sponsors as a continuous animated logo slider instead of tiered cards

### Modified Capabilities

## Impact

- `src/Mercurius.LAN.Web/Components/Pages/Home.razor`: replace tiered sponsor markup with slider-based presentation
- `src/Mercurius.LAN.Web/Components/Pages/Home.razor.cs`: simplify home-page sponsor view logic if tier-based helpers are no longer needed
- `src/Mercurius.LAN.Web/Components/Shared/SponsorScroller.razor`
- `src/Mercurius.LAN.Web/Components/Shared/SponsorScroller.razor.cs`
- `src/Mercurius.LAN.Web/Components/Shared/SponsorScroller.razor.css`
