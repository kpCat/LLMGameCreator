# Goal 097 — Final Roadmap Rebaseline, Dream Scope Register & Aggressive Goal Productivity Policy

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Rebaseline the LLMGameCreator roadmap after the Goal 074–096 aggressive goal chain.

This is a strategic control goal, not a feature implementation goal. It must make the project’s endgame explicit, prevent endless proof-layer expansion, and make future Codex goals larger/more end-to-end without losing the current architecture and validation discipline.

The rebaseline must include:

- milestone definitions: playable vertical slice, strong alpha, v1 full final, dream full final;
- visual/world/Unity pipeline status after Goal 096;
- full dream-scope register including fantasy, sci-fi, space-rangers-like, adult/rating pipeline, and realism/geospatial simulator tracks;
- a realistic/procedural world track for both self-generated worlds and optional real-world/geospatial ingestion;
- release risk register and release gate plan;
- aggressive goal productivity policy: fewer proof-only goals, larger composite deliverables, user-visible/editor/Unity progress every few goals;
- a revised estimate of remaining goals by milestone;
- future goal queue priorities.

Do not implement code, Runtime, Unity, provider, schema, map ingestion, OCR, internet access, or external dependencies in this goal.

## Important context

Recent stack:
- Goal 083 integrated visual/adult docs.
- Goal 084 added visual asset contract/rating metadata.
- Goal 085 added visual part-pack/rule-stack contracts.
- Goal 086 added deterministic microtile materializer.
- Goal 087 added map patch composer.
- Goal 088 added 144x144 surface/underground region proof, later unblocked by Goal 088A.
- Goal 089 added tiered validation pipeline.
- Goal 090 added parameterized visual world profiles and infinite chunk addressing.
- Goal 091 added chunk stream windows.
- Goal 092 added visual stream preview workspace.
- Goal 092A split oversized service/source-health guard.
- Goal 093 added visual chunk cache export contract.
- Goal 094 integrated cache export inspector.
- Goal 095 added Unity StreamingAssets handoff/probe.
- Goal 096 integrated Unity handoff inspector.

User concern:
The "Dream Full Final" previously described was incomplete. It must include realism/simulator tracks:
1. Optional real-world/geospatial ingestion track:
   - online/offline map/geodata ingestion;
   - not necessarily OCR as first-class term, but map/vector/raster/geospatial ingestion and reconstruction;
   - possible runtime/on-demand travel mode;
   - legal/licensing/provider/API boundaries;
   - 2D-to-3D/first-person world reconstruction;
   - living world simulation and causal systems.
2. Fully self-generated realism/simulation track:
   - finite and infinite generated realistic worlds;
   - causal living world;
   - simulation-first generation.

Important: do not start building this now. Record it as an important future track with required research and risk gates.

