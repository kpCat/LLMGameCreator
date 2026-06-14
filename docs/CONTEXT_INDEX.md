# CONTEXT_INDEX.md

Purpose: reduce repeated orientation cost for Codex/LLM agents.

Read this file after `AGENTS.md` when a task touches code. This file is a routing index, not a replacement for detailed docs. If this file conflicts with a more specific doc, the specific doc wins.

## Full generator source-of-truth docs

Read these before broad generation, capability, prompt, Lua integration, artifact-contract, roadmap or Codex-task-shaping work:

| Document | Use when |
|---|---|
| `docs/FULL_GAME_GENERATION_MASTER_PLAN.md` | Defining what full game generation means, target architecture, lifecycle, milestones and done criteria. |
| `docs/GAME_GENERATION_CAPABILITY_MATRIX.md` | Choosing or scoping game capability domains, priorities, current status, required contracts, validators and acceptance criteria. |
| `docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md` | Checking C# / LLM / Lua ownership, forbidden outputs/features, promotion rules and repair boundaries. |
| `docs/PROMPT_AND_ARTIFACT_CONTRACT_HARDENING.md` | Designing strict prompts, context packs, repair prompts, artifact envelopes and validation gates. |
| `docs/ROADMAP_TO_FULL_GENERATOR.md` | Planning executable Codex milestones from the current vertical slice to a full generator. |
| `docs/CODEX_EXECUTION_DOCTRINE.md` | Deciding whether a future Codex task is worth doing and what boundaries/final report it must follow. |
| `docs/GAME_FORM_FACTORS_AND_PRESENTATION_MODES.md` | Choosing explicit presentation modes, view models, asset modes and pseudo-3D/first-person-grid targets. |
| `docs/GAME_SYSTEM_VARIANT_TAXONOMY.md` | Choosing world, actor, inventory, equipment, interaction, combat, progression, pathfinding and NPC behavior ids. |
| `docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md` | Reviewing staged draft artifacts, applying approve/reject/repair decisions and rebuilding approved artifact sets without package mutation. |
| `docs/CHARACTER_CARD_AND_ACTOR_MODEL_CONTRACTS.md` | Planning character card, party roster and actor model profile contracts. |
| `docs/WORLD_TOPOLOGY_AND_CHUNKING_CONTRACTS.md` | Planning finite maps, regions, first-person grid dungeons, seamless/infinite chunks and runtime chunk deltas. |
| `docs/INTERACTION_COMBAT_PROGRESSION_VARIANTS.md` | Planning interaction, combat, progression, inventory and equipment contract families. |

The current one-click package export flow is a narrow vertical MVP. It proves the approved-artifact-set -> GamePackage assembly -> package export path, but it is not the full generator.

Machine-readable atlas seeds for these variant docs live under `generator-library/atlas/`:

- `game_form_factor_taxonomy.json`;
- `game_system_variant_taxonomy.json`;
- `character_actor_contracts.json`;
- `world_topology_contracts.json`;
- `interaction_combat_progression_contracts.json`.

## Project map

| Project / folder | Responsibility | Read when |
|---|---|---|
| `src/LLMGameCreator.Domain/` | Data contracts: game definitions, assets, scripting definitions, validation primitives. No IO, no UI. | Any model, validator, runtime, package, Lua, asset task. |
| `src/LLMGameCreator.GamePackage/` | Root `GamePackageDefinition` and package path conventions. | Package format, loading/saving, validators, runtime startup. |
| `src/LLMGameCreator.Runtime.Abstractions/` | Runtime command/state/event interfaces. Frontends should talk through these contracts. | Runtime, Unity bridge, WinForms preview, command/event work. |
| `src/LLMGameCreator.Runtime/` | Headless runtime implementation. No WinForms, no LLM, no ComfyUI. | Movement, interaction, command execution, state updates. |
| `src/LLMGameCreator.Scripting/` | Script engine abstraction and null implementation. No real Lua execution yet unless task explicitly says so. | Lua engine planning, script execution contracts. |
| `src/LLMGameCreator.Generation/` | LLM authoring/generation models. Editor-side only. | ContextPack, GenerationJob, LLM provider tasks. |
| `src/LLMGameCreator.AssetPipeline/` | Asset generation provider abstractions and jobs. Editor-side only. | ComfyUI/Fooocus/manual asset workflow tasks. |
| `src/LLMGameCreator.Application/` | Use-cases/services: settings, current package, validation. UI should call this layer, not storage/runtime directly when avoidable. | Application services, validators, editor workflows. |
| `src/LLMGameCreator.Infrastructure/` | JSON storage, settings persistence, file logging. No UI logic. | Storage, serialization, app settings, logging. |
| `src/LLMGameCreator.WinForms/` | Editor shell and pages. Designer layout in `*.Designer.cs`, logic in `*.cs`. | UI page work, preview page, settings/projects/assets pages. |
| `tests/LLMGameCreator.Tests/` | Smoke/contract/regression tests. Keep tests useful and small. | Any behavior/validator/runtime change. |
| `generator-library/` | Lua generator/capability library assets and manifests. Imported as metadata only; Lua is not executed by the registry. | Generator library, capability registry, manifest import tasks. |
| `samples/minimal-map-game/` | Minimal GamePackage sample. Should stay valid. | Package, validation, runtime, Lua/asset examples. |
| `templates/lua-stdlib/` | Shared Lua helper library baseline. | Lua authoring/sandbox/API tasks. |
| `templates/lua-blueprints/` | Reusable Lua blueprint examples for LLM-assisted game creation. | Lua generation task design. |
| `docs/` | Architecture and task guidance. | Read only relevant docs; do not read the whole folder for tiny fixes. |

