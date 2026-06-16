# ARCHIVE_MANIFEST.md — llmgc_agent_task_pack_002

Archive: `llmgc_agent_task_pack_002.zip`

Purpose:

```text
Add the second agent-task pack: executable M4.1 strict-generation/evaluation task specs based on the current repository state.
```

This archive is docs/devflow guidance only. It does not modify production code, tests, solution files, or project files.

## Files included

```text
README_APPLY_AGENT_TASK_PACK_002.md
ARCHIVE_MANIFEST.md

docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md

docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
docs/agent-tasks/M4_1/M4_1_004_STRICT_JSON_PARSER_CORPUS_GUARD.md
docs/agent-tasks/M4_1/M4_1_005_EVALUATION_MARKDOWN_GOLDEN_RECOMMENDATIONS.md
docs/agent-tasks/M4_1/M4_1_006_STRICT_REPAIR_PROMPT_GUARDRAILS.md
docs/agent-tasks/M4_1/M4_1_007_M4_GATE_DECISION_REPORT.md
docs/agent-tasks/M4_1/M4_1_008_AGENT_TASK_DOCS_CONSISTENCY_GUARD.md
```

## Repository-state assumptions used

```text
- Current phase remains M4.1 real-model evaluation gate.
- M5/M6/M8 production work remains locked until current-state docs explicitly unlock it.
- Strict JSON parsing is currently owned by GeneratorPlanStrictJsonResponseParser.
- Strict evaluation summary/hot spots are currently owned by GeneratorPlanStrictLlmEvaluationService.
- Evaluation markdown output is currently owned by GeneratorPlanStrictLlmEvaluationMarkdownRenderer.
- Repair prompt text is currently owned by GeneratorPlanStrictLlmArtifactRepairPromptBuilder.
```

## Safety

```text
- No src/ changes.
- No tests/ changes.
- No .sln changes.
- No .csproj changes.
- No M5/M6 unlock.
- No NEXT_TASK forced cursor change.
```
