## 1. Shared Sponsor Marquee Refactor

- [x] 1.1 Refactor `Components/Shared/SponsorScroller` so it accepts sponsor data as input instead of loading sponsors internally.
- [x] 1.2 Move the richer marquee markup, linked logo-card behavior, loop structure, and accessibility treatment from `Home.razor` into the shared sponsor marquee component.
- [x] 1.3 Move or adapt the home-page marquee styles into the shared sponsor marquee stylesheet so the reusable component preserves masking, animation, and responsive logo sizing.
- [x] 1.4 Update `Home.razor` to render the shared sponsor marquee component inside the existing home sponsor section wrapper without changing the home hero.

## 2. Games Overview Browse Experience

- [x] 2.1 Update `GamesOverview.razor` to remove the separate top hero/intro block and replace it with a single browse-first surface that combines the heading, search, sort, and filter controls.
- [x] 2.2 Restyle `GamesOverview.razor.css` so the combined browse surface feels visually cohesive on desktop and mobile while preserving the existing search, sort, and filter behavior.
- [x] 2.3 Remove the `Need more context?` callout section and any now-unused related markup or styles from the games overview page.
- [x] 2.4 Add sponsor loading to `GamesOverview.razor.cs`, fetch games and sponsors in parallel, and render the shared sponsor marquee only when sponsor data is available.

## 3. Verification

- [x] 3.1 Build the web project to verify the Razor, C#, and CSS changes compile successfully.
- [x] 3.2 Check the games overview page at desktop width to confirm the browse-first layout, removed callout, and sponsor marquee behavior.
- [x] 3.3 Check the games overview page at mobile width to confirm the unified controls remain grouped and usable without layout breakage.
- [x] 3.4 Check the home page sponsor section to confirm it still renders correctly after switching to the shared sponsor marquee component.
