# External Technology Scouting - Semantic Catalog Candidate

Subsystem: semantic catalog inputs  
Candidate id: `candidate_semantic_catalog_v1`  
Base accepted gate: `modular_generator_kernel_parallel_readiness_verification passed`  
Date: 2026-06-29  
Agent: Codex  
Final candidate target status: `candidate_ready_for_serial_adoption`

## Search Scope

- Libraries: ConceptNet API/dataset, Open English WordNet tooling and formats.
- Datasets: ConceptNet commonsense graph, Open English WordNet lexical network.
- Algorithms: lexical relation extraction, synonym/hypernym narrowing, concept-link filtering.
- File formats: ConceptNet JSON-LD/API edges, Global WordNet LMF XML, OEWN WNDB/RDF/XML downloads, repo-local `semantic_pack_v1` JSON.
- Unity packages: none; semantic catalog import is editor-time data preparation only.
- Existing .NET packages: none accepted by this candidate.
- Existing repo-local helpers: `SemanticCatalogService`, `SemanticCatalogModels`, `SemanticGenerationContextPreviewService`, `semantic_pack_contract_v1`, semantic foundation smoke/tests.

## Candidates Reviewed

| Candidate | Type | License | Runtime dependency? | Offline usable? | Deterministic? | Decision | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ConceptNet | Commonsense graph/API/dataset | CC BY-SA 4.0 for ConceptNet 5 data; individual API edges expose license metadata such as `cc:by-sa/4.0` | No, rejected as live runtime API | Yes, but only as a reviewed subset | Yes if a fixed snapshot/subset is imported | `reference_only` | Useful relation vocabulary and commonsense links, but ShareAlike/redistribution risk is high for generated-game catalog payloads. Do not import by default. |
| Open English WordNet | Lexical network/dataset | Derived from Princeton WordNet, further developed under CC-BY 4.0; attribution to Princeton WordNet and OEWN team required | No | Yes via XML/RDF/WNDB/downloaded snapshot | Yes with pinned release/input hash | `reference_only` | Useful for synonym, hypernym and lexical family candidates. Better license fit than ConceptNet, but still requires attribution/provenance and filtering. |
| Repo-local semantic helpers | Existing Application-layer catalog services and tests | Project-local | No | Yes | Yes | `reference_only` | Existing `semantic_pack_v1` already has layer quarantine, supported term kinds, deterministic ordering and no GamePackage schema dependency. |
| Offline curated JSON/YAML catalog | Manually curated internal catalog/source overlay | Project-owned for authored data; imported entries retain source license metadata | No | Yes | Yes with sorted ids and content hashes | `adapt_behind_adapter` | Recommended boundary: curated subset/reference design, imported as `imported_candidate` and reviewed before active use. |

## Source Notes

ConceptNet:

- Primary site: https://conceptnet.io
- API/license metadata: https://github.com/commonsense/conceptnet5/wiki/API
- The site identifies ConceptNet 5 as CC BY-SA 4.0 and recommends attribution to the Commonsense Computing Initiative.
- API edges expose source and license metadata. A future importer should preserve edge ids, sources, relation labels, license ids and snapshot/version metadata.

Open English WordNet:

- Project site: https://en-word.net/
- GitHub: https://github.com/globalwordnet/english-wordnet
- License file: https://github.com/globalwordnet/english-wordnet/blob/main/LICENSE.md
- Format reference: https://globalwordnet.github.io/schemas/
- OEWN publishes XML/RDF/WNDB downloads and documents CC-BY 4.0 plus Princeton WordNet attribution requirements.

Repo-local evidence:

- `docs/SEMANTIC_PACK_AND_RAG_STRATEGY.md`
- `docs/SEMANTIC_PACK_CONTRACT_V1.md`
- `docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md`
- `src/LLMGameCreator.Application/Design/Semantics/SemanticCatalogService.cs`
- `src/LLMGameCreator.Application/Design/Semantics/SemanticCatalogModels.cs`
- `tests/LLMGameCreator.Tests/Application/Semantics/SemanticCatalogServiceTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/SemanticCatalogFoundationSmokeTests.cs`