User concern:
Despite "aggressive goals", many goals produce about ~1000 lines or limited slices. Future goals should be more composite and product-oriented. Do not optimize for line count; optimize for larger end-to-end outcomes.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes `cc177a0f GREEN Goal 096 Unity handoff inspector probe readiness`.
4. Confirm Goal096 artifacts exist and remain `accepted=false`.
5. Confirm Goal096 report/quality gate show no P0/P1 blockers.
6. Inspect current dirty state before edits. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/VALIDATION_PIPELINE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md`
- `docs/context/VISUAL_ADULT_LAYER_CONTEXT_INDEX.md`
- `docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md`
- all `docs/deepsearch/*.md`
- `.llmgc/procedural/goal-096-unity-handoff-inspector-probe-readiness/unity-handoff-inspector-report.md`
- `.llmgc/procedural/goal-096-unity-handoff-inspector-probe-readiness/unity-handoff-inspector-quality-gate-scan.json`
- recent state/evidence for Goals 089–095 if needed for current status.

## Allowed files / areas

- `docs/ROADMAP_FINAL_REBASELINE.md`
- `docs/RELEASE_RISK_REGISTER.md`
- `docs/MILESTONE_GATES.md`
- `docs/GOAL_PRODUCTIVITY_POLICY.md`
- `docs/context/DREAM_SCOPE_REGISTER.md`
- `docs/context/REALISM_GEOWORLD_SIMULATOR_TRACK.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `.llmgc/procedural/goal-097-final-roadmap-rebaseline-dream-scope-productivity/`
- `docs/agent-tasks/goal-097-final-roadmap-rebaseline-dream-scope-productivity/`

## Forbidden files / areas

Do not change:
- any C# source file;
- WinForms code;
- Runtime / Runtime.Abstractions;
- Unity files;
- public GamePackage schema;
- provider / LLM / RAG / media provider code;
- Lua / Scripting;
- generator-library;
- `.sln`, `.csproj`, lock files;
- binary/raster media;
- generated assets;
- prompt dumps;
- external dependencies.

No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Create final roadmap rebaseline

Create `docs/ROADMAP_FINAL_REBASELINE.md`.

Required sections:
- Current position after Goal 096.
- What exists now.
- What is not yet real/playable.
- Milestone ladder:
  - Vertical Slice Final.
  - Strong Alpha.
  - v1 Full Final.
  - Dream Full Final.
- Estimated remaining aggressive goals per milestone.
- Mandatory end-to-end/user-visible progress rule.
- What to defer.
- What should be killed if scope explodes.

The roadmap must explicitly say that proof/evidence layers are not enough; future work must increasingly produce editor-visible, Unity-visible, playable or exportable progress.

### 2. Create dream scope register

Create `docs/context/DREAM_SCOPE_REGISTER.md`.

Must include:
- fantasy exploration / Heroes + Might-and-Magic-like track;
- sci-fi / ultra-modern future track;
- space-rangers-like track;
- procedural visual/media compiler track;
- adult/rating-gated extension track;
- realism/geospatial simulator track;
- self-generated realism simulator track;
- release/packaging/Steam/export track.

Each track needs:
- purpose;
- current status;
- future dependencies;
- what is not now;
- risks;
- required research.

### 3. Create realism/geoworld simulator track

Create `docs/context/REALISM_GEOWORLD_SIMULATOR_TRACK.md`.

This is a planning document only.

It must describe two future modes:

#### A. Real-world/geospatial ingestion mode

Use neutral terms:
- geodata ingestion;
- vector/raster tile ingestion;
- map source adapters;
- optional OCR/georeferencing for screenshots or non-structured maps;
- licensing/ToS gate;
- offline import/cache boundary;
- runtime online mode as optional adapter only;
- 2D-to-3D reconstruction;
- living-world simulation overlay;
- causal world state/deltas.

Must explicitly warn:
- do not scrape map tiles by default;
- do not violate provider ToS;
- prefer licensed/open data and official APIs;
- keep external data adapters optional and outside core;
- no runtime LLM/provider dependency;
- no implementation until deep research and legal/licensing policy exists.

#### B. Self-generated realism mode

- finite/infinite generated realistic worlds;
- procedural settlements/traffic/ecology/economy;
- first-person/pseudo-3D/3D presentation;
- simulation and causal deltas.

### 4. Create release risk register

Create `docs/RELEASE_RISK_REGISTER.md`.

Classify risks:
- P0 release blockers;
- P1 serious risks;
- P2 technical debt;
- P3 deferrable polish.

Must include:
- playable quality vs proof quality;
- Unity performance;
- StreamingAssets/platform issues;
- save/load and infinite world deltas;
- visual consistency;
- provider/provenance/licensing;
- adult/rating leakage;
- geospatial licensing/ToS/API risks;
- installer/export/clean machine risk;
- source-health/code-size risk;
- test duration/validation noise.

### 5. Create milestone gates

Create `docs/MILESTONE_GATES.md`.

Define acceptance gates for:
- Vertical Slice Final.
- Strong Alpha.
- v1 Full Final.
- Dream Full Final.

Each gate must require:
- user-visible/editor-visible progress;
- generated package export/import;
- Unity/player proof where applicable;
- validation tiers;
- manual acceptance checklist;
- release risk review.

### 6. Create aggressive goal productivity policy

Create `docs/GOAL_PRODUCTIVITY_POLICY.md`.

Must address:
- future "aggressive goals" must be larger composite deliverables, not just small proof layers;
- line count is not the target, but outcome size is;
- ordinary future feature goals should combine Application seam + evidence + tests + UI/export/readback when appropriate;
- every 3–5 feature goals should produce user-visible/editor-visible/Unity-visible progress;
- every 5–8 goals should have quality consolidation or release-risk pass;
- avoid repeatedly creating isolated 1000-line proof services;
- split source files before they exceed source-health limits;
- use Goal089 tiered validation policy;
- no manual check-all requirement from the user.

### 7. Update existing docs/state

Update:
- `CONTEXT_INDEX.md`
- `FULL_GENERATOR_GOAL_QUEUE.md`
- `CURRENT_GENERATOR_STATE.md/json`
- debt register
- artifact-scope policy.

Goal097 manual gate:
`final_roadmap_rebaseline_dream_scope_productivity_verification required`

Goal097 status:
`accepted=false`.

### 8. Generate evidence

Create `.llmgc/procedural/goal-097-final-roadmap-rebaseline-dream-scope-productivity/`.

Recommended artifacts:
- `final-roadmap-rebaseline-report.md`
- `milestone-estimate-matrix.json`
- `dream-scope-register-summary.json`
- `release-risk-register-summary.json`
- `goal-productivity-policy-summary.json`
- `quality-gate-scan.json`

Evidence must prove:
- all required docs exist;
- realism/geospatial simulator track is recorded;
- arbitrary/infinite world track remains;
- release risk register exists;
- productivity policy exists;
- no product code changed;
- no Unity/Runtime/provider/schema changes;
- no binary media;
- no prompt dump.

## Validation policy

Use Goal089 tiered validation.

Required:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-097-final-roadmap-rebaseline-dream-scope-productivity" -FocusedFilter "CurrentState" -ProductSmokeFilter ""
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-097-final-roadmap-rebaseline-dream-scope-productivity"
git diff --check
git diff --cached --check
```

`check-spine-fast.ps1` is optional for this docs-only rebaseline unless the repo policy requires it. Full `check-all.ps1` is not required.

## Quality gate

GREEN only if:
- no product code changed;
- all required roadmap/risk/productivity/dream-scope docs exist;
- realism/geospatial simulator track is represented as future scoped track, not immediate implementation;
- release risks are explicit;
- aggressive goal productivity policy is explicit;
- milestone estimates are present;
- docs/state/queue/debt are synchronized;
- evidence is deterministic;
- validation commands pass;
- artifact scope passes;
- final worktree clean.

## Stop/block conditions

Return BLOCKED if:
- existing docs conflict so badly that a coherent roadmap cannot be written;
- artifact scope cannot be satisfied;
- roadmap requires product code changes.

Return FAILED if:
- validation fails due to this goal and cannot be repaired inside allowed docs/evidence files.

## Final report

Report:
- Final status.
- Latest commit before/after.
- Push status.
- Files changed.
- Roadmap summary.
- Dream scope additions.
- Realism/geospatial track summary.
- Goal productivity policy summary.
- Milestone estimate summary.
- Release risk summary.
- Validation results.
- Artifact scope result.
- Evidence hygiene.
- Remaining debt.
- Final git status.
- Git commands used and why.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
- `GREEN Goal 097 final roadmap rebaseline dream scope productivity`
- `BLOCKED Goal 097 final roadmap rebaseline dream scope productivity`
- `FAILED Goal 097 final roadmap rebaseline dream scope productivity`
