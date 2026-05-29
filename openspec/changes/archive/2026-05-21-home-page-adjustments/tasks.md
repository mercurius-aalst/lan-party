## 1. Hero Content Structure

- [x] 1.1 Move the event date display out of the pre-logo chip row in `Home.razor`.
- [x] 1.2 Place the event date display within the grouped hero information area below the logo and event title.
- [x] 1.3 Remove the small event description paragraph from the hero while keeping the event title visible.
- [x] 1.4 Preserve the existing primary and secondary hero actions.

## 2. Hero Information Styling

- [x] 2.1 Add or adjust CSS classes for the grouped event date display so it matches the surrounding hero information elements.
- [x] 2.2 Ensure the event date display remains readable for long date ranges on desktop and mobile.
- [x] 2.3 Update responsive rules so the date display and stat cards stack or span cleanly on small screens.

## 3. Bottom Fade Motion

- [x] 3.1 Soften the bottom hero fade gradient so the transition into the white homepage content is less abrupt.
- [x] 3.2 Add a subtle breathing animation to the bottom viewport fade without changing layout height or moving content.
- [x] 3.3 Disable the breathing fade animation under `prefers-reduced-motion: reduce` while preserving a softened static fade.
- [x] 3.4 Synchronize the scroll prompt label, arrow, and highlight animation so the cue feels aligned.

## 4. Verification

- [x] 4.1 Build the web project to verify the Razor and CSS changes compile successfully.
- [x] 4.2 Check the home page at desktop width for the updated information grouping and softened viewport handoff.
- [x] 4.3 Check the home page at mobile width for readable date layout, preserved title visibility, and no overlapping hero content.
- [x] 4.4 Verify reduced-motion mode does not animate the bottom fade.
