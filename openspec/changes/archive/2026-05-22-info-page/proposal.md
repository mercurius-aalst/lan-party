## Why

Visitors and registered gamers need one reliable place to find the practical LAN party details before arriving: what to bring, where to go, when to attend, ticket options, catering, and how to contact Mercurius Aalst. Presenting this as a styled information experience reduces repeated questions and helps the event feel organized and welcoming.

## What Changes

- Add one or more visitor-facing info pages for the LAN party.
- Display a "what to bring" section as an illustrated or image-led packing guide rather than a simple list, while still covering gaming essentials and basic personal items.
- Show the event date/time and location, including a map for Kwalestraat 154, 9320 Aalst (Odisee Aalst).
- Present ticket pricing for visitors, gamers, and gamers with a gaming chair using a vintage physical-ticket inspired treatment.
- Present catering options in a restaurant food-menu style so changeable drinks, snacks, and prices can be updated without changing the spec contract.
- Add a contact form and social links for Discord, Facebook, and Instagram.
- Centralize reusable LAN event data such as date, address, social links, tickets, and catering data in application configuration.
- Submit contact form messages through server-side email sending rather than opening the visitor's mail app.
- Style the information to match the existing website, using icons and tasteful animations where they make the page easier to scan and less static.

## Capabilities

### New Capabilities

- `lan-info-page`: Visitor-facing LAN information pages covering preparation, event details, ticket pricing, catering menu presentation, contact, and social links.

### Modified Capabilities

None.

## Impact

- Affected frontend routes, navigation, and Razor components in the web application.
- New or updated styling for info-page sections, icons, animations, responsive layout, and map/contact presentation.
- Centralized web-app configuration for reusable LAN event details, social links, tickets, catering, and contact email settings.
- Server-side SMTP email integration for contact form handling.
