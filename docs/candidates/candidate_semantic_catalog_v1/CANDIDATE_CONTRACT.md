# Candidate Contract - Semantic Catalog v1

Candidate id: `candidate_semantic_catalog_v1`  
Base accepted gate: `modular_generator_kernel_parallel_readiness_verification passed`  
Final candidate status: `candidate_ready_for_serial_adoption`  
Scope: candidate-only documentation for semantic catalog inputs and offline import boundaries.

## Purpose

This candidate defines a low-conflict boundary for semantic catalog inputs that can help generation with lexical relations, tags, synonyms, hypernyms, archetype families and concept links without turning generation or runtime into a live RAG/provider/API dependency.

The candidate is not an accepted goal and does not claim an accepted gate.

## Relationship To Existing Contracts

Reuse the existing semantic spine:

- `semantic_pack_v1` remains the authoring/import candidate contract.
- Existing semantic catalog compilation remains deterministic and editor-side.
- Candidate/imported entries stay quarantined until reviewed.
- Runtime consumes generated package/runtime data, not raw external semantic data.
- GamePackage public schema is unchanged.

This candidate does not change `SemanticCatalogService` or shared kernel files. Serial adoption can later decide whether to add candidate-owned adapter code under a bounded source root.

## Internal Semantic Catalog Format

Recommended internal candidate record shape:

```json
{
  "schemaVersion": "semantic_catalog_candidate_v1",
  "catalogId": "candidate_semantic_catalog_v1/core_reference",
  "catalogVersion": "0.1.0",
  "sourceSet": {
    "sourceSetId": "manual/core-reference-0.1.0",
    "createdBy": "manual",
    "sourceLicense": "project-owned",
    "attributionRequired": false,
    "sourceVersion": "0.1.0",
    "sourceUrl": "",
    "inputHash": "",
    "importRuleVersion": "semantic-catalog-import-rules/0.1.0"
  },
  "terms": [
    {
      "termId": "npc_archetype/vendor",
      "kind": "npc_archetype",
      "label": "Vendor",
      "status": "known",
      "aliases": ["merchant", "trader"],
      "tags": ["economy", "social"],
      "sourceRefs": ["manual/core-reference-0.1.0"],
      "generationHints": ["prefer trade dialogue and transaction hooks"],
      "constraints": []
    }
  ],
  "relations": [
    {
      "relationId": "relation/npc_archetype/vendor/prefers_dialogue_intent/dialogue_intent/bargain",
      "sourceTermId": "npc_archetype/vendor",
      "relationKind": "prefers_dialogue_intent",
      "targetTermId": "dialogue_intent/bargain",
      "status": "known",
      "sourceRefs": ["manual/core-reference-0.1.0"]
    }
  ],
  "diagnostics": []
}
```

Field rules:

- `termId`, `relationId`, source ids and source refs use lowercase slash ids with safe segments.
- `kind` maps to existing or serially adopted semantic kinds.
- `status` uses existing status vocabulary: `known`, `candidate`, `deprecated`, `conflict`, `invalid`.
- `sourceRefs` are required for imported or externally influenced entries.
- `generationHints` are optional plain data; they are not executable prompts or code.

## Source Attribution Metadata

Every source set should record:

- source name;
- source URL;
- source version or release tag;
- retrieval date for imported snapshots;
- license id and license URL;
- attribution text or attribution pointer;
- whether ShareAlike, non-commercial, no-derivatives or other redistribution restrictions apply;
- input file hash;
- import rule version;
- output hash after normalization.

ConceptNet-specific imported candidates, if ever allowed by serial adoption, must preserve edge-level source and license metadata. ConceptNet-derived active known terms should not be redistributed until ShareAlike impact is reviewed.

Open English WordNet (OEWN)-specific imported candidates, if ever allowed by serial adoption, must preserve OEWN and Princeton WordNet attribution metadata and source release/version.

## Import Pipeline Boundary

Recommended offline pipeline:

```text
Pinned external snapshot or manual curated file
-> editor-time import adapter
-> normalized semantic_catalog_candidate_v1
-> validation and diagnostics
-> imported_candidate or project layer
-> review/promotion
-> compiled semantic catalog
-> deterministic generation
```

Forbidden pipeline:

```text
runtime/generator
-> live ConceptNet/OEWN/API/RAG query
-> unreviewed generated package content
```

The adapter is replaceable. Generators must depend on the internal compiled semantic catalog, not the external dataset format.

## Offline Subset And Versioning Approach

Default subset policy:

- Start with a small manual/core curated catalog.
- Add genre or project overlays only when needed by a product outcome.
- Keep external imports as `imported_candidate` until reviewed.
- Prefer source-specific tiny fixtures in tests over full external datasets.
- Do not commit large ConceptNet/OEWN snapshots into the repository without explicit permission.

Deterministic versioning:

- Sort terms by `termId` and relations by `relationId`.
- Normalize aliases/tags case-insensitively while preserving display labels.
- Exclude timestamps, machine names and absolute paths from generated artifacts.
- Record source snapshot hash and import rule version.
- Require byte-stable output for the same input snapshot and rule version.

## Absence Behavior

If no semantic catalog is present:

- generation uses existing seed/default terms where available;
- unknown author terms become `candidate` instead of crashing;
- semantic-guided features produce diagnostics such as `semantic_catalog.absent_optional`;
- no live network fallback is attempted;
- runtime behavior remains independent from the semantic catalog.

If an optional external source is unavailable:

- mark source as `absent_optional`;
- preserve manual/project terms;
- do not silently switch to live API calls;
- do not fake imported evidence.

## Validation Strategy

Candidate validation should reject:

- unsafe ids, rooted paths or traversal paths;
- unsupported status values;
- unsupported relation kinds unless a serial contract change adopts them;
- relation endpoints missing from the active or candidate term set;
- imported entries without source refs and license metadata;
- ConceptNet-derived active known entries without ShareAlike review status;
- OEWN-derived active known entries without required attribution metadata;
- nondeterministic output ordering or volatile timestamps;
- claims that the candidate is production-integrated or accepted.

Focused future tests, if code is adopted:

- deterministic normalization from a tiny manual curated fixture;
- quarantined `imported_candidate` behavior for external entries;
- ConceptNet ShareAlike risk diagnostic on copied external entries;
- OEWN attribution diagnostic when attribution metadata is missing;
- absence behavior when external snapshots are missing.

## Recommended Serial Adoption Shape

Serial adoption can safely start with:

- candidate-owned docs;
- a tiny project-owned curated catalog fixture;
- candidate-owned adapter/validator code only if a source/project file change is explicitly allowed;
- focused candidate tests;
- optional product-smoke manifest only if candidate code writes compact proof artifacts.

Serial adoption should not start with:

- large external dataset imports;
- live ConceptNet/OEWN API use;
- runtime dependencies;
- RAG/provider/LLM calls;
- public GamePackage schema changes;
- shared kernel or state-doc updates from the candidate lane.

## Final Candidate Decision

Decision: ready for serial adoption review as a docs-first candidate.

Final status:

```text
candidate_ready_for_serial_adoption
```

No accepted manual gate is claimed.
