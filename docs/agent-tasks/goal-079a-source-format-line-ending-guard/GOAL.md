/goal

Repo URL:
https://github.com/kpCat/LLMGameCreator

Working copy:
C:\Users\endim\LLMGameCreator\

Branch:
main

Goal ID / name:
Goal 079A — Source Formatting Line Ending Repair & Guard Backstop

Codex reasoning:
very high

Primary objective:
Fix the P0 source-format regression found after Goal 079: several newly touched C# files appear in GitHub/raw as one physical line because the files are stored with CR-only/no-LF line endings or equivalent raw source-format debt, while the Goal 079 quality scan still reported minifiedSourceFileCount=0 and normal line counts. Repair the affected source formatting and strengthen the guard so CR-only/no-LF/one-physical-line files cannot pass again.

Why this is P0:
The project had a previous source-format P0 after Goal 072, and Goal 073 closed it. Goal 079 reintroduced a variant: local .NET line readers may see logical lines, but GitHub/raw renders affected files as one line. This makes review and future patching dangerous, so do not start a new feature goal until this is fixed.

Read first:
Follow read-first.md before editing.

Allowed files / areas:
Follow allowed-files.md exactly.

Forbidden files / areas:
Follow forbidden-files.md exactly.

Exact behavior:

1. Preflight and confirmation
   - Confirm current branch is main.
   - Fetch/check origin/main and record current top commit.
   - Confirm Goal 079 exists in history and Goal 079 artifacts are GREEN, accepted=false.
   - Confirm the raw-byte source-format issue locally, not just from the report:
     - scan C# files as bytes;
     - count LF bytes and CR bytes;
     - detect files with zero LF bytes and nonzero CR bytes;
     - detect files whose raw physical line count by LF is <= 3 while file size is large;
     - detect raw physical lines > 500 chars;
     - compare with logical lines split by CRLF/LF/CR.
   - At minimum inspect:
     - src/LLMGameCreator.Application/Design/EditDrivenSpineQualityConsolidation/*.cs
     - src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs
     - src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignEditDrivenSpineQualityControl.cs
     - src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignEditDrivenSpineQualityControl.Designer.cs
     - tests/LLMGameCreator.Tests/Application/EditDrivenSpineQualityConsolidation/*.cs
     - tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenSpineQualityConsolidationProductSmokeTests.cs
   - Also scan the Goal 074-078 edit-driven files from allowed-files.md because the same line-ending bug may exist there.

2. Repair source formatting
   - Normalize affected C# files to normal repository-readable line endings, preferably CRLF on Windows or LF if that is the repo convention, but never CR-only.
   - Preserve code semantics.
   - Do not rewrite architecture, public APIs, or behavior except where required for the stricter scanner/test additions.
   - Keep readable indentation. Do not produce minified or one-line code.
   - Explicitly repair all affected Goal 079 source files, including any file that GitHub/raw or byte scan would treat as one physical line.
   - If the same CR-only/no-LF issue is found in Goal 074-078 edit-driven C# files within allowed scope, repair those as formatting-only changes in the same hotfix.

3. Strengthen the guard
   - Update the Goal 079 source-health/quality scanner, or add a narrow helper used by it, so it scans raw bytes in addition to logical lines.
   - The guard must fail if any scanned C# file has:
     - zero LF bytes while containing CR bytes;
     - CR-only line endings;
     - raw physical line count by LF <= 3 for a large C# file;
     - raw physical line length > 500;
     - logical max line length > 500;
     - minified/one-line source shape;
     - files over 1000 logical lines.
   - The quality report should include explicit metrics such as:
     - zeroLfSourceFileCount;
     - crOnlySourceFileCount;
     - rawPhysicalMaxLineLength;
     - rawPhysicalOneLineSourceFileCount;
     - logicalMaxLineLength;
     - minifiedSourceFileCount.
   - Do not merely alter the report text. The scanner/test must fail on a synthetic CR-only/no-LF source sample.

4. Tests
   - Add or update focused tests in tests/LLMGameCreator.Tests/Application/EditDrivenSpineQualityConsolidation/ proving:
     - a synthetic CR-only C# file is rejected;
     - a synthetic zero-LF/one-physical-line C# file is rejected;
     - current repo scanned files pass after normalization;
     - Goal 079 workspace binding remains intact.
   - Update product smoke if needed to assert the new metrics and ensure report-only success cannot pass.
   - Tests must not leave scratch files in tracked artifact directories.

5. Evidence and docs
   - Regenerate Goal 079 evidence if the strengthened scanner changes Goal 079 quality artifacts. Keep Goal 079 accepted=false.
   - Add Goal 079A-specific evidence under:
     .llmgc/procedural/goal-079a-source-format-line-ending-guard/
   - Goal 079A report must be GREEN only if:
     - all scanned C# files have normal LF-containing line endings;
     - no one-line/minified C# file remains in the allowed scan scope;
     - scanner/tests catch synthetic CR-only/no-LF source.
   - Update the current-state docs quartet to record that Goal 079A repaired a P0 source-format line-ending regression. Do not mark Goal 079 accepted.
   - Update the debt register: remove/resolve the P0 entry if present, or add a resolved P0 note. Preserve P2/P3 debts like AlphaRuntimeBootstrap size and long-but-below-limit services.
   - Update artifact-scope policy with scenario goal-079a-source-format-line-ending-guard.

6. Quality requirements
   - No new minified/one-line .cs files.
   - No CR-only/no-LF C# files in the scanned scope.
   - max logical C# line length <= 500.
   - max raw LF physical line length <= 500 after normalization.
   - no new C# file over 1000 logical lines.
   - AlphaRuntimeBootstrap.cs must remain unchanged/read-only.
   - CampaignAuthoringReviewWorkspacePageControl.cs must remain bounded; do not fold child controls into the parent.
   - Do not touch forbidden areas.
   - No absolute local paths, timestamps, heavy logs, or scratch/tamper files in tracked evidence.
   - No mojibake markers in changed files.

Validation commands:
Follow validation.md.

Stop / block conditions:
- BLOCKED if the source-format issue cannot be repaired without touching forbidden areas.
- BLOCKED if scanner strengthening requires broad refactor or external dependencies.
- BLOCKED if there are remaining CR-only/no-LF C# files in the required scan scope and they cannot be fixed safely.
- FAILED if build/tests regress due to this hotfix and cannot be repaired inside allowed files.
- Do not mark Goal 079 accepted/passed.

Final report format:
Follow final-report-format.md.

Mandatory commit/push policy:
Always commit and push to origin/main even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:
- GREEN Goal 079A source format line ending guard
- BLOCKED Goal 079A source format line ending guard
- FAILED Goal 079A source format line ending guard
