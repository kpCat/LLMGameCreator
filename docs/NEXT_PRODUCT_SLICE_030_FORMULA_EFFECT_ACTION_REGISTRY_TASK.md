# Product Slice 030 Task: Formula/Effect/Action Registry Foundation

Status: current proposed Codex task  
Depends on: Product Slice 029 Seeded Procedural Game Kernel v1  
Primary outcome: deterministic validated runtime-facing rule-pack foundation  
Non-outcome: runtime loop, UI, Lua execution, provider execution, Unity export, media generation, broad GamePackage schema expansion

## Source-Of-Truth Reading Order

Read only these files before implementation unless a referenced code area requires local context:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
5. `docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md`
6. `docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md`
7. `docs/FULL_GAME_GENERATION_MASTER_PLAN.md`
8. `docs/GAME_SYSTEM_VARIANT_TAXONOMY.md`
9. Slice 029 procedural kernel files:
   - `src/LLMGameCreator.Application/Generation/Procedural/ProceduralGameKernelModels.cs`
   - `src/LLMGameCreator.Application/Generation/Procedural/ProceduralGameKernelService.cs`
   - `src/LLMGameCreator.Application/Generation/Procedural/ProceduralGamePlanMarkdownRenderer.cs`
10. Existing runtime/domain files only as needed to preserve future compatibility.

Do not read old root `README_APPLY_*` files, old `*_CODEX_PROMPT.md`, old `*_KILO_PROMPT.md`, old archive manifests, or old apply READMEs as current planning authority.

## Goal

Add a deterministic Application-layer Formula/Effect/Action Registry Foundation that converts Slice 029 procedural placeholders into a validated, runtime-facing rule-pack artifact.

The point of this slice is not to execute a game loop yet. The point is to create the data/rule layer that Slice 031 can consume to run a tiny generated runtime loop.

## Product Context

Slice 029 generated plans contain placeholders such as:

```text
requirement/open_route
requirement/faction_access
action/resolve_encounter
reward/quest_progress
```

Slice 030 must replace "placeholder only" with deterministic rule definitions:

```text
generated plan
-> placeholder extraction
-> formula/requirement/effect/action/event rule-pack
-> validation diagnostics
-> deterministic JSON/Markdown sidecars
```

## Required Behavior

Create an Application-layer service that accepts:

- a `ProceduralGeneratedGamePlan` or a request containing the plan plus optional generation settings;
- selected placeholder ids from the plan;
- optional strict mode flag, defaulting to non-throwing diagnostics;
- optional project folder for sidecar writes.

The service must produce a deterministic rule pack containing at least:

- metadata:
  - rule-pack schema version;
  - rule-pack id;
  - source generated plan id/hash;
  - deterministic hash;
  - stable summary;
- formulas;
- requirements;
- effects;
- actions;
- event rules;
- diagnostics;
- Markdown summary.

Same input must produce byte-stable JSON and Markdown.

Different source plans or seeds should produce the same schema but different source metadata and affected refs where appropriate.

## Minimum Rule Pack Shape

Prefer Application-side models first. Do not change GamePackage schema in this slice unless absolutely necessary and explicitly approved.

Suggested model family:

```text
FormulaEffectActionRulePack
FormulaDefinition
RequirementDefinition
EffectDefinition
ActionDefinition
EventRuleDefinition
FormulaEffectActionDiagnostic
FormulaEffectActionRegistryService
FormulaEffectActionRulePackValidator
FormulaEffectActionRulePackMarkdownRenderer
```

Exact names may follow local repository style.

## Minimum Built-In Rules

Create deterministic built-in mappings for Slice 029 placeholders.

### `requirement/open_route`

Purpose:

```text
Check whether a region connection can be traversed.
```

Required shape:

- requirement id: `requirement/open_route`
- references generated connection ids or accepts connection refs;
- has formula or predicate slot;
- validates referenced region/connection ids when source plan is supplied.

### `requirement/faction_access`

Purpose:

```text
Check whether a faction relationship/reputation/story access gate allows an action.
```

Required shape:

- requirement id: `requirement/faction_access`
- references faction ids from the generated plan;
- has formula or predicate slot;
- validates referenced faction ids.

### `action/resolve_encounter`

Purpose:

```text
Resolve an encounter into explicit effects.
```

Required shape:

- action id: `action/resolve_encounter`
- references generated encounter ids;
- applies at least one effect such as reward item/resource, reputation delta, status/event flag or encounter-resolved flag;
- no runtime execution yet.

### `reward/quest_progress`

Purpose:

```text
Advance quest/event state and grant a resource or item.
```

Required shape:

- action/reward id: `reward/quest_progress`
- references generated quest/event ids;
- applies at least one effect such as quest-progress flag and reward item/resource;
- no runtime execution yet.

## Formula Safety

Do not introduce a broad scripting language.

Allowed formula foundation:

- formula id;
- expression string;
- declared variables;
- declared numeric bounds or result type;
- deterministic validation of allowed characters/tokens;
- reference validation against declared variables;
- no arbitrary code execution;
- no reflection;
- no Lua;
- no dynamic C# compilation;
- no external expression-engine dependency unless already present and clearly suitable.

Minimum formulas may be simple and conservative, for example:

```text
formula/route_access_score
formula/faction_access_score
formula/encounter_reward_count
formula/quest_reward_count
```

The validator must reject:

