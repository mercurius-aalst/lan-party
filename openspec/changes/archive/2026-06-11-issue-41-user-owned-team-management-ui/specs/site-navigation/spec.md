## ADDED Requirements

### Requirement: Authenticated navigation exposes team management and user notifications
The site navigation SHALL provide authenticated users with a discoverable route to their team management area and SHALL surface user-level notifications, including team invites and team-related updates, through a clear branded notification control with a count.

#### Scenario: Authenticated user has team management access
- **WHEN** an authenticated user opens the header or mobile navigation
- **THEN** navigation provides access to the team management area
- **AND** the team management destination uses Blazor-native route navigation behavior
- **AND** selecting it closes temporary navigation surfaces consistently with other routed destinations

#### Scenario: Authenticated user has notifications
- **WHEN** an authenticated user has one or more unread or actionable user notifications
- **THEN** navigation presents a notification bell action within the authenticated user widget
- **AND** the count is visible as a compact badge that does not obscure the bell icon in desktop and mobile navigation contexts where the notification control is shown
- **AND** opening the notification bell shows a compact list of user notifications separately from the profile, teams, and logout menu
- **AND** notification entries for team invites can navigate to the team management area or received invites section
- **AND** the notification control does not expose private notification details in public or anonymous navigation

#### Scenario: Notification actions fit inside the notification menu
- **WHEN** an authenticated user opens the notification menu with entries that have action buttons
- **THEN** the notification list is wide enough for the notification title, message, and available action buttons
- **AND** action buttons MUST NOT overlap notification title or message text
- **AND** long notification text wraps within the notification row instead of pushing controls out of the menu

#### Scenario: Account and notification menus close on outside click
- **WHEN** an authenticated user has the profile menu or notification menu open
- **AND** the user clicks outside the authenticated user widget and its open dropdown
- **THEN** the open profile or notification menu MUST close
- **AND** clicks inside the authenticated user widget or open dropdown MUST NOT close the menu before its selected action can run

#### Scenario: Admin navigation is rendered
- **WHEN** an authenticated admin opens admin navigation
- **THEN** admin team management is not shown as an admin navigation item
- **AND** removing the admin team item does not disrupt remaining admin navigation destinations

#### Scenario: Notification state changes
- **WHEN** an authenticated user accepts, declines, receives, reads, or otherwise resolves a user notification during the session
- **THEN** the notification control updates to reflect the current confirmed notification state
- **AND** stale notification counts or labels are not left visible after the notification state refreshes

#### Scenario: Team hub invite notification arrives
- **WHEN** an authenticated user receives a team SignalR hub notification for a pending team invite
- **THEN** the authenticated navigation updates its notification control after refreshing confirmed notification and invite state
- **AND** the notification does not expose private invite details to anonymous visitors or public navigation surfaces
