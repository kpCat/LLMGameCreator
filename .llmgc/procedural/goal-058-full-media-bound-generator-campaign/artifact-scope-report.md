# Goal 058 Artifact Scope Report

- Scenario: goal-058-full-media-bound-generator-campaign
- Declared gate: full_media_bound_generator_campaign_verification required
- Allowed code root: src/LLMGameCreator.Application/Design/FullMediaBoundGeneratorCampaign/
- Allowed tests root: tests/LLMGameCreator.Tests/Application/FullMediaBoundGeneratorCampaign/
- Allowed product smoke: tests/LLMGameCreator.Tests/ProductSmoke/FullMediaBoundGeneratorCampaignProductSmokeTests.cs
- Allowed artifact root: .llmgc/procedural/goal-058-full-media-bound-generator-campaign/
- Narrow Unity allowance: unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
- Forbidden provider/network/LLM/RAG/media generation/runtime/schema/UI/generator-library changes: enforced by task scope and final artifact guard
