# _TEST_QUALITY_RULES.md — proof-test quality rules

This file is a shared quality rule document, not a task spec.

A local agent must follow it for every code/test task unless the task spec explicitly says otherwise with user approval.

## Core rule

Weak tests are not proof tests. A proof test must pin the observable contract that matters for the task.

## Weak tests

These are weak when used alone:

```text
Assert.False(result.Ok);
Assert.True(result.Ok);
Assert.Single(result.Diagnostics);
Assert.NotEmpty(items);
Assert.NotNull(result);
Assert.Contains(text, output);
```

They can be useful only as supporting assertions, not as the whole proof.

## Strong tests

Prefer exact assertions:

```text
Assert.Contains(result.Diagnostics, d => d.Code == ExpectedCode);
Assert.Equal(expectedStatus, result.Status);
Assert.Equal(expectedCount, result.Items.Count);
Assert.Equal(expectedIds, result.Items.Select(item => item.Id));
Assert.Equal(expectedPath, diagnostic.Target);
Assert.False(packageWasMutated);
```

## Diagnostic behavior

If behavior produces diagnostics:

```text
- assert the exact diagnostic code;
- assert severity when severity matters;
- assert target/path/contract id when available;
- do not assert only that diagnostics exist;
- do not rely on diagnostic message text unless message is the actual public contract.
```

If multiple diagnostic codes are acceptable, the task spec must name the allowed set. The test must assert one of the allowed codes explicitly and the final report must explain why multiple codes are acceptable.

## Parser/validator tests

For parser/validator tasks:

```text
- at least one valid input test;
- at least one invalid input test;
- exact diagnostic code for invalid input;
- no exception for normal malformed user/LLM input;
- deterministic result across runs;
- no permissive behavior unless explicitly approved.
```

## LLM-facing tests

For LLM-facing code:

```text
- use fake clients/corpus/fixtures;
- do not call real LLM/provider/network;
- assert prompt/parse/repair boundaries;
- assert max repair attempts when relevant;
- assert validation is run after repair;
- assert invalid output is not accepted as valid.
```

## Test weakening rule

It is forbidden to:

```text
- delete tests to pass;
- loosen assertions without explaining why the contract changed;
- replace exact assertions with broad assertions;
- hide failure by catching exceptions in test code.
```

If the test cannot be made exact because current behavior is ambiguous, stop and report the ambiguity.
