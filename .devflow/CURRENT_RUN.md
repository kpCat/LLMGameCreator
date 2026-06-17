Task id: M4_1_004
Goal: Add fixture-driven proof coverage for strict JSON parser behavior - create fixtures and tests for all invalid cases specified in task spec.
Task spec: docs/agent-tasks/M4_1/M4_1_004_STRICT_JSON_PARSER_CORPUS_GUARD.md
Source docs read:
- .devflow/LOCAL_AGENT_ROLE.md
- .devflow/AUTONOMOUS_RUNBOOK.md
- .devflow/STOP_CONDITIONS.md
- .devflow/CONTEXT_BUDGET_POLICY.md
- .devflow/DEFINITION_OF_DONE.md
- .devflow/CODE_QUALITY_AND_STYLE.md
- .devflow/LOCAL_AGENT_REVIEW_CHECKLIST.md
- .devflow/RECURSIVE_TASK_SELECTION_PROTOCOL.md
- docs/agent-tasks/000_INDEX.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- .devflow/MODELING_STRATEGY.md
Target files changed:
- tests/fixtures/strict-llm-raw-output/valid_minimal_json_object.txt (created)
- tests/fixtures/strict-llm-raw-output/empty_response.txt (created)
- tests/fixtures/strict-llm-raw-output/markdown_fenced_json.txt (created)
- tests/fixtures/strict-llm-raw-output/text_before_json.txt (created)
- tests/fixtures/strict-llm-raw-output/text_after_json.txt (created)
- tests/fixtures/strict-llm-raw-output/two_json_objects.txt (created)
- tests/fixtures/strict-llm-raw-output/json_array_root.txt (created)
- tests/fixtures/strict-llm-raw-output/broken_trailing_comma.txt (created)
- tests/fixtures/strict-llm-raw-output/invalid_escape.txt (created)
- tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictJsonResponseParserTests.cs (updated)
Existing patterns inspected:
- GeneratorPlanStrictJsonResponseParser.cs - strict JSON parsing with diagnostic codes
- GeneratorPlanStrictLlmArtifactGenerationModels.cs - diagnostic codes and models
- GeneratorPlanStrictJsonResponseParserTests.cs - existing test patterns (inline strings)
Proof tests added:
- Fixture_ValidMinimalJsonObject_ReturnsOk - valid JSON passes
- Fixture_EmptyResponse_ReturnsJsonInvalid - empty string returns JsonInvalid
- Fixture_MarkdownFencedJson_ReturnsJsonMarkdownFence - fenced JSON returns JsonMarkdownFence
- Fixture_TextBeforeJson_ReturnsJsonTextWrapper - text before JSON returns JsonTextWrapper
- Fixture_TextAfterJson_ReturnsJsonTextWrapper - text after JSON returns JsonTextWrapper
- Fixture_TwoJsonObjects_ReturnsDeterministicError - two objects returns deterministic error
- Fixture_JsonArrayRoot_ReturnsJsonRootNotObject - array root returns JsonRootNotObject
- Fixture_BrokenTrailingComma_ReturnsJsonInvalid - broken trailing comma returns JsonInvalid
- Fixture_InvalidEscape_ReturnsJsonInvalid - invalid escape returns JsonInvalid
- Diagnostic_PreservesContractId - diagnostic preserves contract id
Non-goals preserved:
- No JSON repair performed
- No Markdown fence extraction
- No parser semantic changes from strict rejection to permissive extraction
Stop conditions checked:
- Allowed before M4.1 gate review: yes
- User approval: approved
- Fixtures are deterministic: yes
- No files outside allowed boundaries changed
Checks passed:
- dotnet build: success (0 warnings, 0 errors)
- dotnet test (focused): 15/15 passed
- check-all.ps1: PASSED (406/406 total tests, run directory: .devflow/runs/20260617_065432-check-all)
Repairs attempted: 1
- Fixed invalid_escape.txt fixture content to produce JSON parse failure