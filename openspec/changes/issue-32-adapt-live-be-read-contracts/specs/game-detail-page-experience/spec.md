## MODIFIED Requirements

### Requirement: Game detail page highlights at most one visitor-facing tournament partner
The game detail page SHALL present at most one visitor-facing sponsor or partner highlight on the public tournament page from the live singular sponsor placement response.

#### Scenario: Tournament has a sponsor placement
- **WHEN** the tournament detail page receives a non-null sponsor placement
- **THEN** the page displays a single featured tournament partner surface
- **AND** the page does not render multiple separate sponsor context sections for the same tournament page

#### Scenario: Tournament has no featured partner
- **WHEN** the tournament detail page receives no sponsor placement
- **THEN** the featured partner surface is omitted
- **AND** the rest of the tournament page continues to render normally

#### Scenario: Mock mode returns sponsor placement data
- **WHEN** mock backend mode provides tournament sponsor data for the game detail page
- **THEN** the mock game detail response exposes no more than one sponsor placement for visitor-facing rendering
- **AND** the page behavior remains consistent with the live singular sponsor placement contract
