# Product Vertical Slice Decision

## Context

M4.1 real-model evaluation passed for sampled baseline contracts:

- game_profile_v1
- mechanics_pack_v1
- quest_pack_v1
- scene_pack_v1

This means strict generation/evaluation is stable enough to proceed, but it does not mean broad feature expansion is safe.

## Candidate slices

### Option A: Capability Composer v2 usability slice

Goal: make the current Capability Picker usable and understandable without rewriting the whole system.

Includes:
- Russian labels/help text
- details panel for selected option
- diagnostic categories: impossible / unsupported_yet / risky / info
- no new production generation path
- no schema-breaking selection artifact rewrite

Pros:
- fixes immediate user confusion
- reduces wrong selections
- creates foundation for composable modules

Cons:
- does not yet make generated artifacts become a playable package

### Option B: Baseline artifacts to package assembly slice

Goal: take accepted baseline strict artifacts and apply them into a richer GamePackage draft state.

Includes:
- read staged/accepted artifacts
- map game_profile/scene/quest/mechanics into package draft structures
- validate package
- save/export sample package

Pros:
- most product-like step
- turns generated JSON into game state
- proves editor value

Cons:
- may hit limitations in current contracts/package schema
- capability picker remains confusing

### Option C: Lua-backed content slice

Goal: use one safe Lua generator module family to produce validated artifact envelopes.

Pros:
- aligns with long-term Lua library plan
- opens generator-module architecture

Cons:
- risks returning to infrastructure before user-visible product value

## Recommended order

1. Capability Composer v2 usability slice, limited and non-breaking.
2. Baseline artifacts to package assembly slice.
3. Lua-backed content slice.
4. New artifact contract families only when needed by one of the above slices.

## Why not do everything now

Trying to implement composable capabilities, package assembly, Lua execution, runtime preview, economy, infinite world generation, and balance in one run will create a large unstable branch.

The correct acceleration strategy is not “huge task”, but “larger vertical slices with clear acceptance criteria”.
