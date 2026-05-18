## Why

The frontend currently relies on a mix of global CSS, component-scoped styles, and Bootstrap assets, which makes styling conventions harder to keep consistent as the application grows. Moving to Tailwind CSS provides a more structured styling foundation so the team can standardize future UI work while preserving the current visual identity and improving the mobile experience where needed.

## What Changes

- Introduce Tailwind CSS as the primary styling framework for the web application.
- Define a migration path for existing frontend styles so current pages and components retain their established look and behavior after the framework change.
- Establish project-level styling conventions and build support needed for structured utility-based styling in the frontend.
- Allow targeted responsive and mobile usability improvements that do not amount to a broad visual redesign.

## Capabilities

### New Capabilities
- `tailwind-styling-foundation`: The web application supports Tailwind CSS as its structured styling framework while preserving the current visual identity of existing UI surfaces and enabling responsive improvements for mobile users.

### Modified Capabilities

## Impact

- Affects the frontend project in `src/Mercurius.LAN.Web`.
- Affects shared styling assets such as `wwwroot/app.css` and component/page style files under `Components/**/*.razor.css`.
- Likely affects frontend dependency and build configuration used to produce application CSS assets.
- May affect how existing Bootstrap-based or custom styles are authored and maintained going forward.