## Accepted, Adapted, Reference, Rejected

- Accepted: no external dependency accepted in this candidate.
- Adapted behind adapter: offline curated JSON/YAML catalog boundary, with imported external entries stored as quarantined candidates.
- Used as reference only: ConceptNet, Open English WordNet, existing repo-local semantic helpers.
- Rejected: live runtime API use, live RAG dependency, provider/LLM calls, large dataset import into this lane.
- Deferred: real ConceptNet/OEWN import adapter code, legal review of redistributed external subsets, UI review workflow, module manifest adoption.

## Adapter Boundary

- LLMGameCreator contract: `semantic_catalog_candidate_v1`, producing reviewable `semantic_pack_v1`-compatible candidate layers.
- Adapter name: future `SemanticCatalogImportAdapter` under candidate/adoption-owned source, not runtime.
- External dependency boundary: importer reads a pinned offline source snapshot and emits normalized candidate JSON with attribution metadata.
- Replacement plan: a curated project-owned JSON catalog can replace external imports at any time because generators consume the internal semantic catalog contract, not ConceptNet/OEWN structures.

## Risk Notes

- License/attribution: ConceptNet ShareAlike can contaminate redistributed compiled catalogs if copied into outputs; OEWN is simpler but still attribution-bound and derived from Princeton WordNet.
- Runtime footprint: no runtime footprint in this candidate; all import work remains editor-time/offline.
- Build impact: no `.csproj`, `.sln` or package dependency changes.
- Testability: future adapter tests should pin tiny fixtures, source hashes, and expected normalized candidate output.
- Determinism: every imported subset must declare source name, source version, source URL, retrieval date, import rule version, input hash and output hash.
- Maintenance: external datasets change; imports must be reproducible from pinned snapshots rather than live queries.
- Security: no live network calls during generation or runtime; no arbitrary file paths in source metadata.
- Paid/proprietary/API dependency: none accepted.

## Technology Decisions

### ConceptNet

Decision: `reference_only`.

ConceptNet is useful for relation ideas such as `IsA`, `PartOf`, `UsedFor`, `CapableOf`, `AtLocation`, `RelatedTo` and multilingual concept links. The candidate should not ingest ConceptNet data by default because CC BY-SA introduces adoption and redistribution questions. A serial adoption task may later permit a tiny offline imported-candidate subset only after attribution and ShareAlike impact are reviewed.

### Open English WordNet

Decision: `reference_only`.

OEWN is suitable as a future source for synonym sets, hypernyms and lexical families because it is downloadable and versionable. The candidate should still keep OEWN behind an editor-time adapter, preserve attribution to Princeton WordNet and the OEWN team, and map terms into game-useful semantic kinds instead of exposing raw synsets directly to generators.

### Repo-local Semantic Helpers

Decision: `reference_only`.

The existing local pattern already compiles `semantic_pack_v1` into deterministic catalog sidecars, treats unknown terms as candidates, avoids GamePackage schema changes, and produces preview context without model calls. This candidate should reuse that boundary rather than inventing a parallel live semantic engine.

### Offline Curated Catalog

Decision: `adapt_behind_adapter`.

The recommended fallback is a compact, manually curated JSON/YAML catalog. It can include authored project/core/genre entries and quarantined imported candidates, but active generation should consume only validated known terms after review.

## Conclusion

The candidate should proceed as an offline/editor-time semantic catalog boundary. It should not add a runtime API dependency, live RAG, live LLM/provider calls, large imported datasets, public GamePackage schema changes, shared kernel changes, or state-doc updates. The safest serial adoption path is to start with a project-owned curated subset and keep ConceptNet/OEWN imports as optional reference/adapters with explicit attribution and license metadata.
