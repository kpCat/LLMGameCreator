# Goal 152 report

Status: BLOCKED

The accepted mechanics milestone is recorded. The generic cached Unity host built successfully
with Unity `6000.1.10f1` and starts from the cache. A project payload was assembled only from a
disposable `goal148-manual` copy through the normal workspace controller. The source manifest
remained byte-identical.

The required renamed `<safe-project-slug>.exe` exits `1` before the bootstrap. Unity reports that
it cannot load the adjacent byte-identical `MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll`.
The unrenamed cache host starts and reaches its expected missing-payload diagnostic, isolating the
blocker to the project-scoped executable rename/runtime contract. No historical Goal142/143/sample
fallback was used. Goal152 remains unaccepted and its manual gate is not ready.
