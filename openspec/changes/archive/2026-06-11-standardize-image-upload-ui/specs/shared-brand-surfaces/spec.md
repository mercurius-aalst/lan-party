# shared-brand-surfaces Specification

## ADDED Requirements

### Requirement: Image upload fields use branded picker affordances
Image upload fields SHALL use the shared branded native upload picker treatment instead of exposing default browser file controls.

#### Scenario: A page renders an image upload field
- **WHEN** an admin or authenticated user can upload an image such as a sponsor logo, game image, or team logo
- **THEN** the upload field MUST show the branded dashed picker surface with an image icon and action label
- **AND** the native browser file input MUST NOT visually overlap or obscure the branded picker
- **AND** the selected filename or empty-file state MUST remain visible near the picker
- **AND** existing upload validation, accepted image types, and form binding behavior MUST remain intact
