# 60_M9_TEMPLATE_FAMILIES_AND_BALANCING.md — M9 template families and balancing

Locked until at least one end-to-end family lifecycle is stable.

## Phase goal

Prove breadth by defining several complete game families through shared capability bundles without bespoke one-off rewrites.

## TASK M9-001 — First complete family acceptance pack

Status: future locked.
Requires approval: yes.

Objective: define one family acceptance pack covering capability selection, artifact contracts, approved artifacts, assembly, validation, runtime smoke and diagnostics.

Source docs:

```text
docs/GAME_GENERATION_CAPABILITY_MATRIX.md
docs/GAME_FORM_FACTORS_AND_PRESENTATION_MODES.md
docs/GAME_SYSTEM_VARIANT_TAXONOMY.md
docs/ROADMAP_TO_FULL_GENERATOR.md
.devflow/FINAL_ACCEPTANCE_CRITERIA.md
```

Target areas:

```text
docs/ or generator-library/atlas/ for family definition
tests/fixtures/families/ if created
tests/LLMGameCreator.Tests/
```

Required checks:

```text
family capability resolution
artifact fixture validation
package validation
runtime smoke
check-all
```

Stop on:

```text
one_off_hardcoded_family_logic
schema_change_without_approval
requires_media_generation
requires_more_than_10_files
```

## TASK M9-002 — Second and third family reuse check

Status: future locked after M9-001.
Requires approval: yes.

Objective: prove at least two more families reuse the same lifecycle without new bespoke architecture.
