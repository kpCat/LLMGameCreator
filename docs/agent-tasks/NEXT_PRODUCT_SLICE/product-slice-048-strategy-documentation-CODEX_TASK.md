# Product Slice 048 - Strategy Documentation Consolidation

## Goal

Consolidate the strategic architecture decisions after Goal 003 so future work does not drift into endless Runtime Preview polish, infrastructure-only slices, or C# per mechanic.

This is a documentation-only slice unless a tiny docs index update is required.

## Inputs

Read first:

- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/CONTEXT_INDEX.md
- docs/EXTENSION_RULE_PACK_CONTRACT_V1.md
- docs/ROADMAP_TO_FULL_GENERATOR.md if it exists
- docs/MANUAL_EXTENSION_SPINE_VERIFICATION.md

Also add or update the docs from this task pack:

- docs/ARCHITECTURE_STRATEGY_AND_BOUNDARIES.md
- docs/SEMANTIC_PACK_AND_RAG_STRATEGY.md
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/OPEN_DESIGN_QUESTIONS.md

## Required Behavior

1. Add the strategy documents exactly where appropriate under docs/.
2. If docs/ROADMAP_TO_FULL_GENERATOR.md already exists, merge instead of overwriting useful current content.
3. Update docs/CONTEXT_INDEX.md so these strategy docs are discoverable.
4. Update README.md only if it has a compact "where to start" docs list.
5. Update docs/CURRENT_GENERATOR_STATE.md and docs/CURRENT_GENERATOR_STATE.json to mention that strategic docs were consolidated after Goal 003.
6. Do not create S049.
7. Do not change runtime, UI, generators, validators, rule-pack code, tests, csproj, sln, provider, Lua execution, Unity, or media code.

## Forbidden

- No gameplay changes.
- No Runtime Preview changes.
- No new services.
- No broad rewrite of existing docs.
- No manual verification gate changes except documenting the current process policy.
- No git commands.

## Validation

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

If docs tests require exact references, update them narrowly.

Then run:

```powershell
.\.devflow\scripts\check-all.ps1
```

If check-all is too expensive or blocked, report why and run the focused docs tests at minimum.

## Acceptance

- Strategy docs exist and are linked.
- Current state docs parse.
- No code behavior changed.
- No S049 created.
- Final report includes changed files and tests run.