## High-value local patterns

### Validator pattern

Primary files:
- `src/LLMGameCreator.Application/Validation/GamePackageValidator.cs`
- `src/LLMGameCreator.Domain/Validation/ValidationIssue.cs`
- `tests/LLMGameCreator.Tests/SmokeTests.cs`

Current style:
- `GamePackageValidator.Validate(package)` remains backward-compatible.
- Folder-aware validation uses `Validate(package, projectFolder)`.
- Add issues via private `Add(report, code, severity, message, targetId)`.
- Prefer stable machine-readable issue codes such as `script.path.missing`.
- Keep validation deterministic and side-effect free.
- Folder-aware file existence checks may check `File.Exists`, but should not read or execute content unless task explicitly says so.

### Storage pattern

Primary files:
- `src/LLMGameCreator.Infrastructure/Storage/JsonGamePackageRepository.cs`
- `src/LLMGameCreator.Infrastructure/Storage/SqliteDesignDatabase.cs`
- `src/LLMGameCreator.Infrastructure/Storage/SqliteDesignSchema.cs`
- `src/LLMGameCreator.Infrastructure/Storage/JsonAppSettingsRepository.cs`
- `src/LLMGameCreator.Application/Abstractions/Repositories.cs`
- `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`

Current style:
- System.Text.Json.
- camelCase JSON.
- indented output.
- case-insensitive read.
- `JsonStringEnumConverter` for enum strings.
- `package.json` is currently the loaded source for `GamePackageDefinition`.
- Do not introduce additional package files without an explicit task.
- Design DB is editor-side SQLite under `.llmgc/design.db`; it stores design and generator registry metadata, not runtime package content.

### Generator library registry pattern

Primary files:
- `docs/DESIGN_DB_AND_GENERATOR_REGISTRY.md`
- `src/LLMGameCreator.Application/Design/`
- `src/LLMGameCreator.Infrastructure/Storage/SqliteDesignDatabase.cs`
- `src/LLMGameCreator.WinForms/Pages/GeneratorLibrary/`
- `tests/LLMGameCreator.Tests/Design/GeneratorLibraryRegistryTests.cs`

