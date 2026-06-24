# Files To Delete Or Archive Before Codex

Status: cleanup recommendation  
Purpose: reduce accidental old-context reads before Product Slice 029

## Safe To Delete Or Move To Archive

These files are historical packaging/apply instructions. They are not current source-of-truth and should not be read for the next Codex task.

Root files:

```text
README_APPLY_AGENT_TASK_PACK_001.md
README_APPLY_AGENT_TASK_PACK_002.md
README_APPLY_AGENT_TASK_PACK_003.md
README_APPLY_AGENT_TASK_PACK_004.md
README_APPLY_AGENT_TASK_PACK_005.md
README_APPLY_AGENT_TASK_PACK_006.md
README_APPLY_AGENT_TASK_PACK_007.md
README_APPLY_AGENT_TASK_PACK_009.md
README_APPLY_AGENT_TASK_PACK_010.md
README_APPLY_AGENT_TASK_PACK_011.md
README_APPLY_AGENT_TASK_PACK_012.md
README_APPLY_AGENT_TASK_PACK_013.md
README_APPLY_CAPABILITY_COMPOSER_V2_PACK.md
README_APPLY_PACK_008.md
README_APPLY_PRODUCT_SLICE_001.md
README_APPLY_PRODUCT_SLICE_002.md
README_APPLY_PRODUCT_SLICE_003.md
README_APPLY_PRODUCT_SLICE_004.md
README_APPLY_PRODUCT_SLICE_005.md
README_APPLY_PRODUCT_SLICE_006.md
README_APPLY_PRODUCT_SLICE_006_1.md
```

The GitHub web listing currently showed files through `README_APPLY_PRODUCT_SLICE_006_1.md` and may hide the rest behind "View all files". If more root files match these patterns, they can also be deleted or archived:

```text
README_APPLY_AGENT_TASK_PACK_*.md
README_APPLY_PRODUCT_SLICE_*.md
README_APPLY_PACK_*.md
README_APPLY_CAPABILITY_COMPOSER_V2_PACK.md
```

Old task-pack files:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/*_CODEX_PROMPT.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/*_KILO_PROMPT.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/*_HARDENING_PROMPT.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/*_ARCHIVE_MANIFEST.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/*_README_APPLY_PRODUCT_SLICE.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/README_S021_HARDENING_AND_S022.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/product-slice-028-manual-import-repair-semantic-catalog-foundation-CODEX_TASK.md
```

## Prefer Archive Instead Of Delete If You Want History In The Working Tree

If you do not want to delete them, move them under:

```text
docs/archive/old-agent-packs/
docs/archive/old-product-slice-prompts/
```

Then add a short `README.md` in each archive folder:

```text
Historical context only. Do not use as current Codex planning authority.
```

## Do Not Delete These

Keep current source-of-truth files:

```text
AGENTS.md
README.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md
docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md
docs/FULL_GAME_GENERATION_MASTER_PLAN.md
docs/GAME_SYSTEM_VARIANT_TAXONOMY.md
docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md
docs/ROADMAP_TO_FULL_GENERATOR.md
```

Keep architecture/domain docs unless there is a separate cleanup task. They can be stale in parts, but deleting them blindly can remove useful design constraints.
