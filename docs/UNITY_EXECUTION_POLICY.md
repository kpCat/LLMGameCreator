# Unity execution policy

This policy applies to every Goal that changes Unity-hosted work.

1. The default Unity execution budget for an ordinary Goal is 0.
2. Unity may run only when GOAL.md explicitly authorizes Unity-host changes or a real Unity proof.
3. Project standalone assembly uses `ProjectStandaloneBuildService` and `StandaloneHostCache`.
4. Direct ad-hoc `Unity.exe` experiments are forbidden.
5. Validate the cache key and cache manifest before every Unity invocation.
6. A valid cache is reused for payload/project changes without launching the Unity Editor.
7. A Unity Windows player is atomic: EXE, matching `_Data`, `UnityPlayer.dll`, `MonoBleedingEdge` and build-manifest files travel together.
8. Never rename or copy an EXE independently from its matching `_Data` and runtime files.
9. Automated proof roots use short LocalAppData paths.
10. Automated standalone smoke runs hidden and headless.
11. Never run more than one Unity build process simultaneously.
12. Never repeat an unchanged failed Unity command.
13. Rerun only after a concrete diagnosis and code/configuration change.
14. Per authorized Goal, allow one planned host build and at most one corrective retry.
15. Historical proof tests may not launch Unity to reconstruct old evidence.
16. Raw Unity logs remain ignored under `.devflow/runs` or LocalAppData.
17. Future GOAL.md files state the Unity invocation budget explicitly.
