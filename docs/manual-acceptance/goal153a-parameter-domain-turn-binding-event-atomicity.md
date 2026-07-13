# Goal 153/153A/153B/153C combined manual gate

Status: automated implementation GREEN; independent Goal153C audit required; no Goal is accepted
Accepted: false
Accepted by human: false
Accepted by Codex: false
Manual review performed: false

Short combined manual gate after independent audit:

1. Enable «Активные способности», «Мана и заклинания» and «Эффекты по ходам» and set `2/12/3/5/1`.
2. Build and verify one GREEN card.
3. Confirm the readable summary reports damage 2, mana `12 → 9`, five ticks of 1 and expiry on the real hostile target.
4. Save/reopen and confirm the five values remain.
5. Launch the cached standalone and confirm the same readable facts.

Proof-fixture absence, health `30/0/30`, lethal terminal outcomes, conditional skip/replay, event atomicity and duration-1000 planning are automated facts. No hash or raw module-ID inspection is required from the human reviewer.
