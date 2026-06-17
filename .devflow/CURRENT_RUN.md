# CURRENT_RUN.md

Task id: DEVFLOW_DOCS_GUARD_TEST_SPLIT_POLISH
Goal: split docs guard tests and polish stop-mode automation checks
Task source: user_request

Source docs read:
- AGENTS.md
- README.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
- docs/agent-tasks/000_INDEX.md
- .devflow/NEXT_TASK.md
- .devflow/task-queue.json
- .devflow/scripts/check-devflow-state.ps1
- .devflow/scripts/advance-next-task.ps1
- .devflow/scripts/check-all.ps1
- .devflow/scripts/_common.ps1
- .devflow/CURRENT_RUN.md
- tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsConsistencyGuardTests.cs
- tests/LLMGameCreator.Tests/Docs/CurrentGeneratorStateDocsTests.cs

Existing patterns inspected:
- docs guard tests use xUnit Fact methods and repository root discovery from test base directory.
- devflow scripts use UTF-8/no-BOM writes and explicit stop-mode handling.
- stop-mode tests protect real .devflow/NEXT_TASK.md with backup/restore in try/finally.

Planned files changed:
- .devflow/CURRENT_RUN.md
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsConsistencyGuardTests.cs
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsFrameworkGuardTests.cs
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsExecutableSpecGuardTests.cs
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsStopModeGuardTests.cs
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsTestSupport.cs

Planned files deleted:
- README_APPLY_NEXT_TASK_REPAIR_005.md, if present
- README_APPLY_NEXT_TASK_006.md, if present
- README_APPLY_NEXT_TASK_008.md, if present

Non-goals:
- Do not modify production code under src/**.
- Do not modify LLMGameCreator.sln or any *.csproj.
- Do not modify docs/CURRENT_GENERATOR_STATE.md or docs/CURRENT_GENERATOR_STATE.json.
- Do not unlock or edit M5/M6/M8/M9/M10 task specs.
- Do not add dependencies.
- Do not rewrite the devflow framework or execute the next task.
- Do not use git commands.

Expected checks:
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~AgentTaskDocs"
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1

Actual checks:
- check-devflow-state.ps1: passed. Output: "Devflow state check passed. Current mode: stop (STOP_REVIEW). Tasks: 9. Known warnings: 2."
- focused AgentTaskDocs tests: passed. 17 passed, 0 failed.
- check-all.ps1: passed. Build: 0 warnings, 0 errors. Tests: 440 passed. Run directory: .devflow\runs\20260617_191547-check-all.
- Mojibake marker scan over changed files: passed, no markers found.

Changes applied:
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsConsistencyGuardTests.cs: deleted the 507-line monolithic guard file after moving tests.
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsTestSupport.cs: added shared repo root discovery, docs constants, spec path parsing, executable spec validation, safe PowerShell process start, and temporary NEXT_TASK backup/restore helper.
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsFrameworkGuardTests.cs: added focused framework/index/support-doc guards.
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsExecutableSpecGuardTests.cs: added focused executable task spec guards.
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsStopModeGuardTests.cs: added focused stop-mode NEXT_TASK and check-devflow-state guards.
- .devflow/CURRENT_RUN.md: recorded the current task, sources read, planned files, checks, results, and follow-up.

Deleted files:
- tests/LLMGameCreator.Tests/Docs/AgentTaskDocsConsistencyGuardTests.cs
- README_APPLY_NEXT_TASK_REPAIR_005.md: not present.
- README_APPLY_NEXT_TASK_006.md: not present.
- README_APPLY_NEXT_TASK_008.md: not present.

Follow-up:
- ARCHIVE_MANIFEST.md exists and describes llmgc_next_task_008.zip. It was left in place because deletion was not required to pass the task and should be reviewed separately if desired.
