## 1. Tailwind Tooling Setup

- [x] 1.1 Add repo-managed Tailwind build dependencies and configuration files under `src/Mercurius.LAN.Web`
- [x] 1.2 Create the Tailwind input stylesheet and generated output target in `wwwroot`
- [x] 1.3 Configure Tailwind content scanning to include Razor components and any relevant frontend source files
- [x] 1.4 Configure Tailwind to avoid broad global reset behavior during the migration phase

## 2. Frontend Asset Integration

- [x] 2.1 Wire the generated Tailwind stylesheet into `src/Mercurius.LAN.Web/Components/App.razor` alongside the existing Bootstrap and shared CSS assets
- [x] 2.2 Preserve the current shared token system in `src/Mercurius.LAN.Web/wwwroot/app.css` so Tailwind adoption does not replace the established visual identity
- [x] 2.3 Document or script the local build command needed to regenerate the Tailwind asset consistently

## 3. Initial Migration And Responsive Improvements

- [x] 3.1 Apply Tailwind to shared structural layout concerns such as containers, spacing, and breakpoint-driven stacking without forcing a broad visual rewrite
- [x] 3.2 Improve small-screen navigation and page shell behavior so controls remain accessible and layouts do not clip or overlap on mobile
- [x] 3.3 Improve small-screen forms, dialogs, and card/grid layouts so they avoid horizontal overflow and cramped interaction targets
- [x] 3.4 Keep existing Bootstrap-dependent and CSS-isolated components visually stable while introducing Tailwind utilities where needed

## 4. Verification And Cleanup

- [ ] 4.1 Verify representative desktop and mobile screens still match the established visual identity after Tailwind is introduced
- [ ] 4.2 Verify responsive improvements on high-impact screens such as navigation, profile/forms, dialogs, and content grids
- [x] 4.3 Remove or avoid new redundant structural CSS where Tailwind has become the intended source of layout behavior
