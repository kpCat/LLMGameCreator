Task id: S023_STATE_UPDATE_RULE_FIX
Goal: repair CURRENT_GENERATOR_STATE.json required schema property after S023 state cleanup

Source docs/code read:
- docs/CURRENT_GENERATOR_STATE.json
- tests/LLMGameCreator.Tests/Docs/CurrentGeneratorStateDocsTests.cs
- latest user check-all failure log

Implemented:
- added required string property state_update_rule to docs/CURRENT_GENERATOR_STATE.json
- preserved explicit M5/M6 Locked semantics
- no markdown or code files changed in this pack

Expected checks:
- check-devflow-state.ps1: should pass
- check-all.ps1: should pass CurrentGeneratorStateJsonParses

Forbidden scope preserved:
- no Unity project or implementation
- no Runtime, WinForms, GamePackage schema, generator-library, solution or project changes
- no provider, generator, LLM or Lua execution
- no git commands
