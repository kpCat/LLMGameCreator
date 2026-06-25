# Codex Goal 005 - Layered Semantic Packs And Semantic-Guided Composition

## Command

Execute:

```text
docs/GOAL_005_LAYERED_SEMANTIC_PACKS_AND_GUIDED_COMPOSITION.md
```

## Mandatory Starting Evidence

Start only when the user prompt explicitly includes the Goal 004 acceptance evidence required by the goal document.

The evidence is based on deterministic artifact review. Do not require a WinForms run.

## Hard Limits

- Complete only S054-S058.
- Do not create S059.
- Do not add Runtime Preview features.
- Do not add a semantic editor UI or RAG/vector database.
- Do not download or vendor external knowledge datasets.
- Do not run LLM/provider/Lua/Unity/media execution.
- Do not redesign GamePackage or public runtime contracts.
- Do not use git commands.
- Reuse the existing semantic catalog and Goal 004 family seams.
- Do not grow `QuestDialogInteractionFamilyAcceptanceService` with substantial new semantic logic.
- Prefer compact data packs, compiler/validator services, narrow adapters and headless acceptance.

## Context Budget Rule

Read first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/GOAL_005_LAYERED_SEMANTIC_PACKS_AND_GUIDED_COMPOSITION.md`

Read `docs/CURRENT_GENERATOR_STATE.md` only for S054/S058 state changes.

Read broad semantic/architecture docs only when the implementation needs a stated boundary. Do not read historical task packs or reports by default.

## Verification

Run focused tests per slice. Run the full suite once at final acceptance unless earlier risk/failure requires it.

Final commands:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~SemanticCatalog|FullyQualifiedName~SemanticGuided|FullyQualifiedName~QuestDialogInteractionFamily|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario semantic-guided-composition
.\.devflow\scripts\check-all.ps1
```

## Stop Condition

After S058:

- update the state pair;
- stop at `semantic_guided_composition_artifact_verification`;
- report the generated artifact folder;
- do not start another goal;
- do not create S059.

No application launch and no local LLM are required for this gate.
