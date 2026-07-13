# Goal 154B executable social Runtime core report

Status: GREEN

- Behavioral Runtime tests: 29/29; historical Goal154A source/reflection tests: 18/18, not counted as lifecycle proof.
- Default 0/10/5/10/7: reputation 0 to 10, quest completed, choice unavailable to available to unavailable, gold 0 to 7, claim flag true, outcome claimed.
- Threshold 20: claim SKIPPED, state and events unchanged at the claim boundary, outcome still_locked.
- Direct second claim: rejected atomically with no reward, flag, or dialogue success event.
- Clamps: 95 + 10 = 100 with actual delta 5; -95 - 10 = -100 with actual delta -5.
- Four quest/dialogue rollback scenarios are byte-identical and leak no success events; numeric event args are invariant-culture.
- Claimed final hash: bb04b1e2ab2244bd22f0f68d2519a6376219e28ec06f6f65e5b81381edcc33f9; still-locked final hash: 68477d83101b858ad07fa7ee5e7e11808869fe290cad5a9772199a5c22a31454.
- Checkpoint continuation and full replay hashes/events are equivalent for both outcomes.
- Activated package contains only classified edits to existing faction, quest, dialogue, gold reward, resource and flag contracts; proof fixtures: 0; default-off hashes preserved.
- Goal154, Goal154A and Goal154B remain unaccepted; manualGateReady=false.
- WinForms, real saved-project and standalone closure are deferredTo=Goal154C.
- Unity Editor invocations: 0; standalone smoke invocations: 0.
