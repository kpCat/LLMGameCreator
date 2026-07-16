# Goal161Q standalone self-check diagnosis and qualification closure

Status: `BLOCKED_PLAYER_PAYLOAD_PATH_LENGTH_260`

Goal161Q adds no human gate and requests no manual testing. `goal161qAccepted=false`, `goal161qManualReviewRequired=false`, `goal161qManualGateReady=false`.

The exact failed Goal161 output was recovered offline. Its five payload files, build manifest and smoke marker were hashed. All 12 cached-host-equivalent checks plus actual package SHA pass, and the exact legacy regex parser extracts all 5 frames and all 62 human facts. Unity source was not changed.

Application now validates the assembled staging output before publication or process start. Every failure has a stable named code; a failed preflight removes staging and preserves the prior output. `RunSmoke` passes a short confined `-logFile` and returns exit code, marker text, Player.log presence, bounded sanitized relevant lines and a named failure.

The one new hidden smoke budget was consumed with corrective retry count zero. Cache `6af4d5eb5b42f956110555b58fb4e276` was reused, host rebuild=false and Unity Editor starts=0. Preflight was GREEN, but player exit code was 2 and the marker was `LLMGC_PROJECT_STANDALONE_SMOKE_FAIL`. The confined Player.log proves `standalone.player.payload_path_unreadable`: `player-adapter-model.json` is exactly 260 characters long and cannot be opened by the cached player.

RC CURRENT, portable all-selectable and portable core-only were blocked before execution. Core-only did not claim false RC readiness. Goal160's audit blocker is not closed. Next action requires a new explicit task and budget for a short confined Application smoke path; no Unity/bootstrap change is justified.
