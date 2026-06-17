# ARCHIVE_MANIFEST.md — Agent Task Pack 008

Pack id: `agent-task-pack-008-locked-m6-assembly-draft-specs-shortpaths`

Purpose:

```text
Convert M6_002..M6_008 sequence placeholders into locked draft task specs for GamePackage assembly planning without enabling M6 implementation while M4.1/M5 remain unresolved.
```

Safety:

```text
- documentation-only pack;
- no src/ changes;
- no tests/ changes;
- no .sln/.csproj changes;
- no .devflow/scripts changes;
- does not unlock M6;
- locked drafts must be refreshed before execution.
```

Files in this archive:

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/M6/000_M6_SEQUENCE.md
docs/agent-tasks/M6/M6_002_BASE_MAPPING.md
docs/agent-tasks/M6/M6_003_ITEMS_MAPPING.md
docs/agent-tasks/M6/M6_004_SCENE_MAPPING.md
docs/agent-tasks/M6/M6_005_QUEST_MAPPING.md
docs/agent-tasks/M6/M6_006_VALIDATION.md
docs/agent-tasks/M6/M6_007_REVIEW_APPLY.md
docs/agent-tasks/M6/M6_008_SAMPLE_SLICE.md
```

Path policy: this v2 archive intentionally uses short M6 task filenames to avoid Windows/zip/extractor path-length issues.
