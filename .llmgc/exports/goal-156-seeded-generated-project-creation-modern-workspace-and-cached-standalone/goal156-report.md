# Goal 156 seeded generated project creation

Status: GREEN_ACCEPTABLE_CANDIDATE

- Goal155A independent audit is GREEN and closes the Goal155 RC milestone; Goal156 remains accepted=false and requires independent audit.
- The Games to New game workflow creates seeded_generated projects atomically with seed, mode, preset and a data-driven mechanics profile; the legacy template lane remains compatible.
- Same seed/options are stable, a different seed changes visible generated world content, and all three supported modes validate.
- The immutable Goal142 baseline remains byte-identical. Generated records are namespaced and additive; differing ID collisions fail. Explicit custom-base composition uses and hash-validates the generated base while the default Goal142 lane remains unchanged.
- The all-selectable project builds, repeats and reopens GREEN with generated records, checkpoint/replay and AcceptedMechanics preserved. Core-only builds GREEN but does not claim RC readiness.
- The typed generated-world summary persists in GREEN history and is shown as one concise Generated world card.
- One hidden standalone smoke reused the existing cached host, rebuilt nothing, started Unity zero times, kept the host file set unchanged, passed all self-checks and correlated generated-world plus accepted-mechanics payload facts.
- A complete portable copy restored generated summary, AcceptedMechanics and RC CURRENT without execution. Failure/rollback checks preserved current package and last successful RC evidence.
- Focused Goal156/regression filters and the two required slice runners are GREEN. Full suite, 85-case closure, all-ProductSmoke and Unity host build were not run.
