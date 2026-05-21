## 1. Update Routed Navigation Markup

- [x] 1.1 Replace plain anchor elements that target internal routed pages in `src/Mercurius.LAN.Web/Components/Layout/NavMenu.razor` with Blazor `NavLink` components.
- [x] 1.2 Keep fragment-only items and action-style controls in their appropriate element types so they do not participate in routed active-state behavior.
- [x] 1.3 Apply the same routed-navigation treatment consistently to eligible dropdown destinations such as admin and profile page links.

## 2. Preserve Menu Interaction Behavior

- [x] 2.1 Ensure routed `NavLink` selections still invoke the shared navigation selection handler that closes dropdowns and the mobile navigation panel.
- [x] 2.2 Adjust any navigation helper code in `src/Mercurius.LAN.Web/Components/Layout/NavMenu.razor.cs` only as needed to keep login, logout, and non-route actions working as they do today.

## 3. Verify Active-State And Responsive Behavior

- [ ] 3.1 Verify that route-based header items show the active visual state only for the current routed page.
- [ ] 3.2 Verify that fragment links and action-style controls do not appear as the active routed navigation item.
- [ ] 3.3 Verify that mobile navigation and dropdown menus close correctly after routed selections and supported actions.
