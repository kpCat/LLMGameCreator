# Codex Goal 024: Rich Package Assembly Coverage Audit

Start only after the prompt explicitly includes:

```text
capability_bundle_pipeline_inputs_verification passed
```

Then execute strictly:

```text
docs/GOAL_024_RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT.md
```

Hard stop:

```text
rich_package_assembly_coverage_audit_verification required
```

Do not mark the gate passed.

Do not start Goal 025 or S199.

Do not implement package assembly expansion.

Do not use git commands except through the bounded scope allowed by `docs/GOAL_024_RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT.md` and `.devflow/scripts/check-artifact-scope.ps1`.

## Required read-first order

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md`
8. `docs/GOAL_023_CAPABILITY_BUNDLE_SELECTION_TO_PIPELINE_INPUTS.md`
9. `docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md`
10. `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-report.json`
11. `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-generator-inputs.json`
12. `docs/GAME_PACKAGE_FORMAT.md`
13. local package assembly / validation / runtime-loader analog files needed by the audit.

## Scope reminder

This goal is a package assembly coverage audit. It must not add new `GamePackage` schema, change package validators, implement generation, run Unity, add UI, edit `generator-library`, execute LLM/RAG/provider/media/Lua, or mutate historical `.llmgc/procedural/**` artifact families.

The only current artifact root is:

```text
.llmgc/procedural/rich-package-assembly-coverage-audit/
```

Use the Goal 022 scope guard at the end.

## Final response reminder

Report changed files, compact artifacts, hashes, focused/product-smoke/check-all/scope-guard results, coverage summary, gap summary, next-slice recommendation, final gate status, no Goal 025/S199 confirmation, and exact bounded git usage.
