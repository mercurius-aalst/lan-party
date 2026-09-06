## 1. Foundation and contract inventory

- [x] 1.1 Inventory every routed page, stable anchor, primary navigation item,
      auth/role gate, form field order, callback, and API/service seam covered by
      the current front end and record the preservation checklist.
- [x] 1.2 Confirm the landed visual foundation, existing shared components,
      CSS/Tailwind/MudBlazor loading order, and required responsive breakpoints
      before adding new composition.
- [x] 1.3 Map each player, team, public-profile, tournament, and organizer
      journey to its existing loading, empty, error, unauthorized, forbidden,
      not-found, conflict, and success states without changing their messages
      or data ownership.

## 2. Shared content primitives

- [x] 2.1 Extend or add the smallest reusable page/section framing primitives
      for headings, local section links, action groups, and bounded surfaces.
- [x] 2.2 Standardize entity identity and safe-link rendering for users, teams,
      tournaments, sponsors, and matches using only supplied public identifiers.
- [x] 2.3 Standardize action pending, disabled, confirmation, validation,
      conflict, and error presentation while keeping callbacks and service calls
      with their containing pages.
- [x] 2.4 Standardize data-state rendering and recovery affordances for loading,
      empty, unauthorized, forbidden, not-found, conflict, and error outcomes.
- [x] 2.5 Verify primitives preserve accessible names, keyboard order, focus
      visibility, and existing field order when adopted by forms and dialogs.

## 3. Shell and player-first discovery

- [x] 3.1 Recompose the shell navigation into concise player-goal groups while
      retaining canonical routes, account-access visibility, global search
      behavior, notification behavior, ticket action, admin visibility, and
      mobile close callbacks.
- [x] 3.2 Add contextual next-action and conditional local section navigation
      affordances to Home, event information, sponsors, and tournament browse
      surfaces without duplicating route ownership or API reads or adding a
      redundant anchor rail.
- [x] 3.3 Reorganize Home and public discovery composition around event
      orientation, tournament discovery, sponsor context, and clear next steps
      while preserving every existing anchor, image resolver, copy, and empty or
      load-error branch.
- [x] 3.4 Preserve and verify login, register, logout, privacy/legal links, and
      validated return destinations while the shell is reorganized, including
      compatibility with the active account-access changes.

## 4. Tournament browse, detail, and registration journeys

- [x] 4.1 Recompose tournament browse controls, results, schedule context,
      admin add access, sponsor context, pagination, and empty states into the
      player-first discovery flow without changing filtering or query behavior.
- [x] 4.2 Add tournament detail section reachability and persistent identity/
      status context while preserving standalone overview, schedule, bracket,
      participant, result, partner, and registration action surfaces. The
      registration workflow itself is launched from an accessible popup.
- [x] 4.3 Reorganize individual registration and unregister actions with clear
      eligibility, confirmation, pending, success, conflict, and failure states
      while retaining the current service/API operations and refresh semantics.
- [x] 4.4 Reorganize team registration stepper entry, roster ownership,
      eligibility explanations, confirmation, draft preservation, realtime
      updates, and a user-friendly update action without changing the
      three-step contract or backend request shapes.
- [x] 4.5 Preserve privacy-safe participant rendering, canonical participant and
      tournament links, bracket overflow behavior, match dialogs, and all
      schedule timing semantics after composition changes.

## 5. Team and profile journeys

- [x] 5.1 Recompose the authenticated team hub to distinguish captain-owned,
      member-owned, sent-invite, and received-invite work while retaining team
      creation, invite, roster, captain, leave, delete, logo, and toast flows.
- [x] 5.2 Reorganize public user and team profile surfaces around identity,
      returned relationships, tournament context, and match summaries while
      preserving privacy-safe fields and omission behavior.
- [x] 5.3 Preserve profile completion and authenticated profile updates,
      including field order, validation, cancellation/back behavior, success and
      error states, and existing Auth0/current-user boundaries.
- [x] 5.4 Verify public profile, team, tournament, and match-summary handoffs
      use canonical route identifiers and never issue private enrichment calls.

## 6. Organizer workflows

- [x] 6.1 Group authorized tournament setup, update, schedule, lifecycle,
      registration oversight, sponsor placement, and team-management controls
      into task-oriented organizer surfaces without changing role checks.
- [x] 6.2 Preserve organizer form field order, validation, multipart names,
      lifecycle state transitions, sponsor zero-or-one semantics, delete/remove
      confirmations, and backend-authoritative refresh behavior.
- [x] 6.3 Recompose sponsor administration separately from public sponsor
      discovery, retaining public links, logo handling, admin mutation feedback,
      and mock/live response parity.
- [x] 6.4 Verify organizer actions show pending, success, validation, conflict,
      unauthorized, forbidden, not-found, and error feedback without optimistic
      state or duplicate submissions.

## 7. Responsive and accessibility pass

- [x] 7.1 Apply responsive composition rules at 640, 860, 960, 1100, and 1280
      pixels for shell, browse, detail, forms, dialogs, rosters, schedules,
      brackets, notifications, and organizer panels.
- [x] 7.2 Contain dense content overflow inside its owning surface and verify no
      page-wide lateral overflow or action overlap at mobile widths.
- [x] 7.3 Audit keyboard navigation, focus order, focus visibility, labels,
      `aria-expanded`/`aria-controls`, dialog focus, live regions, and Escape/
      Enter/Arrow/outside-click behavior for moved interactions.
- [x] 7.4 Verify reduced-motion behavior and ensure motion is never the only
      state or navigation cue; confirm dark/light logo contrast using existing
      assets.

## 8. Contract preservation and validation

