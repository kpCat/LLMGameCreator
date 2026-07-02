# Goal 079A — Source Formatting Line Ending Repair & Guard Backstop

This task is a bounded P0 hotfix after Goal 079. It repairs source files that appear readable to .NET line readers but are stored in GitHub/raw as one physical line because of CR-only/no-LF line endings or equivalent source-format debt. It also strengthens the guard so this cannot be missed again.
