# Goal 153/153A/153B/153C combined manual gate

Status: accepted by human
Accepted: true
Accepted by human: true
Accepted by Codex: false
Manual review performed: true

The owner completed the combined review with values `2/12/3/5/1`: damage 2, mana `12 → 9`, five ticks of 1, expiry, saved/reopened values and cached standalone presentation were accepted at `ad2e404f1c938113a0c111d4c1fe1bfb55e0e836`.

Historical combined manual gate:

1. Enable «Активные способности», «Мана и заклинания» and «Эффекты по ходам» and set `2/12/3/5/1`.
2. Build and verify one GREEN card.
3. Confirm the readable summary reports damage 2, mana `12 → 9`, five ticks of 1 and expiry on the real hostile target.
4. Save/reopen and confirm the five values remain.
5. Launch the cached standalone and confirm the same readable facts.

Proof-fixture absence, health `30/0/30`, lethal terminal outcomes, conditional skip/replay, event atomicity and duration-1000 planning are automated facts. No hash or raw module-ID inspection is required from the human reviewer.
