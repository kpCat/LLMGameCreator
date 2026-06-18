# LLMGameCreator Product Slice 001 Pack

## Purpose

This pack starts the first real product-facing slice after the successful M4.1 real-model evaluation gate.

Main goal:

```text
Capability Picker v2 usability
+ non-breaking composable capability foundation
+ Russian explanations
+ compatibility diagnostic categories
+ tests
```

This is intentionally a **large but bounded implementation task** for Codex. It is not documentation-only.

## Apply

Unzip at repository root.

Expected new/updated files:

```text
docs/PRODUCT_SLICE_001_CAPABILITY_COMPOSER_V2_FOUNDATION.md
docs/LLMGAMECREATOR_1_0_ROADMAP.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/001_CAPABILITY_COMPOSER_V2_FOUNDATION.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/001_CODEX_PROMPT.md
```

## Recommended Codex run

Give Codex the contents of:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/001_CODEX_PROMPT.md
```

Do not ask Codex to “figure out the whole 1.0 plan”. The plan is already included here.

## Strategy

We do not want a week of documentation-only work. We also do not want a giant unbounded rewrite.

This task should produce immediate user-facing product value:

- user can understand dropdown options;
- user can understand feature bundles;
- user can see why a selection is invalid/risky/unsupported;
- model begins supporting modules/modifiers/constraints without breaking old selection JSON;
- prompt context can include modules/modifiers/constraints when available;
- existing strict LLM generation/evaluation remains green.

## Not included

This slice does not implement:

- full infinite world generation;
- full economy simulator;
- full balance simulator;
- Lua executor;
- GamePackage assembly/apply path;
- runtime preview;
- Unity export.

Those are later product slices.
