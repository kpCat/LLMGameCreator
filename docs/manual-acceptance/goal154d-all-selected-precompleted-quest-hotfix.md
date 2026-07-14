# Goal154D all-selected precompleted quest hotfix

Status: GREEN implementation; combined human gate retry ready.
Accepted: false
Accepted by human: false
Accepted by Codex: false
Manual review performed: false
Manual gate ready: true

The failed attempt at `2c95ee8f689ef104946859432706fd6d4b22deb2` used 22 selected mechanics and 10 configured parameters. Alchemy Focus supplied 4 starting herbs, so the 3-herb quest completed during `start_or_update_quest`; the redundant capability advance then failed with strict Runtime `quest.not_active`.

Goal154D keeps every selected profile, preserves direct Runtime strictness, and truthfully skips only the redundant qualification action after completed quest/objective plus prior completion/reward evidence. Automated proof covers 2/3/4/20 herbs, both completion snapshots, exact build/repeat/reopen, checkpoint/full replay, source immutability and cached standalone reuse.

## Four-step retry

1. In the already configured `goal148-manual` project, press “Собрать и проверить игру”.
2. Confirm the social card shows reputation `0 → 10` and gold `0 → 10 → 17`.
3. Save, close/reopen and confirm the values and card remain.
4. Build/launch the cached standalone and confirm the same social facts.

Do not disable Alchemy Focus or any other profile. No source, journal, event or hash inspection is required from the owner.
