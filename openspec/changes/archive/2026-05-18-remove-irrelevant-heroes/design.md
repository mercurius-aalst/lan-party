## Context

The public-facing pages currently use hero sections as the top visual block, but they are not tuned to the purpose of each page. The home page needs a more cinematic first impression, while the sponsors and games overview pages do not benefit from having large hero treatments at all. This change needs a shared direction: make the home page more visually ambitious, and make the other pages simpler and more useful.

## Goals / Non-Goals

**Goals:**

- Make the home page hero a visually stronger landing experience with a more memorable first impression
- Replace the sponsors and games overview heroes with simpler visitor-relevant page intros
- Keep each page top section aligned to its page purpose instead of applying one generic hero pattern everywhere
- Reuse the existing visual language of the site while making the home page feel more polished and dramatic

**Non-Goals:**

- Redesign the full structure of the home, sponsors, or games pages beyond their top sections
- Change sponsor data, game data, or backend contracts
- Introduce a new site-wide hero component abstraction if the pages need materially different markup
- Replace the rest of the homepage sections or the game/sponsor content below the top section

## Decisions

### Decision 1: Treat the redesign as a page-experience change, not a reusable single-component refactor

The home, sponsors, and games overview top sections serve different goals. The implementation should keep separate page-specific markup and CSS where needed rather than forcing all three into one shared hero component. This preserves flexibility for the home page to become more cinematic while allowing the sponsors and games pages to step back to simpler intros.

Alternative considered: create a shared hero component with configuration inputs.
Why not chosen: the page-entry needs are too different, and a forced abstraction would add complexity before the visual direction is stable.

### Decision 2: Make the home hero more visual without depending on new data sources

The home hero should gain its “wow effect” through composition, imagery, layering, animation, and stronger visual hierarchy using data and assets already available in the frontend. It should occupy the first viewport as a full-bleed experience rather than a framed panel, and transition smoothly into the rest of the homepage content instead of ending as a hard visual block. This keeps the change focused on experience and avoids coupling the redesign to backend changes or new content pipelines.

Alternative considered: add new backend-driven hero content or CMS-like fields.
Why not chosen: it increases scope and complexity for a change that is fundamentally about presentation.

### Decision 3: Remove heavyweight heroes from sponsors and games overview

The sponsors and games overview pages should stop using large hero treatments. Instead, they should open with concise page intros that explain what the page is for, what visitors can do there, and what action to take next.

Alternative considered: keep hero sections and just strip out the stats.
Why not chosen: the problem is not only the content inside the hero, but the fact that these pages do not need large showcase blocks at all.

### Decision 4: Preserve the existing design system while pushing the home hero further visually

The redesign should still use the project’s typography, button styles, spacing rhythms, and card language. The home hero can become more dramatic through layout, art direction, and motion, but it should still look like part of the same site rather than a disconnected experiment.

Alternative considered: apply a radically different visual system to the home hero only.
Why not chosen: it would likely make the homepage feel detached from the rest of the product.

## Risks / Trade-offs

- [Home hero becomes visually louder than the rest of the page] -> Use stronger composition and motion selectively, and keep supporting sections visually compatible rather than escalating everything
- [Sponsors and games pages become too plain after removing hero weight] -> Keep a clear heading, concise description, and direct actions so the top of each page still feels intentional
- [Three page-specific top sections drift stylistically] -> Reuse the same base tokens, spacing logic, and interaction language while allowing different layouts
- [Animations reduce usability or feel excessive] -> Keep motion meaningful, restrained, and compatible with reduced-motion preferences

## Migration Plan

- Update the top-of-page markup and CSS on Home, Sponsors, and Games overview pages
- Verify responsive behavior and reduced-motion handling after the redesign
- Ship as a frontend-only change with no data migration or backend rollout steps

## Open Questions

- Whether the home hero should feature a centered poster-style composition or a broader full-bleed background emphasis
