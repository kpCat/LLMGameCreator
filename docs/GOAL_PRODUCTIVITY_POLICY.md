# Goal Productivity Policy

Status: Goal 097 process policy
Manual gate: `final_roadmap_rebaseline_dream_scope_productivity_verification required`
Accepted: false

## Purpose

Future aggressive goals should deliver larger composite outcomes. The project should not optimize for line count, but it must stop treating small isolated proof layers as the default measure of progress.

## Policy

- Line count is not the target; outcome size is the target.
- Future aggressive goals should combine contract, Application seam, deterministic evidence, focused tests and UI/export/readback when that combination creates a real product-visible result.
- Avoid repeatedly creating isolated 1000-line proof services that do not move editor-visible, Unity-visible, playable or exportable behavior forward.
- Every 3-5 feature goals should produce user-visible, editor-visible, Unity-visible, playable or exportable progress.
- Every 5-8 goals should include quality consolidation or release-risk review.
- Split source files before they exceed source-health limits; do not wait for a P0 source-format repair.
- Use Goal089 tiered validation. `check-current-goal.ps1` is the ordinary route, `check-spine-fast.ps1` is the visual/world/gameplay spine route, and full/observed full validation is for consolidation, milestone and release-like risk.
- Do not ask the user to manually run `check-all`; choose and run the required validation tier in the goal.
- Keep one final manual gate per goal unless there is a real blocker/crash/schema/runtime exception.

## Composite Goal Shape

An ordinary future feature goal should usually include:

1. A small contract/model/validator or reuse of an existing one.
2. A bounded Application service/use-case over real inputs.
3. Deterministic compact evidence under one current goal root.
4. Focused tests scaled to risk.
5. Product/editor/player smoke when the goal claims visible behavior.
6. State docs and artifact-scope sync after the final evidence state.

## When To Split

Split a goal when:

- more than one independent artifact family is required;
- more than 8-10 implementation files must change without an explicit bounded exception;
- a public schema, provider, Runtime, Unity or Lua unlock is needed but not explicitly allowed;
- legal/licensing/release risk blocks honest implementation;
- the goal cannot prove its claimed product outcome inside one bounded path.

## Anti-Patterns

- adding a new report without a downstream decision or product-visible use;
- adding another wrapper around a not-yet-playable pipeline;
- broad refactoring instead of reusing a local seam;
- creating a tiny proof-only service when the existing editor/export/player surface can consume the result;
- starting the next goal before the current manual gate is accepted.
