# Goal161Q report

Status: BLOCKED_PLAYER_PAYLOAD_PATH_LENGTH_260; accepted=false; no human gate.

The exact failed Goal161 output was recovered and hashed. Application structural checks, package SHA and exact cached-host regex extraction all pass: 13/13 checks, 5/5 frames and 62/62 human facts. Goal161 product/save migration behavior remains GREEN.

Application now performs named self-check preflight on staging before publish or process start. A mismatch preserves the prior output. RunSmoke passes a confined `-logFile` and returns exit code, marker, bounded sanitized Player.log lines and a named failure. Goal161Q discovery is 24 tests with 20 behavioral; all 24 pass. Goal161–157, ProjectStandaloneBuild, UnifiedGameProjectWorkspace, RuntimeSnapshotStore, capability/equipment, attributes/progression and current-state regressions are GREEN.

The one new hidden smoke was consumed with retry count zero. Preflight passed, cache `6af4d5eb5b42f956110555b58fb4e276` was reused, host rebuild=false and Unity Editor starts=0. The player returned exit code 2 and only the FAIL marker. The new Player.log proves `standalone.player.payload_path_unreadable`: the model path is exactly 260 characters and the frames path is 261.

RC CURRENT and portable all-selectable/core-only were blocked before execution, so Goal160's audit blocker remains open. No Unity/bootstrap fix was attempted. The next task requires a short confined Application smoke path and a new explicit smoke budget.
