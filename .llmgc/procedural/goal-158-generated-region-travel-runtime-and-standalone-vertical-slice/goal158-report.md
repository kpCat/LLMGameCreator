# Goal 158 generated region travel Runtime and standalone vertical slice

Status: GREEN_ACCEPTABLE_CANDIDATE

- Goal157 independent audit intake is GREEN at 8939aea0, with no human gate.
- Runtime now executes a generic, atomic, data-driven map transition through PlayerCommand.Interact and emits additive MapChanged while legacy interactions remain unchanged.
- Strict regenerated-plan provenance binds every region to an exact generated map. Lane B adds one deterministic gate per directed connection, generated start and project identity; Lane A accepted mechanics/social remain unchanged.
- The primary Runtime route interacts in the origin, follows a deterministic data-driven path through at least one generated connection, enters the generated destination and interacts there. Replay and map-state roundtrip are exact.
- History/UI distinguish START_CURRENT from TRAVEL_CURRENT. A cached hidden standalone smoke ran exactly once, reused its host, rebuilt nothing, started Unity zero times, correlated travel/accepted facts and RC CURRENT, and restored a portable copy without execution.
- Goal158 and the bounded Goal157/156/155A/155/154D/153C/150/149 plus Runtime/procedural/workspace/standalone regressions are GREEN. Full suite, 85-case closure, all-ProductSmoke and Unity host build were not run.
