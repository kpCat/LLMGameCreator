# Goal160 sealed regeneration commit and generated-world history rollback

Status: `GREEN_ACCEPTABLE_CANDIDATE`

Goal159 independent audit is `BLOCKED_AT_C7788E1E` on `regeneration_commit_not_sealed_inside_shared_operation_and_semantic_rollback_boundary`. Goal160 closes this P1 without replacing the Goal159 product foundation.

Build, standalone, authoring mutation, regeneration, history rollback and recovery share one operation coordinator. Its cross-process project lock is held for the complete mutation. Candidate Preview writes an immutable seal over source, generation, package, authoring, identity, selected build history, support files, qualification hashes and typed diff. Apply accepts only the cached `AttemptId` and seal. Caller-modified preview or candidate package/authoring/identity/history/support tamper is rejected before mutation.

The transaction receives expected truth tokens and authoritative inventory, recaptures them after the shared lock and before backups, enters journal state `validating`, then performs semantic source/package/authoring/identity/history/RC/world-change validation before `committed` and cleanup. Semantic failure and validating-state crash restore exact before hashes. A later presentation reopen diagnostic cannot falsely claim rollback of an already committed transaction.

Generated-world history stores only generation source and deterministic sidecars. Regeneration and rollback archive current/candidate worlds atomically. Restoring an earlier world copies only its historical generation into an isolated candidate and rebuilds with current selected mechanics, parameters and project identity. It builds twice, freshly reopens `TRAVEL_CURRENT`, publishes a seal and applies through the same lock/transaction/semantic validator. Historical package, authoring, identity and RC are never promoted as current truth.

Old histories remain and each apply adds exactly one GREEN build-history row. Old RC bytes remain and read `LAST_SUCCESS`/standalone pending until an ordinary standalone writes a new `CURRENT` record. The Projects UI exposes «История миров», a data-derived world list and «Проверить и восстановить». One cache-only hidden standalone smoke proves rollback-world/travel and accepted-mechanics payload facts, CURRENT RC and execution-free portable recovery with `HostReused=true`, `HostRebuilt=false` and zero Unity starts.

No user action or manual Unity review is requested. `goal160Accepted=false`, `goal160AcceptedByHuman=false`, `goal160AcceptedByCodex=false`, `goal160ManualReviewRequired=false`, `goal160ManualGateReady=false`. Independent audit is `BLOCKED_AT_D8DD05E7` on `semantic_commit_validator_requires_complete_accepted_mechanics_for_core_only_generated_projects`; Goal161 implements and tests the correction, but the blocker remains formally open because Goal161 did not reach GREEN, so `goal160IndependentAuditRequired=true`.
