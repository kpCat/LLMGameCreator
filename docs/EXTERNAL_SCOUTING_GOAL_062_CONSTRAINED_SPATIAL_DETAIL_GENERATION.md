# External scouting — Goal 062 Constrained Spatial Detail Generation

## Purpose

Goal 062 uses the recent mxgmn scouting as reference material for a bounded in-house spatial-detail layer. The goal should not add external dependencies or copy external sample assets.

## Current repository context

Goal 061 produced a full campaign playable review package RC with 9 package rows and Unity proof. The next step is to enrich those package rows with deterministic spatial detail that can be validated, replayed and previewed, without changing public GamePackage schema or turning the generator into a single happy-path demo.

## Scouted references

### mxgmn/WaveFunctionCollapse

URL: https://github.com/mxgmn/WaveFunctionCollapse

Useful ideas:
- local tile/bitmap similarity;
- observation/propagation cycle;
- tile adjacency constraints;
- contradiction handling;
- constrained synthesis/autocomplete from partial inputs;
- local-detail generation after semantic/world decisions.

Risk/limits:
- WFC can hit contradictions.
- It should be budgeted and bounded.
- It should not own global world semantics, quests, factions or economy.
- sample images/tilesets must not be imported without provenance/license review.

Decision for Goal 062:
- Do not add dependency.
- Do not copy mxgmn code.
- Implement a tiny BCL-only, domain-specific constrained tile/detail planner inspired by the model.
- Keep a future optional adapter slot for WFC/DeBroglie after separate review.

### mxgmn/MarkovJunior

URL: https://github.com/mxgmn/MarkovJunior

Useful ideas:
- probabilistic rewrite rules;
- constraint propagation;
- grammar-like spatial transforms;
- repair passes over grids/voxels.

Risk/limits:
- full language adoption is too large for the current goal;
- the goal needs deterministic, explainable, JSON-friendly records;
- no external execution language should become a hidden runtime authority.

Decision for Goal 062:
- Do not add dependency.
- Implement a small in-house spatial rewrite rule record model:
  - match tags;
  - placement/rewrite effect;
  - priority;
  - profile/family applicability;
  - deterministic order;
  - diagnostics.

### mxgmn/TextureSynthesis

URL: https://github.com/mxgmn/TextureSynthesis

Useful ideas:
- texture synthesis from examples;
- large texture/detail variation;
- resynthesis-style local material patch generation;
- possible future tile/material fixture expansion.

License note:
- The user checked `SynTex.cs` and found embedded MIT license text for the code.
- Still treat samples/input images/assets separately; code license does not automatically clear sample asset provenance.

Decision for Goal 062:
- Do not add dependency.
- Use only conceptual inspiration.
- If Goal 062 writes PNG thumbnails/patch previews, generate them in-house from deterministic tile data and BCL-only writers.

### DeBroglie

URL: https://github.com/BorisTheBrave/DeBroglie

Useful ideas:
- C# WFC library;
- non-local constraints;
- backtracking support;
- 2D/hex/3D support.

Decision for Goal 062:
- Do not add dependency.
- Keep as later optional adapter candidate if in-house BCL planner proves insufficient.

## Goal 062 dependency policy

No new NuGet package.

No external source copy.

No external sample assets.

Allowed implementation style:
- BCL-only Application seam;
- deterministic local-grid / chunk-detail model;
- small in-house constraint/rewrite planner;
- JSON evidence;
- generated fixture thumbnails if possible with existing/BCL-only utilities.

Forbidden:
- changing public GamePackage schema;
- broad Runtime/UI/Unity refactor;
- provider/LLM/RAG calls;
- media generation/provider calls;
- arbitrary Lua execution;
- importing external tiles/images;
- dependency adoption.
