# External scouting for Goal 032: Dynamic Semantic Feature System And Influence Rule Kernel

## Decision

Goal 032 should remain BCL-only and Application-layer only.

No external dependency is accepted for this goal.

Reason: the required behavior is not a generic business rules engine or arbitrary expression evaluator. LLMGameCreator needs a deterministic, inspectable, UI-schema-friendly semantic factor system with explicit feature scopes, inheritance, applicability, missing/illegal factor diagnostics, influence traces, and reproducible seeded resolution. A third-party rules engine would increase dependency and expression-language risk before the project has its own authoring contract.

## Current repository context checked

- `main` currently contains Goal 031 artifacts and state update.
- `CURRENT_GENERATOR_STATE.md` records Goal 031 at `semantic_pack_composition_blueprint_verification required`.
- `FULL_GENERATOR_GOAL_QUEUE.md` says Goal 031 produced semantic pack composition blueprint evidence and Goal 032 has not started.
- `CONTEXT_INDEX.md` references Goal 030 and Goal 031 specs/scouting artifacts.
- Goal 029/030/031 already provide:
  - modular generator kernel readiness;
  - semantic artifact contract registry;
  - semantic pack compatibility planner;
  - semantic pack composition blueprint and cross-artifact linkage plans.

Goal 032 should consume that direction but must not require public `GamePackage` schema, Runtime, Unity, UI, provider/LLM/RAG, Lua or generator-library changes.

## Options considered

### Microsoft RulesEngine

Repository/project: `microsoft/RulesEngine`.

Pros:
- MIT license.
- JSON-based workflow/rules model.
- Supports C# dynamic expression style rules.
- Useful later as an optional adapter for business-rule-like gameplay/editor validations.

Cons for Goal 032:
- Encourages string/expression-rule authoring too early.
- External dependency and dynamic expression surface increase validation/security complexity.
- Does not directly model semantic inheritance, feature applicability, UI schema generation, or provenance.
- Would risk turning LLMGameCreator semantic design into a generic rules-engine wrapper.

Decision:
- Do not adopt now.
- Keep as future optional adapter candidate only.

### NRules

Repository/project: `NRules/NRules`.

Pros:
- MIT license.
- Mature .NET production rules engine.
- Rete/inference/forward-chaining behavior could be useful for future large simulations.

Cons for Goal 032:
- Too heavy and too inference-engine-shaped for the current deterministic authoring kernel.
- Rule execution order/conflict resolution must be tightly controlled and explained to the user.
- Rules are authored in C# DSL, which does not fit current user-editable semantic pack JSON/dynamic UI direction.
- Adds a dependency before the internal feature/influence contract is stable.

Decision:
- Do not adopt now.
- Keep as future optional simulation/rules adapter candidate after the in-house semantic contract is stable.

### Dynamic Expresso

Repository/project: `dynamicexpresso/DynamicExpresso`.

Pros:
- MIT license.
- Simple C# expression interpreter.
- Could help later if user-authored formulas need expression parsing.

Cons for Goal 032:
- Allows expression strings, custom functions and injected variables; that is more power than this goal needs.
- Would make validation, deterministic traces and safe UI authoring harder.
- The user specifically wants LLM not to carry combinatorial state and wants programmatic factor systems, not arbitrary script-like logic.

Decision:
- Do not adopt now.
- Future optional expression adapter only after a safe expression DSL contract exists.

### NCalc / Flee / JsonLogic.Net and similar expression/rules libraries

Pros:
- Some are permissive-license and convenient for formulas or JSON rules.

Cons for Goal 032:
- They solve expression evaluation, not semantic feature modeling.
- External dependencies would not create the required editor-schema, inheritance, applicability and diagnostics model.
- JsonLogic.Net has a Newtonsoft.Json dependency and visible maintenance concerns in issues; not ideal for this core seam.
- Flee-style IL/codegen/expression evaluation is unnecessary for deterministic semantic factor resolution.

Decision:
- Do not adopt now.

## Required architecture implication

Goal 032 should implement a small internal rules model, not an expression language:

- conditions are typed data records, not strings;
- effects are typed data records, not scripts;
- resolution order is explicit and deterministic;
- every output has trace/provenance;
- every illegal/missing/overconstrained state has stable diagnostics;
- the same model can later drive dynamic WinForms tabs/UserControls without touching UI in this goal.

## LLM policy for Goal 032

LLM is not the combinatorial resolver.

Accepted LLM role for future goals:
- lore intake;
- lore normalization;
- proposing seed concepts/factors/rules;
- suggesting missing archetype coverage;
- explaining contradictions;
- drafting review candidates.

Rejected LLM role for this goal:
- choosing every NPC mood/reputation/motive at generation time;
- writing final dialogue lines;
- evaluating runtime state;
- replacing the deterministic feature resolver.

Goal 032 must encode this as docs/report wording and tests where practical.
