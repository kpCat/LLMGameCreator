# _FIXTURE_AND_GOLDEN_RULES.md — fixture and golden file rules

This file is a shared quality rule document, not a task spec.

## Fixture purpose

Fixtures exist to make behavior deterministic and reviewable. They are not a dumping ground for logs, generated artifacts, or full real-world outputs.

## Good fixtures

Good fixtures are:

```text
- small;
- human-readable;
- named by scenario;
- deterministic;
- minimal while still reproducing behavior;
- stored under an obvious task/domain fixture folder;
- safe to review in a diff.
```

Example names:

```text
valid_minimal_json_object.txt
markdown_fenced_json.txt
text_before_json.txt
missing_required_id.json
unknown_manifest_capability.json
runtime_command_invalid_target.json
```

## Bad fixtures

Bad fixtures include:

```text
- huge generated run logs;
- full TRX files;
- raw provider outputs with unrelated noise;
- local absolute paths unless the task specifically validates path handling;
- binary blobs unless asset handling task explicitly requires them;
- duplicated fixtures with unclear purpose;
- fixtures that pass only because of whitespace/encoding accident.
```

## Golden/snapshot files

Golden files are allowed when exact output shape matters.

Golden files must be:

```text
- stable;
- minimized;
- easy to diff;
- named by scenario;
- updated only when behavior intentionally changes;
- accompanied by test explaining what contract they pin.
```

## LLM/corpus fixtures

For LLM-facing code:

```text
- use redacted/minimized raw output;
- preserve the failure shape;
- remove irrelevant text;
- never include secrets, API keys, personal data, or full logs;
- do not include large generated artifacts unless task explicitly approves.
```

## Fixture loader discipline

Prefer existing fixture loading patterns.

If adding a new fixture loader:

```text
- keep it local to the test class unless reuse is needed;
- avoid brittle absolute paths;
- avoid project structure assumptions when an existing helper exists;
- if a brittle local helper is unavoidable, say so in CURRENT_RUN.md.
```

## Generated run outputs

`.devflow/runs/**`, logs, TRX files, build outputs, and local diagnostics are run artifacts.

They can be referenced by path in reports, but they should not become source fixtures unless a task explicitly says to minimize/redact and add them as fixtures.
