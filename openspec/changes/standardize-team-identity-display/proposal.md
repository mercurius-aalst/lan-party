## Why

Team identity appears in several public and authenticated surfaces, but not every surface uses the saved team logo treatment from the team management page. This leaves some pages showing initials-only team avatars even when a logo is available.

## What Changes

- Add a shared team identity display component that renders saved team logos with transparent image backgrounds and initial fallback behavior.
- Replace similar team identity displays across public team, participant card, bracket, match detail, and placement surfaces with the shared component where team logo data is available.
- Preserve existing surrounding page layouts, actions, loading states, and privacy-safe data boundaries.

## Impact

- Affects Blazor shared components and team-facing page/tournament display markup.
- Does not change backend contracts or fetch additional team data.
