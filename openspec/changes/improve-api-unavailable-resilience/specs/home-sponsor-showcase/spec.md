## ADDED Requirements

### Requirement: Homepage sections fail independently

The home page MUST preserve successfully loaded tournament or sponsor content when the other independent API-backed section fails.

#### Scenario: Homepage receives a partial API failure
- **WHEN** either tournament or sponsor loading fails while the other succeeds
- **THEN** the successful content remains visible
- **AND** the failed section shows a friendly unavailable state or is safely omitted without blocking the page shell