- unknown variable refs;
- empty expression;
- unsafe characters;
- path-looking or code-looking text;
- formulas longer than a reasonable bound;
- duplicate ids.

It is acceptable if this slice validates formulas without evaluating them. Actual runtime evaluation may be part of Slice 031 if needed.

## Requirement / Effect / Action Safety

The rule pack must validate:

- duplicate ids;
- unsafe ids;
- unknown formula refs;
- unknown requirement refs;
- unknown effect refs;
- unknown action refs;
- refs to missing generated plan ids when a source plan is supplied;
- event rules pointing at missing actions or requirements;
- empty action effect lists;
- empty event trigger ids.

Allowed effect/action types should be explicit strings or enums. Keep the list small.

Suggested initial types:

```text
requirement/open_route
requirement/faction_access
effect/set_flag
effect/grant_item
effect/adjust_reputation
effect/advance_quest_event
action/resolve_encounter
action/grant_quest_progress
event_rule/on_enter_region
event_rule/on_resolve_encounter
event_rule/on_complete_quest_event
```

Use existing local naming/style if similar types already exist.

## Output Artifacts

When a project folder is supplied, write deterministic sidecars:

```text
.llmgc/procedural/formula-effect-action-rule-pack.json
.llmgc/procedural/formula-effect-action-rule-pack.md
.llmgc/procedural/formula-effect-action-validation-report.json
.llmgc/procedural/formula-effect-action-validation-report.md
```

The artifacts must not contain:

- timestamps;
- absolute paths;
- machine names;
- nondeterministic ordering;
- culture-dependent formatting.

Use UTF-8 without BOM if local patterns support it.

## Integration With Slice 029

Add a focused path that proves:

```text
ProceduralGameKernelService.Generate(...)
-> FormulaEffectActionRegistryService.Generate(...)
-> deterministic rule-pack artifacts
```

Do not mutate the generated plan.

Do not mutate `GamePackage`.

Do not add runtime execution yet.

## Important Design Constraints

- No LLM calls.
- No provider calls.
- No Unity work.
- No media generation.
- No Lua execution.
- No C# code generation.
- No WinForms UI.
- No semantic catalog approval UI.
- No archive review/manual import polish.
- No broad template-family work.
- No GamePackage schema change unless absolutely required and explicitly approved.
- No runtime command/state contract changes unless absolutely required and explicitly approved.

## Preferred Implementation Area

Prefer a new focused Application area near Slice 029, for example:

```text
src/LLMGameCreator.Application/Generation/Procedural/
```

or:

```text
src/LLMGameCreator.Application/Generation/Rules/
```

Use local repository style.

Keep the service split enough that Slice 031 does not have to extend one giant class:

- models;
- service/generator;
- validator;
- markdown renderer;
- writer if local style uses one.

Avoid adding abstractions unless they remove real complexity.

## Validation Requirements

Add focused tests for:

- same generated plan produces byte-identical rule-pack JSON and Markdown;
- rule pack contains formulas, requirements, effects, actions and event rules;
- every Slice 029 placeholder is mapped or diagnosed;
- generated rule refs point to existing formulas/requirements/effects/actions;
- generated plan refs point to existing regions/factions/encounters/quest-events/items where applicable;
- duplicate ids are rejected by validator;
- unsafe ids are rejected by validator;
- invalid formula expressions are rejected by validator;
- missing source-plan refs produce diagnostics instead of throwing;
- no LLM/provider/Lua/Unity/media/runtime execution is invoked.

Add one product smoke scenario:

```text
formula-effect-action-registry
```

The smoke should prove:

- Slice 029 plan generation still works;
- Slice 030 rule-pack generation works from that plan;
- all four expected sidecar files are written;
- repeated generation is deterministic;
- validation report is present and readable.

## Acceptance Criteria

The slice is complete only when:

- deterministic rule-pack JSON and Markdown are produced;
- deterministic validation-report JSON and Markdown are produced;
- every Slice 029 built-in placeholder has a rule mapping or explicit diagnostic;
- formulas/requirements/effects/actions/event rules are validated;
- tests cover deterministic repeatability and invalid rule cases;
- product smoke `formula-effect-action-registry` passes;
- `README.md`, `docs/CONTEXT_INDEX.md`, `docs/CURRENT_GENERATOR_STATE.md`, and `docs/CURRENT_GENERATOR_STATE.json` continue to point away from infrastructure-only work;
- `CURRENT_GENERATOR_STATE.md` and `.json` recommend Slice 031: Tiny Generated Runtime Loop;
- no UI/provider/LLM/Lua/Unity/media/runtime execution is added.

## Stop Conditions

Stop and report instead of implementing if:

- implementation would require GamePackage schema changes;
- implementation would require runtime command/state contract changes;
- implementation would require a full expression evaluator or broad DSL interpreter;
- implementation would require new third-party dependencies;
- existing Slice 029 generated plan cannot be used as source input;
- the task starts drifting into UI, archive review, manual import, provider, Unity, Lua execution, media generation or broad generator-family work.

## Final Report Requirements

The final report must include:

- changed files;
- generated rule-pack artifact paths;
- generated validation-report artifact paths;
- tests/smokes run;
- confirmation that no LLM/provider/Lua/Unity/media/runtime execution was added;
- confirmation that Slice 031 remains viable without redesign;
- explicit note if full `check-all.ps1` was not run.
