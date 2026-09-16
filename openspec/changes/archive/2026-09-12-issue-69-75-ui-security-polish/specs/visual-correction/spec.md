## ADDED Requirements

### Requirement: Home supporting layouts use proportionate widths

The Home page MUST give its featured tournament and final action areas a
proportionate layout that follows the surrounding page rhythm rather than
making content feel unnecessarily boxed or vertically off-center.

#### Scenario: Visitor views featured tournaments

- **WHEN** the Home featured tournament section renders at a desktop or tablet
  width
- **THEN** the lead presentation MUST use a natural narrower measure relative
  to the page and nearby sections
- **AND** the content MUST retain its adjacent tournament list, image,
  identity, status, and detail action without an oversized enclosing frame

#### Scenario: Visitor views Join the LAN and Contact

- **WHEN** the final Home action row renders at a desktop or tablet width
- **THEN** the Contact area MUST receive more horizontal space than Join the LAN
- **AND** the two areas MUST remain visually balanced and aligned

#### Scenario: Visitor views the final Home row on a narrow screen

- **WHEN** the viewport reaches a supported narrow breakpoint
- **THEN** Join the LAN and Contact MUST stack in reading order
- **AND** neither section MUST appear vertically off-center or create page-wide
  horizontal scrolling
