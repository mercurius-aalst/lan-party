## Why

The game detail page can lose its overview when match, bracket, and participant datasets grow, making visitors scan through too much dense content at once. The bracket view can also visually collide with nearby participant content, even though the bracket itself should remain a full-width inspection surface.

## What Changes

- Improve the game detail layout so matches, bracket, and participants remain easy to scan when their content volume is high.
- Preserve the bracket as a full-width view while separating it clearly from participant content in the page flow.
- Add clearer containment, spacing, and progressive disclosure patterns for dense match and participant sections.
- Ensure large bracket presentations do not overlap, clip into, or visually compete with adjacent sections.
- Keep the existing standalone section model for overview, schedule, bracket, participants, and results.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `game-detail-page-experience`: Update the public tournament detail page requirements so dense match, bracket, and participant content stays scannable, with a full-width bracket that remains visually isolated from participant sections.

## Impact

- Affects the public game detail page layout and the match schedule, bracket, and participant section components.
- May require updates to responsive layout CSS, section spacing, overflow behavior, and content-density controls.
- No API, data model, authentication, or dependency changes are expected.
