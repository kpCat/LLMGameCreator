# Goal169 test inventory

This inventory was fixed before production edits. Every numbered item maps one-to-one to the required Goal169 behavioral contract; the implementation may group data variants into xUnit theories, but discovery and evidence must retain every case.

## Profile neutrality

1. All-branch relationship qualifies.
2. Challenge-only zero-arc qualifies.
3. Support+Refuse without Challenge qualifies.
4. Support-only qualifies.
5. Refuse-only qualifies.
6. No-branch relationship qualifies without Runtime.
7. Unavailable Support starts zero Runtime sessions.
8. Unavailable Challenge starts zero Runtime sessions.
9. Unavailable Refuse starts zero Runtime sessions.
10. Required branch failure remains failure.
11. Support never indexes an empty arc.
12. Aggregate flags are vacuous only for unavailable branches.
13. Explicit branch matrix is persisted.
14. Old v6 all-branch row remains eligible.
15. Old v6 false or partial row is rejected.
16. New profile-neutral row does not require maximum arc length above zero.
17. Required Support row requires positive arc length.
18. Atomic rollback uses only an available branch.

## Exact effects

19. Health descriptor is accepted.
20. Stat descriptor is accepted.
21. Status descriptor is accepted.
22. Delayed status damage reaches actual victory.
23. Utility success is rejected.
24. Mixed utility-first route selects a progressing descriptor.
25. Ability-only route succeeds.
26. Descriptor effect fingerprint is exact.
27. Ability definition SHA is exact.
28. Package reference and SHA stay unchanged.
29. Repeated encounter state is rejected.
30. No-op-only route fails causally.

## Event binding and placement

31. Support event is derived from a completed arc.
32. Challenge event is derived from a challenge branch.
33. Refuse event is derived from a refuse branch.
34. Absent branch creates no event.
35. No-branch relationship creates no event.
36. RegionalEventId equals the event dialogue ID.
37. ResolutionFlagId equals the event dialogue ID.
38. Event identities are deterministic.
39. Support target region is derived from the final quest.
40. Challenge target region is derived from the encounter.
41. Refuse target region is derived from the relationship.
42. Arbitrary map size is supported.
43. Arbitrary event count is supported.
44. Selected cell is walkable.
45. Selected cell is reachable.
46. Player start, gate and blocking cells are excluded.
47. Event placements are unique.
48. Reordered input is deterministic.
49. Insufficient safe placement fails causally.
50. Event inventory is exact.

## Overlay

51. Only event records and map entities are added.
52. Existing definitions are canonical-identical.
53. Manifest and GeneratedContent are preserved.
54. Relationship, combat and travel records are preserved.
55. Event references resolve exactly.
56. Support reward derivation fingerprint is exact.
57. Challenge and Refuse events have no duplicate reputation effect.
58. Overlay is deterministic across two builds.
59. Forbidden delta is rejected.

## Runtime events

60. Support event is locked before arc completion.
61. Support event is available after the full arc.
62. Support event resolves through ordinary interaction.
63. Support event reputation delta is exact and applied once.
64. Challenge flee does not unlock its event.
65. Challenge victory unlocks its event.
66. Challenge event resolves with zero duplicate penalty.
67. Refuse unlocks its fallout event.
68. Refuse event resolves with zero duplicate penalty.
69. Resolution flag is exact.
70. Resolved event cannot resolve twice.
71. Locked route has zero mutation.
72. Malformed resolution is atomic.
73. Event replay is equivalent.
74. Map movement and interaction commands are captured.
75. Event qualification performs no direct state mutation.
76. Event consequence is state-backed.

## Projection and UI

77. Locked event row is projected.
78. Available event row is projected.
79. Resolved event row is projected.
80. Current-map marker is human-readable.
81. Other-region event is human-readable.
82. The «События мира» tab is present.
83. Primary event UI has no raw IDs, hashes or paths.
84. The campaign surface fits at 1100x720.
85. Decisions, relationships and events are consistent.
86. No-event profile shows no false row.

## History and regeneration

87. v7 regional events are current.
88. Genuine v6 projects events as pending.
89. Genuine v6 campaign is not ready.
90. One build upgrades v6.
91. v5, v4, v3 and v2 behavior is retained.
92. Event-absent v7 is valid.
93. Seal includes branch matrix and event summary, overlay and inventory.
94. Placement, prerequisite and reward tampering is rejected.
95. Regeneration restores v7 current.
96. Rollback restores v7 current.

## Save, migration and standalone

97. Exact AVAILABLE state continues.
98. Exact RESOLVED state continues.
99. Continue performs zero Runtime Start calls.
100. v6 save requires rebase.
101. Compatible event flag is preserved.
102. Incompatible event flag is dropped.
103. World migration compatibility is applied.
104. Migration creates no ghost event, flag or marker.
105. Post-migration event and travel routes work.
106. Exactly one hidden smoke runs.
107. Host is reused, not rebuilt, and Unity starts zero times.
108. Release candidate is current.
109. All-selectable portable project is current.
110. Core-only portable project has no false RC readiness.

## Regressions

111. Goal168 focused regressions pass.
112. Goal167 focused regressions pass.
113. Goal166 focused regressions pass.
114. Goal165 focused regressions pass.
115. Goal164 focused regressions pass.
116. Goal163 through Goal157 focused regressions pass.
117. GeneratedCampaign focused regressions pass.
118. GeneratedGameplaySave focused regressions pass.
119. RuntimeSimulator focused regressions pass.
