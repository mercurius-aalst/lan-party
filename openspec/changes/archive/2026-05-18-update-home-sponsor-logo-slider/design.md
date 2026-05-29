## Context

The homepage already uses a sponsor logo slider instead of tiered sponsor cards, but the current version still feels unfinished in placement and presentation. The section still sits too low in the page, its spacing is not aligned cleanly with surrounding sections, the separate copy column reduces room for the logos, and the edge fade still reveals too much of the underlying track transition.

## Goals / Non-Goals

**Goals:**

- Move the sponsor showcase higher on the homepage so it appears before the feature cards section
- Merge the sponsor copy and CTA into the same visual panel as the marquee so the slider has more room
- Make the sponsor slider feel visually balanced within the overall sponsor panel
- Increase or refine logo frame sizing so sponsor logos fit comfortably
- Improve the edge fade treatment so the marquee feels softer and more continuous
- Keep the existing animated infinite-slider concept and sponsor link behavior

**Non-Goals:**

- Reintroduce tier-based sponsor cards on the homepage
- Redesign the dedicated sponsors page
- Change sponsor data loading or backend contracts
- Replace the homepage sponsor slider with a different interaction model

## Decisions

### Decision 1: Reposition the sponsor section directly beneath the hero

The sponsor showcase will move above the feature cards section, directly below the hero. This makes sponsor visibility stronger on the landing page and aligns better with the intended prominence of partner branding.

### Decision 2: Merge the copy and CTA into the sponsor panel header

The sponsor copy and CTA will sit inside the same sponsor panel as the marquee, above the slider rather than in a separate column. This gives the logos more horizontal room while keeping the section content grouped into a single component.

### Decision 3: Increase logo container flexibility

Logo frames will be adjusted to provide more vertical room and cleaner internal spacing. This prevents cramped presentation and reduces the sense that logos or their containers do not fit the marquee height.

### Decision 4: Strengthen the marquee masking and track presentation

The fade on both sides of the slider will be refined so the visible transition is softer and the moving-track seam is less noticeable during animation. This should improve the illusion of a continuous loop without changing the underlying infinite-slider approach.

## Risks / Considerations

- Moving the sponsor section higher changes homepage visual rhythm, so spacing around neighboring sections needs to be rebalanced carefully
- A stronger fade treatment must still leave enough visible area for sponsor logos
- Larger logo frames must remain responsive on smaller screens
