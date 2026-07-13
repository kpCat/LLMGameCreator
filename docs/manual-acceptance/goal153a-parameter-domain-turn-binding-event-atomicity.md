# Goal 153/153A combined manual gate

Status: automated implementation GREEN; Goal153 and Goal153A are not accepted
Accepted: false
Accepted by human: false
Accepted by Codex: false
Manual review performed: false

Short combined manual gate:

1. Open the existing project, enable «Активные способности», «Мана и заклинания» and «Эффекты по ходам», and confirm all five typed settings appear.
2. Set damage/mana/cost/duration/tick damage to `100/12/3/5/50`, build, and confirm one GREEN summary reports damage 100, mana `12 → 9`, five ticks of 50 and expiry.
3. Save and reopen the project, then launch the cached standalone once and confirm the same ability/mana/status facts remain readable.
4. Accept or reject Goal153 and Goal153A together.

Turn binding, rollback event atomicity, lethal-status resolution, checkpoint/replay and the duration-1000 plan are automated facts; no extra manual technical pass is required.
