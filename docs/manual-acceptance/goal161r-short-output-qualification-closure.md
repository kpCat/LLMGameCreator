# Goal161R short output qualification closure

Status: `BLOCKED_PUBLICATION_AFTER_GREEN_SHORT_PATH_SMOKE`; `accepted=false`; no human gate.

Superseded for publication mechanics by Goal161S: immutable `runs/r-*` and atomic `current.json` reached GREEN at the player/output layer, then the returned controller result was `release_candidate_record: rc.payload.missing`. Retry remains zero.

Goal161R implements the operational output root `%LOCALAPPDATA%/LGC/O/<project-token>/current` with fixed `g.exe`/`g_Data`, deterministic path-derived token, short staging/backup siblings and a hard 240-character player-path budget. The old project-local `Builds/Windows` output is untouched.

The exact one authorized hidden player smoke ran from short staging after completeness, path-budget and 13/13 payload/legacy-parser preflight. It reused cache `6af4d5eb5b42f956110555b58fb4e276`, rebuilt no host, started Unity zero times, exited 0, wrote all five required markers and captured Player.log. This closes the proved 260-character player-read root cause.

The containing build returned FAILED during publication and rolled staging back. The failed fixture did not persist the returned publication diagnostic, so `current`, RC CURRENT and the portable all-selectable/core-only post-publication assertions are not claimed. Retry is zero and no player or Unity rerun is authorized. The next action is independent publication diagnosis only.
