# Standardize Image Upload UI

## Why
Image upload fields outside team management still render as default browser file controls, while the team logo picker now has a branded upload affordance that better matches the Mercurius UI.

## What Changes
- Reuse the branded image upload picker styling for shared image upload fields.
- Keep existing image validation, accepted file types, model binding, and temporary-file behavior intact.

## Impact
- Affects shared image upload UI rendered through `CustomInputFile`.
- Affects sponsor logo and game image upload fields.
