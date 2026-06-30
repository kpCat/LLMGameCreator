# Goal 059 Artifact Scope Report

- Scenario: goal-059-full-generator-variability-regression-matrix
- Declared gate: full_generator_variability_regression_matrix_verification required
- Allowed code root: src/LLMGameCreator.Application/Design/FullGeneratorVariabilityRegressionMatrix/
- Allowed tests root: tests/LLMGameCreator.Tests/Application/FullGeneratorVariabilityRegressionMatrix/
- Allowed product smoke: tests/LLMGameCreator.Tests/ProductSmoke/FullGeneratorVariabilityRegressionMatrixProductSmokeTests.cs
- Allowed artifact root: .llmgc/procedural/goal-059-full-generator-variability-regression-matrix/
- Narrow Unity allowance: unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
- Forbidden provider/network/LLM/RAG/media generation/runtime/schema/UI/generator-library changes: enforced by task scope and final artifact guard
