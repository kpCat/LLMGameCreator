# Product Slice 008: Active Generated Package Flow + Quest/Dialogue Preview Stubs

## Goal

Fix the product seam between package assembly and Runtime Preview, then add the first safe quest/dialogue preview state.

## Mandatory repair first

Manual testing found:

```text
Save decisions + apply
-> .llmgc/package-assembly/package.json is generated
-> Runtime Preview still starts the old root package.json
-> generatedContent appears empty until user manually copies package.json
```

This is not a Runtime Preview browser bug. It is an active-package flow gap.

## Desired active package flow

After Artifact Review successfully applies approved artifacts:

```text
Package assembly status: valid_package
Exported: .llmgc/package-assembly/package.json
Applied artifacts: N
[Use assembled package as current]
```

Clicking `Use assembled package as current` should:

```text
read .llmgc/package-assembly/package.json
validate it
load it into ICurrentGamePackageService as the active current package
show clear status
allow Runtime Preview to start generatedContent without manual file copy
```

Do not overwrite root `package.json` by default.

Optional later behavior can copy/replace root package after confirmation, but this slice should stay non-destructive by default.

## Quest/dialogue preview stubs

After the active package flow works, add minimal preview-only interactions:

```text
select NPC -> show linked dialogues if any
select dialogue -> preview dialogue lines to log
select quest -> start quest preview
started quest -> show in preview journal
mark next quest step preview -> advance in-memory preview state
```

This is not full runtime quest/dialogue execution.

## Non-goals

Do not implement:
- real dialogue choice execution;
- real quest completion/effect execution;
- real inventory;
- real combat;
- Unity;
- Lua/effect script execution;
- LLM generation changes;
- package schema destructive migration.

## Done

Done when:
- Runtime Preview can start assembled generated package without manual copy;
- Browser still works;
- dialogue lines can be previewed;
- quests can be started/advanced in preview state;
- headless smoke proves the active-package flow and preview stubs.
