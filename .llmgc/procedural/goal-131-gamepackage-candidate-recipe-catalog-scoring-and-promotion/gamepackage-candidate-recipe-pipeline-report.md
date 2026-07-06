# Goal 131 GamePackage Candidate Recipe Catalog Scoring and Promotion

- recipePipelineStatus: GREEN
- recipeCount: 4
- candidateCount: 4
- passedCandidates: 4
- failedCandidates: 0
- matrixPassed: true
- selectedCandidateId: minimal-map-game-balanced-baseline
- selectedCandidateScore: 100
- selectedCandidatePackagePath: .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/package.json
- normalCommand: .devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd
- recipeCatalogPath: .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/candidate-recipe-catalog.json
- pipelineResultPath: .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-recipe-pipeline-result.json
- scoringResultPath: .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/candidate-scoring-result.json
- manualUnityOptional: true
- samplePackageUnmodified: true
- projectionOnly: true
- metadataOnlyRecipeMutation: true

## Scope

- Recipes are deterministic repo-local input.
- Candidate packages and selected candidate stay under Goal131 artifacts.
- The matrix result is produced by the existing Goal129 runner over the generated recipe index.
- Manual Unity inspection remains optional.
