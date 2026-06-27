# Process Task: Modular Contract Goal Policy Adoption

## Starting gate

Start only after the user explicitly provides:

```text
rich_package_assembly_coverage_audit_verification passed
```

Goal 024 must already be accepted from pushed repository review.

## Final gate

Stop at exactly one final gate:

```text
modular_contract_goal_policy_adoption_verification
```

Leave this gate `required`, not `passed`.

This task is not Goal 025 implementation, not S199, not package assembly expansion and not a product feature. It is a bounded docs/process task before the next implementation direction.

## Purpose

Adopt the next process direction for LLMGameCreator:

```text
modular contracts
+ bounded composite goals
+ rare product vertical gates
+ campaign planning over several goals
+ one executable bounded goal at a time
```

This policy must reduce manual goal cycles. It must not create more default manual gates by splitting contract/module/integration/proof into separate goals.

## Key rule

Contract / Module / Integration / Proof are internal phases of one bounded composite goal by default.

Do not create separate default goals named:

- contract goal;
- module goal;
- integration goal;
- proof goal.

Split those phases into separate goals only when Codex finds one of these conditions:

- budget exceeded;
- scope creep;
- architecture risk;
- inability to prove the result honestly inside one bounded composite goal;
- user explicitly asks for a split.

## Read first

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CURRENT_GENERATOR_STATE.json`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/CODEX_EXECUTION_DOCTRINE.md`
8. `docs/CODEX_PATCH_RULES.md`
9. `docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md`
10. `docs/GOAL_024_RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT.md`
11. `docs/RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT_V1.md`
12. proposed drafts if they exist:
    - `README.proposed.md`
    - `MODULAR_CONTRACT_GOAL_POLICY.proposed.md`
    - `LLMGameCreator_FEATURE_BACKLOG_AUDIT.proposed.md`

Use proposed files as input drafts only. Do not copy them blindly. Adapt to repository style and current source-of-truth docs.

## Scope

Allowed:

- `README.md` cleanup.
- Create/adapt:
  - `docs/MODULAR_CONTRACT_GOAL_POLICY.md`
  - `docs/LLMGameCreator_FEATURE_BACKLOG_AUDIT.md`
  - `docs/PACKAGE_ASSEMBLY_EXPANSION_CAMPAIGN_PACK.md` if useful.
- Minimal references in:
  - `AGENTS.md`
  - `docs/CONTEXT_INDEX.md`
  - `docs/CODEX_PATCH_RULES.md`
  - `docs/CODEX_EXECUTION_DOCTRINE.md`
- State/routing docs:
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- Focused docs/state tests only if existing test structure requires update.

Forbidden:

- production code changes;
- `GamePackage` schema changes;
- runtime changes;
- Unity changes;
- WinForms UI changes;
- provider/media/RAG/LLM/Lua execution;
- package assembly implementation;
- Goal 025 implementation;
- S199;
- broad refactor;
- broad cleanup;
- deleting accepted historical artifacts;
- changing old goal artifacts outside the allowed current docs/process scope.

Budget:

- Prefer no more than 8 changed docs/files.
- If more than 10 files are required, stop and return a split plan before editing.
- No broad cleanup.
- No unrelated docs rewrite.

## Required work

### 1. README source-of-truth cleanup

`README.md` must be a stable project overview, not a current-goal handoff document.

Remove from README:

- concrete current phase;
- active goal;
- active manual gate;
- next practical step;
- stale current-phase links.

README must state:

