# Codex Goal 006 - Semantic-Selected Generated Package And Runtime Composition

## Execute

```text
docs/GOAL_006_SEMANTIC_SELECTED_RUNTIME_COMPOSITION.md
```

## Mandatory Starting Evidence

Start only if the user prompt explicitly contains:

```text
semantic_guided_composition_artifact_verification passed.
```

## Hard Limits

- Complete only S059-S063.
- Do not create S064.
- Do not start Goal 007.
- Do not add Runtime Preview UI work.
- Do not add RAG, LLM/provider execution or arbitrary Lua execution.
- Do not add Unity/media work.
- Do not add genre/project/term-specific C# branches.
- Do not broadly redesign GamePackage or runtime contracts.
- Do not use independent Goal 004 runtime evidence as proof that semantic-selected ids executed.
- Do not use git commands.

## Context Budget

Read first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/GOAL_006_SEMANTIC_SELECTED_RUNTIME_COMPOSITION.md`

Read only directly required implementation files afterward. Read broad strategy/history only for a concrete unresolved boundary.

## Verification

Run focused tests per slice and one final full suite:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~SemanticRuntimeComposition|FullyQualifiedName~SemanticGuidedComposition|FullyQualifiedName~QuestDialogInteractionFamily|FullyQualifiedName~GeneratedPackageMvp|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario semantic-runtime-composition
.\.devflow\scripts\check-all.ps1
```

## Stop Condition

After S063:

- update state/docs;
- stop at `semantic_selected_runtime_composition_artifact_verification`;
- report the generated artifact folder;
- do not create S064 or begin another goal.

No application launch and no local LLM are required for this gate.
