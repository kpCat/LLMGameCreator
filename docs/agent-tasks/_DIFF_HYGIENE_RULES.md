# _DIFF_HYGIENE_RULES.md — final diff hygiene rules

This file is a shared quality rule document, not a task spec.

## Core rule

The final intended diff must be small, reviewable, and limited to the task.

## Allowed changed files

Changed files must match:

```text
- task spec Allowed files;
- files explicitly approved by the user;
- necessary documentation/cursor files required by devflow.
```

If the task needs a file outside Allowed files, stop and report. Do not silently expand scope.

## Forbidden final diff content

The final intended diff must not contain:

```text
- .devflow/runs/**
- *.trx
- *.log
- build outputs
- bin/obj
- IDE metadata
- local settings
- caches
- generated full reports unless task explicitly asks to version them
- unrelated formatting churn
```

## Git command policy

If git commands are forbidden, do not run git commands.

Instead:

```text
- track files you intentionally edited;
- list them in CURRENT_RUN.md;
- list them in final report;
- warn if you cannot verify untracked/generated outputs;
- do not claim a clean git diff unless you actually checked it outside the forbidden policy.
```

If git commands are allowed by the user, use them only for inspection unless explicitly asked to commit/push.

## Formatting churn

Do not mix feature/test behavior changes with broad formatting changes.

Allowed:

```text
- local formatting needed for touched lines;
- preserving or restoring existing style;
- raw string literals for JSON/Markdown/Lua readability.
```

Not allowed without explicit task:

```text
- reformat whole file;
- rename unrelated symbols;
- rewrite unrelated tests;
- switch style conventions globally.
```

## Generated run artifacts

Run artifacts are report evidence, not source.

Correct final report:

```text
check-all passed.
Run directory: .devflow/runs/20260617_065432-check-all
```

Incorrect source change:

```text
Added .devflow/runs/20260617_065432-check-all/test-results/*.trx to repository.
```

## Final report requirements

Final report must include:

```text
Changed files:
Generated artifacts created locally:
Whether generated artifacts are intended source changes: yes/no
Any diff hygiene uncertainty:
```