- what LLMGameCreator is;
- `GamePackage` is the playable source of truth;
- runtime/player does not call LLM/RAG/providers/media tools;
- LLM is used only in editor/generation/authoring pipeline;
- current state must be read from:
  - `AGENTS.md`
  - `docs/CONTEXT_INDEX.md`
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`

If `README.proposed.md` is applied, do not leave it as a conflicting active document. Prefer removing it. If removal is not appropriate, clearly mark it as a temporary draft, but only if that keeps scope smaller.

### 2. Modular Contract Goal Policy

Create official policy:

```text
docs/MODULAR_CONTRACT_GOAL_POLICY.md
```

The policy must define task types:

- audit-only;
- contract-only;
- module implementation;
- integration slice;
- product vertical gate;
- bounded hotfix.

The policy must define proof levels:

- Level 0: docs/contract proof;
- Level 1: conformance proof;
- Level 2: module proof;
- Level 3: integration proof;
- Level 4: product vertical proof.

The policy must state the composite goal rule:

- one bounded composite goal may include Contract / Module / Integration / Proof phases;
- those phases are not separate goals by default;
- split is required only for budget, scope, architecture or proof risk.

The policy must state the product vertical gate rule:

- product vertical gate is not required in every goal;
- product vertical gates are rare and intentional;
- product vertical gate is used after multiple modules/integrations are ready and a new playable/simulatable/runtime-facing result must be proven.

The policy must state the anti-overfit rule:

- a foundational module cannot be accepted through one consumer scenario only;
- require at least one real consumer plus one second/synthetic future-consumer fixture;
- example: pathfinding cannot be proven only by caravan route; it also needs an `npc_city_walk` synthetic consumer fixture or another independent consumer shape;
- synthetic fixture does not need to implement the future system, but it must prove the output contract is not hardcoded to one scenario.

The policy must include goal budget limits:

- max implementation files;
- max docs/state files;
- max artifact family roots;
- max new production concepts;
- max focused tests;
- max hotfix attempts;
- stop conditions.

The hotfix policy must state:

```text
0 hotfixes = excellent
1 hotfix = normal
2 hotfixes = maximum
after the second hotfix, stop blind repair and return diagnosis/split plan
```

The policy must include Codex pre-final self-review protocol:

1. reread acceptance criteria;
2. map each criterion to concrete evidence path/test;
3. directly inspect artifacts, not only test output;
4. confirm final gate status;
5. confirm next goal/slice not started;
6. confirm forbidden files were not changed;
7. confirm public GamePackage schema did not change unless allowed;
8. confirm Unity/LLM/RAG/provider/media/Lua did not run unless allowed;
9. if evidence is missing, do one bounded self-fix;
10. if evidence is still missing after self-fix, stop and return diagnosis instead of claiming done.

The final Codex report must include:

```markdown
| Acceptance criterion | Evidence path/test | Status |
|---|---|---|
```

### 3. Explicit runtime LLM/RAG decision

Record in the policy docs that optional live runtime LLM/RAG is not a target mode for LLMGameCreator.

Runtime must not depend on live LLM/RAG.

Target approach for variable dialogue, quest text, descriptions, events and large/infinite worlds:

```text
compiled semantic catalog
+ seed
+ rule packs
+ phrase/dialogue/event grammar
+ deterministic runtime-safe variation
+ validation
+ save-compatible runtime deltas
```

Allowed:

- pre-runtime LLM/RAG as editor/generation authoring helper only.

Required:

- runtime consumes compiled/validated data, semantic catalog, rules, seeds and grammars;
- runtime variation is deterministic, reproducible and save-compatible.

Forbidden:

- planning live runtime LLM/RAG as an optional product path.

If proposed drafts describe optional live runtime LLM/RAG as a future product possibility, change that to explicit non-goal / forbidden runtime dependency.

### 4. Feature backlog audit

Create official backlog audit document if the proposed file exists or if it is useful:

```text
docs/LLMGameCreator_FEATURE_BACKLOG_AUDIT.md
```

This document is:

- not an implementation plan;
- not the active goal queue;
- not source of truth for current gate;
- backlog/audit of wanted capabilities so future wants are not lost.

It must state:

- wants are not removed;
- wants move into capability backlog / future gaps / campaign planning;
- implementation happens through contracts/modules/integration/product gates;
- live runtime LLM/RAG is not a wanted capability;
- semantic runtime variation must be deterministic.

### 5. Minimal routing/source-of-truth updates

Minimally update these documents only if needed:

- `docs/CONTEXT_INDEX.md`
- `AGENTS.md`
- `docs/CODEX_PATCH_RULES.md`
- `docs/CODEX_EXECUTION_DOCTRINE.md`

Add short references to:

- `docs/MODULAR_CONTRACT_GOAL_POLICY.md`
- `docs/LLMGameCreator_FEATURE_BACKLOG_AUDIT.md`, if created;
- Campaign Pack + bounded composite goal rule;
- modular policy must reduce manual goal cycles, not increase them.

Do not broadly rewrite these documents.

### 6. State docs

Update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Record:

- `rich_package_assembly_coverage_audit_verification passed`;
- Goal 024 accepted;
- Goal 025 / S199 still not started;
- before Goal 025 implementation, this process/policy adoption task runs;
- after this task, next recommended work is Campaign Pack / Goal 025 plan-only, not implementation.

Current active gate after this task must be:

```text
modular_contract_goal_policy_adoption_verification required
```

Do not mark it passed.

### 7. Package Assembly Campaign Pack

Prepare a plan-only Campaign Pack for the next 3-5 bounded package assembly expansion goals.

Preferred file:

```text
docs/PACKAGE_ASSEMBLY_EXPANSION_CAMPAIGN_PACK.md
```

Campaign Pack must contain:

- 3-5 next candidate goals;
- which goals are bounded composite goals;
- task type of each goal;
- required proof level of each goal;
- input contracts;
- output contracts;
- expected module/integration/proof phases;
- anti-overfit consumer fixtures;
- allowed/forbidden scope at plan level;
- where the first rare product vertical gate happens;
- which manual checks can be replaced by automated/synthetic checks;
- stop conditions;
- why this reduces manual goal cycles rather than increasing them.

Expected direction:

- Goal 025 candidate: `package_assembly_expansion_1_world_and_entities`;
- Goal 025 should be a bounded phased composite goal, not a broad vertical slice;
- product vertical gate is not required in Goal 025 if Level 2/3 proof is enough;
- first product vertical gate should be a separate rare gate after several prepared modules/integrations.

## Validation

Run focused docs/state/handoff tests if they exist.

Run `check-all.ps1` only if current repo policy or state-doc tests require it for final acceptance. If not run, explain why.

No product smoke route is required unless existing policy forces a docs/state smoke. This task is not a product feature.

Do not run Unity/provider/LLM/Lua/media.

## Pre-final self-review

Before final report, verify:

- README no longer contains stale current phase / active goal;
- modular policy exists and says Contract / Module / Integration / Proof phases are internal to composite goals by default;
- policy explicitly says it must reduce manual goal cycles, not increase them;
- optional live runtime LLM/RAG is explicit non-goal;
- Goal 025/S199 not started;
- production code not changed;
- GamePackage schema not changed;
- final gate is `modular_contract_goal_policy_adoption_verification required`;
- `rich_package_assembly_coverage_audit_verification` is recorded as passed.

## Final response requirements

The final Codex response must include:

- changed files;
- which proposed docs were promoted/adapted;
- which proposed content was rejected/changed and why;
- acceptance evidence table;
- tests run / not run + reason;
- active gate after this task;
- next recommended work;
- explicit confirmation: Goal 025 not started;
- explicit confirmation: S199 not started;
- explicit confirmation: no production code changes;
- explicit confirmation: no live runtime LLM/RAG path introduced;
- no-git-commands confirmation unless explicitly requested.
