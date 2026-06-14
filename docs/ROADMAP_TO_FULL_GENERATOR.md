# Roadmap To Full Generator

Status: executable roadmap for Codex  
Scope: milestones from current narrow vertical slice to full game generation without media  
Non-scope: direct implementation in this document

Each milestone must be implemented as a bounded task or small task group. A milestone is not complete because docs exist; it is complete when its acceptance gates pass.

## M0 Current Vertical Slice Baseline

Goal: document and preserve the existing one-click export slice as a baseline, not as the full generator.

Why it exists: prevents future tasks from mistaking deterministic template export for real full game generation.

User-visible value: user can export a baseline package and see what is missing.

Required docs/code areas: one-click export docs, GamePackage assembly docs, `docs/CONTEXT_INDEX.md`.

Non-goals: no new generation feature, no Lua execution, no provider call.

Expected file scale: 0-2 docs, 0-2 focused tests if behavior changes.

Acceptance tests: existing one-click export tests continue to pass.

Done criteria: docs explicitly say one-click export is a narrow vertical MVP.

What Codex must not decide by itself: whether to remove or replace the current export pipeline.

## M1 Authoritative Planning Pack

Goal: create master plan, capability matrix, role contract, prompt hardening, roadmap and Codex doctrine.

Why it exists: future tasks need a source of truth and task discipline.

User-visible value: next work is strategic instead of tactical feature drift.

Required docs/code areas: `docs/FULL_GAME_GENERATION_MASTER_PLAN.md`, capability matrix, role contract, prompt hardening, roadmap, doctrine, context index.

Non-goals: no production code, UI, schema, DB, Lua modules or runtime preview.

Expected file scale: 6-8 docs.

Acceptance tests: build/test pass or clear reason if skipped; docs pass consistency and mojibake checks.

Done criteria: all required docs exist and cross-link from context index.

What Codex must not decide by itself: implementation order after this pack unless asked.

## M1.1 Game Form Factor And System Variant Taxonomy

Goal: freeze explicit game form-factor, presentation, world, actor, inventory, combat, progression, pathfinding and NPC behavior ids before implementation-heavy features.

Why it exists: future generation must not accidentally narrow to only top-down 2D adventures.

User-visible value: future tasks can choose concrete taxonomy ids instead of vague requests like "make RPG".

Required docs/code areas: form-factor docs, system variant taxonomy, character card contracts, world/chunk contracts, interaction/combat/progression contracts, atlas JSON seed files, capability matrix, context index.

Non-goals: no production code, WinForms UI, GamePackage schema, DB schema, runtime preview, Lua execution, provider calls or built-in game templates.

Expected file scale: docs and atlas JSON only.

Acceptance tests: atlas JSON parses; build/test pass or clear reason if skipped; docs pass consistency and mojibake checks.

Done criteria:

- all major game forms have ids;
- pseudo-3D/2D-texture mode is documented;
- future contracts are named;
- capability matrix references variant docs;
- Codex tasks must choose variant ids instead of vague "make RPG".

What Codex must not decide by itself: implementing runtime/export support for these variants before a separate approved task.

## M2 Artifact Review / Approval UI

Goal: make artifact review and approval usable for many artifact kinds.

Why it exists: full generation produces many artifacts and needs human review before promotion.

User-visible value: user can inspect, approve, reject or request repair for generated artifacts.

Required docs/code areas: approval pipeline, Design DB artifact storage, `docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md`, WinForms Artifact Review / Generator Library pages.

Non-goals: no direct package mutation from review UI, no provider calls.

Expected file scale: 8-14 files when UI, service, tests and docs are included.

Acceptance tests: application service tests for decision state; UI smoke if page wiring changes.

Done criteria: Artifact Review page can capture `.example.json` files for pending review, persist approved/rejected/repair decisions, rebuild the approved artifact set from approved items only, and save validation rows.

What Codex must not decide by itself: which content should be auto-approved beyond explicitly valid low-risk artifacts.

## M3 Capability + Feature Bundle Picker

Goal: allow selecting game families and feature bundles from a capability matrix/atlas.

Why it exists: generation must start from explicit capabilities, not vague prompts.

User-visible value: user chooses "party RPG", "city builder", "automation" or custom bundles and sees required systems.

Required docs/code areas: capability atlas, generator-library manifests, Design DB registry, profile docs.

Non-goals: no generator execution, no package mutation.

Expected file scale: 5-10 files for a narrow service slice; UI, artifact persistence, presenter tests and docs may be larger when implemented together.

Acceptance tests: dependency closure, missing capability detection, unknown id rejection, compatibility diagnostics, latest artifact save/read and presenter mapping.

Done criteria: selected variants and bundles resolve capabilities, artifact contracts, validators, prompt context templates, runtime targets and missing prerequisites into `artifact/generator_plan_capability_selection/latest`.

What Codex must not decide by itself: adding a new genre as default or enabling sensitive overlays.

## M4 Strict LLM Artifact Generation + Repair Loop