- [x] 8.1 Audit the final diff to confirm no APIClients, Services, DTOs,
      authentication, authorization, mock behavior, JavaScript, or backend
      contract changes were introduced by the reorganization.
- [x] 8.2 Exercise representative mock-mode journeys for anonymous discovery,
      authenticated registration, team/invite management, profile updates,
      public profiles, and organizer mutations, including recoverable failure
      states.
- [x] 8.3 Run route/anchor, form-order, role-visibility, search-keyboard, and
      callback checks against the preservation checklist.
- [x] 8.4 Run strict OpenSpec validation, `dotnet build
      src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj`, `git diff --check`, and
      responsive/accessibility browser smoke checks where the environment
      supports them; document any unavailable live/Auth0 or browser checks.

## 9. Corrective visual QA expansion

- [x] 9.1 Audit the landed shared CSS and MudBlazor theme as one semantic token
      source for light and dark modes, identify component-local theme
      declarations and specificity conflicts, and record the representative
      surfaces covered by the corrective pass.
- [x] 9.2 Restore Home to a true full-viewport, media-led hero using the
      existing dynamic image resolver and fallbacks, keeping the current
      headline, date/location, both CTAs, responsive crop, reduced-motion
      behavior, and no-overflow requirements intact at 1440, 1280, and 390
      pixel targets.
- [x] 9.3 Correct light and dark computed styles and WCAG AA readability for custom
      dialogs, Mud controls/steppers, filters, tabs, brackets, match dialogs,
      profile relationship cards, team invite/danger surfaces, nested values,
      loading/empty/error/status states, and semantic sponsor/winner/loser
      distinctions without broad flattening overrides.
- [x] 9.4 Audit the listed high-risk visual paths, including tournament match
      details and participants, tournament/admin dialogs, team management,
      public profiles, Info, placements/overview/detail, autocomplete,
      participant cards, loading, navigation/search, sponsors, privacy/status,
      and team dialogs, preserving all callbacks and state branches.
- [ ] 9.5 Run mock-mode browser verification at 1440x900, 1280x900, and
      390x844 across representative public, authenticated, and organizer
      routes, opening reachable dialogs and recording computed styles, WCAG AA
      contrast, hero bounds, and document overflow before completion.
- [ ] 9.6 Re-run contract/static checks, strict OpenSpec validation, isolated
      build, 151 contract tests, and `git diff --check`; mark this section
      complete only when browser evidence and API/route/form/auth preservation
      remain intact.

## 10. Follow-up redesign alignment

- [x] 10.1 Record the design-taste audit for the implementation: the event-
      community design read, `DESIGN_VARIANCE: 7`, `MOTION_INTENSITY: 5`,
      `VISUAL_DENSITY: 4`, the single semantic token strategy, the chosen shape
      and accent rules, and the reduced-motion plan.
- [ ] 10.2 Implement a page-wide light/dark theme choice with system preference
      as the default when no preference is stored, an accessible manual toggle,
      persisted explicit choice, and parity checks for both modes. Dark mode
      MUST use neutral surfaces and readable off-white text without a green tint
      across body copy, forms, dialogs, and status surfaces.
- [x] 10.3 Apply the visual calibration pass: use whitespace and hierarchy
      before borders, remove duplicate parent/child frames, add comfortable
      spacing between labels and controls, collapse redundant title-subtitle or
      title-description stacks, and restore visible priority/type accents such
      as the "what to bring" item treatments in both themes.
- [x] 10.4 Recompose Home into only Hero, Sponsors, Tournaments, Tickets, and
      Contact in that order. Preserve required compatibility anchors inside the
      relevant content where needed, but remove redundant anchor navigation and
      filler sections. Add purposeful entry/feedback motion with a reduced-
      motion equivalent.
- [x] 10.5 Keep the Tournaments overview in its general existing browse layout.
      Apply visual polish only where it improves clarity; do not restructure the
      overview into a thereality.nl-style editorial composition. Verify search,
      sort, filters, results, pagination, schedule context, and admin visibility.
- [x] 10.6 Move the tournament registration workflow behind an accessible popup
      trigger. Keep the existing three logical team-registration steps, auto-
      advance after an eligible team is selected, use a click plus friendly
      confirmation for individual registration, and show unavailable teams or
      members with visual state plus accessible plain-language explanations.
      Replace user-facing technical refresh/reason wording with friendly copy or
      a labelled icon while retaining precise accessible status information.
- [x] 10.7 Fully recompose team management around captain, member, and invite
      tasks while retaining create, invite, accept/decline, roster, captain,
      leave, delete, and logo actions. Make team-management users, tournament
      participants, and tournament line-up users open a privacy-safe popup with
      returned names and linked IDs instead of navigating to a public profile.
- [x] 10.8 Verify that existing GUID-based tournament detail routes remain
      canonical. Static route/link inspection confirms the route declaration and
      all current tournament links use GUID-valued identifiers, with fragments
      preserved where required; no name/slug route or lookup logic is introduced.
- [ ] 10.9 Run focused browser validation for the new theme toggle, Home section
      order and no-rail behavior, unchanged Tournaments overview layout,
      registration popup focus and auto-advance, individual confirmation,
      user-info popups, visual eligibility states, canonical GUID routes,
      reduced motion, keyboard dismissal, WCAG AA contrast, and page overflow at
      1440x900, 1280x900, and 390x844 in mock mode where available.
- [x] 10.10 Re-run strict OpenSpec validation, `dotnet build
      src/Mercurius.LAN.Web/Mercurius.LAN.Web.csproj`, `git diff --check`, and
      route/API/auth/privacy preservation checks. Record unavailable live/Auth0
      or browser evidence without weakening the requirements.