Current style:
- Import only `generator-library/manifests/*.manifest.json`.
- Store module/capability metadata, declared paths, and diagnostics.
- Create saved draft `GeneratorPlan` records, then revalidate and update lifecycle status through Application services before any future execution/apply work.
- Compile approved plans into Design DB preview artifacts through `IGeneratorPlanPreviewService`; previews store staged audit JSON and validation rows only.
- Run `IGeneratorLibraryIntegrityValidator` to check physical files, canonical manifest fields, aliases, duplicate ids, batch reports, and root leakage before pushing new Lua batches.
- Do not execute Lua, load arbitrary code, generate Unity code, or change GamePackage format.
- Unknown manifest fields go to `metadata_json`.
- WinForms tabbed pages split each tab into a dedicated `UserControl`.
- Data-only GamePackage apply work uses `game_package_patch_v1` generated artifacts in the Design DB. Read `src/LLMGameCreator.Application/Design/GamePackagePatchService.cs`, `docs/DESIGN_DB_AND_GENERATOR_REGISTRY.md`, current package services, and package validators; do not add Lua/module/LLM/Unity/codegen execution.
- Artifact review uses `GeneratorPlanDraftArtifactReviewService` over the existing draft artifact staging artifacts. It updates persisted staging decisions, validation rows and the approved artifact set, but does not mutate `GamePackage`, export packages, call providers or execute Lua. Read `docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md` for this workflow.
- Patch-capable planning uses optional `config.package_operations` inside `GeneratorPlan` steps. Read `GeneratorPlanDraftService`, `GeneratorPlanValidator`, `GamePackagePatchOperationValidator`, `GeneratorPlanPipelineService`, and `GamePackagePatchService`. Plans may propose allowlisted data-only operations, but approval, preview, patch extraction, dry-run and apply are owned by C# services.
- Pipeline orchestration is a convenience layer only: approved plan -> preview artifact -> patch artifact -> dry-run diff. Prepare must not mutate package state and must not auto-apply.
- The older First Playable Slice `ApplyDraft` path is legacy direct apply. Treat it cautiously because it does not use `game_package_patch_v1` rollback/audit artifacts; prefer the Generator Library safe patch pipeline for new creator flows.
- Prototype Lua execution is implemented only for typed `data:extend(...)` declarations in `src/LLMGameCreator.Scripting/`. Read `PrototypeLuaExecutor`, `PrototypeLuaStaticAnalyzer`, `PrototypeLuaDeclarationMapper`, `PrototypeLuaPatchArtifactService`, and `GamePackagePatchOperationValidator`. It captures declarations and creates `game_package_patch_v1` artifacts; runtime/generator/behavior/interaction/formula/event Lua and generator modules are still not executed.
- Core gameplay economy contracts live in `src/LLMGameCreator.Domain/Definitions/EconomyDefinitions.cs` and are validated by the Application validation subsystem. Read `docs/GAME_SYSTEMS_MODEL.md`, `docs/GAME_PACKAGE_FORMAT.md`, `GamePackagePatchOperationValidator`, `GamePackagePatchService`, and `PrototypeLuaDeclarationMapper` when touching resources, requirements/costs/outputs, recipes, loot, transactions, resource networks/nodes, inventories or item economy metadata.
- Gameplay runtime v1 lives in `src/LLMGameCreator.Runtime/` and extends `src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs` with `GameRuntimeState`, `GameRuntimeCommand`, `GameRuntimeEvent` and `IGameRuntimeService`. It executes requirements/costs/outputs, crafting, deterministic loot, transactions and simple resource node ticks without Lua, generator modules, LLM calls, Unity or package-definition mutation.
- Unified runtime bridge v1 lives in the same runtime contracts/implementation area and adds `UnifiedRuntimeSession`, `IUnifiedGameRuntimeService`, `UseItem`, `ExecuteInteraction` and `IRuntimeStateSerializer`. Read `DefaultGameRuntime`, `GameRuntimeService`, `UnifiedGameRuntimeService`, `UseItemRuntimeService`, `InteractionRuntimeService`, `OutputApplier`, Runtime Simulator and runtime tests when touching frontend runtime bridge behavior.
- Exploration inventory runtime v1 adds equipment, container transfer, harvesting and runtime snapshot files. Read `EquipmentRuntimeService`, `ContainerRuntimeService`, `HarvestRuntimeService`, `RuntimeSnapshotStore`, `CostConsumer`, `InteractionRuntimeService`, `RuntimeSimulatorPageControl` and focused runtime tests when touching equipment slots, tool durability/charge, container inventories, harvest metadata or `.llmgc/runtime-saves`.
- Encounter/combat runtime v1 adds `StatDefinition`, `ProgressionDefinition`, `EncounterDefinition`, `EncounterRuntimeState`, `EncounterRuntimeService` and `EncounterAiService`. Read `EncounterDefinitions.cs`, `RuntimeContracts.cs`, `EncounterRuntimeService.cs`, `OutputApplier.cs`, `EncounterDefinitionValidator.cs`, patch/Prototype Lua mapper files and focused encounter tests when touching stats, progressions, combat abilities, turn order, rewards or runtime simulator encounter commands.
- Narrative runtime v1 adds optional quest/dialogue/faction contracts, `QuestRuntimeService`, `DialogueRuntimeService`, `FactionRuntimeService`, `QuestObjectiveTracker`, narrative commands/events and Runtime Simulator diagnostics. Read `NarrativeDefinitions.cs`, `ContentDefinitions.cs`, `DialogueDefinitions.cs`, `RuntimeContracts.cs`, narrative runtime services, `NarrativeDefinitionValidator`, patch/Prototype Lua mapper files and focused narrative tests when touching quests, dialogue graph choices, faction reputation, objective tracking or talk/start_quest/complete_quest interactions.

### Runtime command pattern

Primary files:
- `src/LLMGameCreator.Runtime.Abstractions/`
- `src/LLMGameCreator.Runtime/DefaultGameRuntime.cs`
- `src/LLMGameCreator.WinForms/Pages/RuntimePreview/`

Current style:
- Frontend creates `PlayerCommand`.
- Runtime accepts `GamePackageDefinition + GameState + PlayerCommand`.
- Runtime returns `CommandResult` with updated state and events.
- Rendering does not mutate `GameState`.
- Runtime does not call LLM, ComfyUI, WinForms, or external generators.

