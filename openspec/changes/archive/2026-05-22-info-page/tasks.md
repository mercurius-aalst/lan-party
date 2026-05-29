## 1. Page Foundation

- [x] 1.1 Add a public `/info` Razor page backed by centralized LAN event configuration
- [x] 1.2 Add the info page to primary visitor navigation with route-aware behavior consistent with existing routed pages
- [x] 1.3 Update home-page tickets/contact teaser links to point to the new info page where appropriate
- [x] 1.4 Resolve the canonical Odisee Aalst event address and align visible event-location text across the info page and existing home page
- [x] 1.5 Move reusable LAN event data such as date, address, socials, tickets, packing items, and catering data into typed configuration

## 2. Event Details and Map

- [x] 2.1 Display the event date/time or maintained event window near the location details
- [x] 2.2 Add a responsive embedded map for Kwalestraat 154, 9320 Aalst at Odisee Aalst
- [x] 2.3 Add an external directions link that opens the event location in a maps experience

## 3. Visual Information Sections

- [x] 3.1 Choose the packing-guide approach: generated bitmap asset, responsive code-built visual, or hybrid
- [x] 3.2 Implement the visual packing guide for computer, max 2 monitors, power cables, power strip, headset, mouse, keyboard, mousepad, phone and charger, and ID
- [x] 3.3 Add accessible labels or supporting text for all items represented in the packing guide
- [x] 3.4 Implement vintage physical-ticket styled pricing for Visitors, Gamer, and Gamer + gaming chair options
- [x] 3.5 Implement restaurant food-menu styled catering sections for current drinks and snacks with visible prices, keeping item data easy to update

## 4. Contact and Social Links

- [x] 4.1 Add a public contact form with name, contact address, and message fields
- [x] 4.2 Wire the contact form to server-side SMTP email submission instead of `mailto:`
- [x] 4.3 Add Discord, Facebook, and Instagram links with accessible labels and external-link behavior where appropriate

## 5. Styling, Motion, and Responsiveness

- [x] 5.1 Style the info page with existing brand primitives, scoped CSS, Bootstrap Icons, and current button hierarchy
- [x] 5.2 Add supportive animation or hover treatments for the packing guide, tickets, catering menu, map, and social/contact sections
- [x] 5.3 Add reduced-motion fallbacks for nonessential info-page animations
- [x] 5.4 Verify narrow and wide viewport layouts avoid overlapping text, clipped controls, horizontal scrolling, and map overflow

## 6. Verification

- [x] 6.1 Build the web project and resolve compile or scoped CSS issues
- [x] 6.2 Validate the OpenSpec change after implementation updates
- [x] 6.3 Manually verify public access, navigation, map rendering, directions link, ticket readability, catering menu presentation, contact behavior, and social links
