## Context

The Mercurius LAN web app is a Blazor application with route-based Razor pages, component-scoped CSS, Bootstrap Icons, Tailwind utilities, and shared visual primitives such as `brand-section`, `brand-card`, `section-eyebrow`, `button-primary`, and `button-secondary`. Existing visitor pages use a polished event identity with responsive grids, strong section hierarchy, hover transitions, and reduced-motion fallbacks.

The info page should become the practical visitor reference for the LAN party. It needs to present an image-led packing guide, event timing, location, ticket prices, a restaurant-style catering menu, contact options, and social links in a way that feels aligned with the current site rather than a static FAQ dump.

## Goals / Non-Goals

**Goals:**

- Add a public `/info` route that visitors and gamers can use before and during the event.
- Structure information into scannable sections: illustrated essentials, when/where, ticket-style pricing, restaurant-style catering, and contact.
- Use the existing brand styling, button hierarchy, scoped CSS, responsive grid behavior, and motion conventions.
- Use icons, compact visual treatments, and light animation to make dense information easier to scan.
- Keep content maintainable by defining reusable event data in centralized application configuration instead of scattering repeated markup.
- Include accessible text labels, keyboard-friendly links/forms, responsive layout, and reduced-motion fallbacks.

**Non-Goals:**

- Do not add ticket purchasing, competition registration, or payment behavior.
- Do not add a full catering management system or backend-driven menu administration.
- Do not make the exact catering items or prices part of the normative spec contract.
- Do not add a new design system or replace existing shared brand styles.
- Do not add a separate public contact API unless the Blazor server-side form flow proves insufficient.
- Do not modify tournament/game data models.

## Decisions

1. Implement the info experience as a dedicated public Blazor page at `/info`.

   Rationale: The requested content is broad enough to deserve its own route and navigation entry. Keeping it separate avoids overloading the home page while allowing the home page's tickets/contact teasers to point to deeper information.

   Alternative considered: Add all details to the home page. This would make the first page longer and mix event preparation, menu pricing, and contact fields into a landing page that already has hero, sponsors, games, schedule, tickets, and contact sections.

2. Store reusable LAN event content in centralized application configuration.

   Rationale: The event date, address, social links, ticket options, packing items, and catering menu may be reused by the home page, info page, footer, and future surfaces. Binding them through a typed configuration options class keeps updates reviewable without scattering duplicate values through Razor components.

   Alternative considered: Keep all values directly in Razor markup. That is faster initially but makes shared values and changing menu prices harder to update consistently.

3. Use existing visual primitives and page-scoped CSS for layout.

   Rationale: Current pages already establish the site language through `brand-section`, `brand-card`, shared buttons, cards, responsive grids, and scoped animation. Reusing those keeps the info page coherent with Home, Sponsors, and Games pages.

   Alternative considered: Build a one-off visual system for the info page. That would risk a disconnected page and increase styling maintenance.

4. Use Bootstrap Icons and existing image/icon conventions for scan cues.

   Rationale: Bootstrap Icons are already loaded globally, and the app already uses `bi` classes. Using those keeps icons lightweight and avoids adding a new dependency.

   Alternative considered: Add a new icon package. This would be unnecessary for a static content page and would increase dependency surface.

5. Present "what to bring" as an illustrated packing visual with accessible supporting labels.

   Rationale: A simple checklist would make the most practical part of the page feel flat. An image-led composition, generated bitmap, or carefully designed visual panel can communicate the setup at a glance while supporting text or labels keep the content accessible and searchable.

   Alternative considered: Use only text cards or a checklist. That is clear, but it does not meet the desired richer presentation and would make the section feel static.

6. Present tickets as vintage physical tickets rather than ordinary pricing cards.

   Rationale: Ticket-shaped pricing blocks with perforation details, serial-style metadata, and strong price treatments make the pricing memorable while still being easy to compare.

   Alternative considered: Use generic pricing cards. That would be simpler but would miss the requested cinema-ticket style and feel too similar to ordinary info cards.

7. Present catering as a restaurant food-menu surface.

   Rationale: Catering options and prices are mutable, so the page should communicate "current menu" rather than treating every item as a permanent requirement. A menu-board or restaurant-menu layout also better matches visitor expectations for food and drink pricing.

   Alternative considered: Use a normal table. Tables are efficient, but a menu-style presentation better fits the requested visual direction and keeps the page from becoming a static data sheet.

8. Embed the map with a responsive iframe and provide a normal external map link.

   Rationale: An embedded map gives visitors immediate orientation while the external link supports users who prefer opening navigation in their own map app. The iframe should use a descriptive title, lazy loading, and stable responsive dimensions.

   Alternative considered: Use a static map image. That is simpler but less useful for directions and requires maintaining an image asset.

9. Submit contact form messages through a server-side email service.

   Rationale: A `mailto:` link depends on the visitor's local mail client and does not guarantee the organizer receives the message. A Blazor server-side form can validate the submitted name, contact address, and message, then send email through configured SMTP settings.

   Alternative considered: Add a separate backend API endpoint immediately. That would add another integration boundary without clear benefit while the Blazor server app can already execute server-side submission logic.

10. Use motion as reinforcement, not as the information carrier.

   Rationale: Subtle reveal, hover, and section accent animations can keep the page alive while still leaving all content readable when animations are disabled. Existing pages already include `prefers-reduced-motion` handling, so the info page should follow that pattern.

   Alternative considered: Use heavy animation or interactive accordions for every section. That would make practical information slower to access, especially on mobile during the event.

## Risks / Trade-offs

- [Risk] The requested location is `Kwalestraat 154, 9320 Aalst (Odisee Aalst)`, while the current home page hardcodes `Odisee Aalst, Kwalestraat 1 9300 Aalst, Belgium`. -> Mitigation: Verify the canonical event address during implementation and update shared visible event-location text if needed.
- [Risk] Event date/time can drift if multiple pages manage it independently. -> Mitigation: Store the event window centrally and use it everywhere instead of deriving it from game data.
- [Risk] Contact email delivery depends on SMTP host and credentials. -> Mitigation: Keep SMTP settings in configuration/user-secrets and show a clear error if sending is not configured or fails.
- [Risk] Catering options and prices are subject to change. -> Mitigation: Keep catering data centralized and make the spec require a menu-style presentation with visible current prices rather than specific menu entries.
- [Risk] Embedded maps can affect performance or privacy. -> Mitigation: Lazy-load the map, keep the iframe isolated to the location section, and include an external link as a non-embedded alternative.

## Migration Plan

1. Add the `/info` Razor page, code-behind data, visual asset strategy, and scoped CSS.
2. Add navigation to the info page from the main navigation and replace home-page teaser links where appropriate.
3. Implement the map section and external directions link.
4. Implement the illustrated packing guide, vintage ticket pricing treatment, and restaurant-style catering menu.
5. Implement the contact form with server-side SMTP email submission.
6. Build and manually verify desktop/mobile layouts, focus states, map rendering, social links, visual assets, and reduced-motion behavior.
7. Roll back by removing the `/info` route and navigation entry if the page needs to be withdrawn before launch.

## Open Questions

- What SMTP provider and credentials should be configured for production mail delivery?
- Should the packing guide use a generated bitmap asset, a hand-built responsive visual panel, or a hybrid of both?