### WinForms page pattern

Primary files:
- `src/LLMGameCreator.WinForms/MainForm.cs`
- `src/LLMGameCreator.WinForms/CompositionRoot.cs`
- existing page folders under `src/LLMGameCreator.WinForms/Pages/`
- `docs/WINFORMS_DESIGNER_RULES.md`

Current style:
- Each page is a `UserControl` implementing `IEditorPage`.
- Register pages in `CompositionRoot`.
- `MainForm` is shell/navigation/status only.
- Visual layout goes to `InitializeComponent()` inside `*.Designer.cs`.
- Dependencies/events/runtime logic stay in `*.cs`.
- Designer files should be conservative and Visual Studio Designer-friendly.
- Provide a design-time safe path/parameterless constructor when needed.

## Read sets by task type

Use these as default reading routes. Add files only when the task clearly needs them.

### Validation task

Read:
- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/VALIDATION_STRATEGY.md`
- relevant domain definition files
- `src/LLMGameCreator.Application/Validation/GamePackageValidator.cs`
- `tests/LLMGameCreator.Tests/SmokeTests.cs`
- `samples/minimal-map-game/package.json`

Do not read:
- WinForms Designer files unless UI validation display is part of the task.
- Lua blueprints unless validating Lua-specific fields.

### Lua task

Read:
- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/LUA_SCRIPTING.md`
- `docs/LUA_STANDARD_LIBRARY.md`
- `docs/LUA_BLUEPRINT_CATALOG.md`
- `docs/LUA_BLUEPRINT_EXPANSION.md`
- `docs/SCRIPT_MANIFEST_SPEC.md`
- `src/LLMGameCreator.Domain/Definitions/ScriptingDefinitions.cs`
- `src/LLMGameCreator.Scripting/`
- relevant `templates/lua-stdlib/` or `templates/lua-blueprints/` files

Do not:
- add real Lua engine unless task explicitly says so.
- execute Lua in validator tasks.

### Asset task

Read:
- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/ASSET_PIPELINE.md`
- `docs/ASSET_CONTRACT_SPEC.md`
- `docs/ASSET_WORKFLOW_PROFILES.md`
- `src/LLMGameCreator.Domain/Definitions/AssetDefinitions.cs`
- `src/LLMGameCreator.AssetPipeline/`
- `src/LLMGameCreator.Application/Validation/GamePackageValidator.cs`
- `samples/minimal-map-game/package.json`

Do not:
- call ComfyUI/Fooocus unless the task is specifically an integration task.
- make runtime depend on asset generation providers.

### Generation/LLM task

Read:
- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/GENERATION_PIPELINE_DETAILED.md`
- `docs/LIMIT_BUDGET_AND_GOALS.md`
- `src/LLMGameCreator.Generation/`
- relevant Domain/GamePackage definitions

Do not:
- make runtime call LLM.
- pass full project context to a generated job unless the task explicitly asks to design such an export/debug view.

### WinForms page task

Read:
- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/WINFORMS_DESIGNER_RULES.md`
- `src/LLMGameCreator.WinForms/Pages/IEditorPage.cs`
- `src/LLMGameCreator.WinForms/CompositionRoot.cs`
- 1-2 existing page controls with Designer files
- target page files

Do not:
- put layout into constructor.
- put dependencies into Designer files.
- add pages without registration in `CompositionRoot`.

### Runtime task

Read:
- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/RUNTIME_MODEL.md`
- `docs/UNITY_PLAYER_CONTRACT.md` if frontend compatibility matters
- `src/LLMGameCreator.Runtime.Abstractions/`
- `src/LLMGameCreator.Runtime/DefaultGameRuntime.cs`
- runtime tests

Do not:
- make rendering mutate state.
- add UI dependencies.
- call LLM or asset providers.

### Storage task

Read:
- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/GAME_PACKAGE_FORMAT.md`
- `src/LLMGameCreator.Application/Abstractions/Repositories.cs`
- `src/LLMGameCreator.Infrastructure/Storage/`
- `src/LLMGameCreator.GamePackage/`
- relevant tests and sample package

Do not:
- change package format without a format-version/migration decision.
- introduce SQLite unless explicitly requested.

## Red flags

Stop and ask/plan first if a change would:
- touch more than 8-10 files;
- add a new project;
- add Unity, Lua engine, SQLite, ComfyUI/Fooocus, or real LLM provider;
- change `package.json` structure;
- change public runtime command/state contracts;
- modify WinForms Designer files and runtime logic in the same task;
- add many tests unrelated to the acceptance criteria.
