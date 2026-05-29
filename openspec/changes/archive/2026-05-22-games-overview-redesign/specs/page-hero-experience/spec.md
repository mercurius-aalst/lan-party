## MODIFIED Requirements

### Requirement: Games overview page opens with simple visitor-facing orientation

The games overview page SHALL open with a browse-first orientation surface instead of a dedicated page intro or dashboard-style hero treatment.

#### Scenario: Visitor opens games overview
- **WHEN** a visitor opens the games overview page
- **THEN** the page heading is presented together with tournament browsing controls in the first interactive surface
- **AND** visitors can begin searching, sorting, or filtering tournaments without scrolling past a separate hero or intro block

#### Scenario: Games entry avoids dashboard-first framing
- **WHEN** the games overview page is rendered
- **THEN** internal-style event snapshot metrics are not the primary supporting content at the top of the page
- **AND** the page emphasizes immediate tournament discovery actions over a standalone orientation panel