Goal: add production-ready prompt execution for one or more strict artifact contracts.

Why it exists: deterministic template production is not enough for full generation.

User-visible value: model can generate valid drafts and repair failures within bounds.

Required docs/code areas: prompt hardening, model workflow roles, prompt context pack map, generated artifacts, validators.

Non-goals: no runtime LLM calls, no auto-apply, no arbitrary text generation.

Expected file scale: 6-12 files for one artifact family.

Acceptance tests: malformed output rejection, enum/id preservation, repair success/failure cases.

Done criteria: one contract can be generated, validated, repaired and staged.

What Codex must not decide by itself: model/provider selection or network endpoint defaults.

## M5 Lua Module Registry / Executor Integration

Goal: run approved deterministic Lua generator modules through sandbox, manifests and validation.

Why it exists: Lua should generate repeatable IR/config/data instead of asking LLM to print bulk content.

User-visible value: selected modules can produce chunk/world/entity/quest/item configs deterministically.

Required docs/code areas: Lua generation plan, manifest contract, scripting project, generator library registry, validators.

Non-goals: no unrestricted Lua, no filesystem/network access, no GamePackage mutation from Lua.

Expected file scale: 8-15 files for one safe module family.

Acceptance tests: forbidden API rejection, deterministic output, manifest capability check, artifact validation.

Done criteria: one approved module produces a validated artifact without direct package mutation.

What Codex must not decide by itself: activating broad Lua execution for all modules.

## M6 Rich GamePackage Assembly

Goal: expand assembly from narrow profile/scene/entity/quest/mechanics mapping to world, dialogue, items, economy, combat and UI-related data where schema supports it.

Why it exists: full generator requires richer package output.

User-visible value: generated packages contain meaningful systems, not only baseline maps and seed entities.

Required docs/code areas: GamePackage format, validators, assembly pipeline, artifact contracts.

Non-goals: no schema change without explicit migration task, no arbitrary artifact writes.

Expected file scale: 6-12 files per capability family.

Acceptance tests: assembly maps known artifacts, rejects invalid refs, package validation passes.

Done criteria: approved artifacts produce a baseline-valid package with selected gameplay domains.

What Codex must not decide by itself: expanding package schema silently.

## M7 Infinite / Chunked World Generation

Goal: introduce seed/config/rule-based chunked world generation artifacts.

Why it exists: large worlds must not be generated as huge arrays.

User-visible value: user can generate and preview chunk rules for large or infinite worlds.

Required docs/code areas: world/chunk contracts, Lua chunk modules, validation strategy, runtime state/saves.

Non-goals: no massive tile dump, no final Unity world streaming.

Expected file scale: 8-14 files for first slice.

Acceptance tests: deterministic same seed/chunk output, sparse override validation, reachability checks.

Done criteria: chunk config validates and runtime preview can load generated/compiled chunk data path.

What Codex must not decide by itself: world scale default for all game profiles.

## M8 Runtime Preview Validation Loop

Goal: run generated packages through command/smoke validation and feed failures back into repair/review.

Why it exists: package validation alone does not prove playability.

User-visible value: generated game can be smoke-played before export.

Required docs/code areas: runtime services, validation strategy, runtime simulator/preview, artifact diagnostics.

Non-goals: no final player UX, no model calls from runtime.

Expected file scale: 5-10 files.

Acceptance tests: load/wait/move/interact/dialogue/combat smoke for selected profiles.

Done criteria: runtime failures become validation/repair artifacts.

What Codex must not decide by itself: auto-repairing playable package state without user approval.

## M9 Full Game Template Families And Balancing

Goal: define and validate several complete game families through shared capability bundles.

Why it exists: the generator must prove breadth without bespoke rewrites.

User-visible value: user can choose a family and get a coherent generated game structure.

Required docs/code areas: capability matrix, artifact contracts, Lua modules, validators, assembly mappings.

Non-goals: no media production, no hardcoded one-game logic.

Expected file scale: varies; one family per task group.

Acceptance tests: each family validates profile, artifacts, package and runtime smoke.

Done criteria: at least three distinct families share the same lifecycle.

What Codex must not decide by itself: canon, sensitive overlays or platform restrictions.

## M10 Export Profiles / Unity IR Later

Goal: define export profiles and Unity-facing IR after package generation is stable.

Why it exists: final player/export should consume stable data, not arbitrary generated code.

User-visible value: generated package can be prepared for a future Unity runtime shell.

Required docs/code areas: Unity/player contract, Unity IR, export profile docs, validators.

Non-goals: no arbitrary Unity C# generation, no full Unity project rewrite in one task.

Expected file scale: 6-12 files for IR-only slice; larger for actual Unity player later.

Acceptance tests: IR schema validation, asset/prefab binding refs, export dry-run report.

Done criteria: Unity IR is validated data and does not bypass GamePackage or C# authority.

What Codex must not decide by itself: building or changing Unity runtime architecture without user approval.
